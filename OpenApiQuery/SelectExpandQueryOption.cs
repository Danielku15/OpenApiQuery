using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenApiQuery.Parsing;
using OpenApiQuery.Utils;

namespace OpenApiQuery
{
    public class SelectExpandQueryOption
    {
        private readonly ParameterExpression _parameter;

        private readonly IDictionary<PropertyInfo, ExpandClause> _expandClauses;

        internal SelectClause RootSelectClause { get; private set; }

        public SelectExpandQueryOption(Type elementType)
        {
            _expandClauses = new Dictionary<PropertyInfo, ExpandClause>();
            _parameter = Expression.Parameter(elementType);
            RootSelectClause = new SelectClause
            {
                IsStarSelect = true
            };
        }

        public IQueryable<T> ApplyTo<T>(IQueryable<T> queryable)
        {
            var selectClause = ExpandClause.BuildSelectLambdaExpression(
                _parameter.Type,
                RootSelectClause,
                _expandClauses);
            queryable = queryable.Select((Expression<Func<T, T>>) selectClause);

            return queryable;
        }

        public void Initialize(HttpContext httpContext, ILogger logger, ModelStateDictionary modelStateDictionary)
        {
            InitializeExpand(httpContext, logger, modelStateDictionary);
            InitializeSelect(httpContext, logger, modelStateDictionary);
        }

        private void InitializeSelect(HttpContext httpContext, ILogger logger,
            ModelStateDictionary modelStateDictionary)
        {
            if (httpContext.Request.Query.TryGetValue("$select", out var values))
            {
                var binder = httpContext.RequestServices.GetRequiredService<IOpenApiTypeHandler>();
                foreach (var value in values)
                {
                    var parser = new SelectClauseParser(binder, value);
                    try
                    {
                        RootSelectClause = parser.Parse(_parameter);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "failed to parse $select clause");
                        modelStateDictionary.TryAddModelException("$select", e);
                    }
                }
            }
        }

        private void InitializeExpand(HttpContext httpContext, ILogger logger,
            ModelStateDictionary modelStateDictionary)
        {
            if (httpContext.Request.Query.TryGetValue("$expand", out var values))
            {
                var binder = httpContext.RequestServices.GetRequiredService<IOpenApiTypeHandler>();
                foreach (var value in values)
                {
                    var parser = new ExpandClauseParser(binder, value, _expandClauses, modelStateDictionary);
                    try
                    {
                        parser.Parse(_parameter);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "failed to parse $expand clause");
                        modelStateDictionary.TryAddModelException("$expand", e);
                    }
                }
            }
        }


        private class SelectClauseParser
        {
            private readonly QueryExpressionParser _parser;

            public SelectClauseParser(IOpenApiTypeHandler binder, string value)
            {
                _parser = new QueryExpressionParser(value, binder);
            }

            public SelectClause Parse(ParameterExpression it)
            {
                var rootClause = new SelectClause
                {
                    // will have sub-clauses
                    SelectClauses = new Dictionary<PropertyInfo, SelectClause>()
                };
                _parser.PushThis(it);
                SelectItemList(rootClause);
                _parser.PopThis();
                return rootClause;
            }

            private void SelectItemList(SelectClause currentClause)
            {
                while (true)
                {
                    SelectItem(currentClause);
                    if (_parser.CurrentTokenKind == QueryExpressionTokenKind.Comma)
                    {
                        _parser.NextToken();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            private void SelectItem(SelectClause currentClause)
            {
                switch (_parser.CurrentTokenKind)
                {
                    case QueryExpressionTokenKind.Star:
                        _parser.NextToken();
                        currentClause.IsStarSelect = true;
                        break;
                    case QueryExpressionTokenKind.Identifier:
                    {
                        var member = _parser.BindMember((string) _parser.TokenData) as PropertyInfo;
                        if (member == null)
                        {
                            _parser.ReportError($"'{_parser.TokenData}' is not a valid property for selection");
                            return;
                        }

                        _parser.NextToken();


                        // not yet selected -> create subclause
                        if (!currentClause.SelectClauses.TryGetValue(member, out var subClause))
                        {
                            subClause = new SelectClause();
                            subClause.SelectedMember = member;
                            currentClause.SelectClauses[member] = subClause;
                        }


                        // nested expression
                        if (_parser.CurrentTokenKind == QueryExpressionTokenKind.Slash)
                        {
                            _parser.NextToken();

                            // activate sub clauses
                            subClause.SelectClauses = new Dictionary<PropertyInfo, SelectClause>();
                            // parse next segment into this subclause
                            if (IsNavigationProperty(member, out var expressionType, out _))
                            {
                                _parser.PushThis(Expression.Parameter(expressionType));
                                SelectItem(subClause);
                                _parser.PopThis();
                            }
                            else if(member.DeclaringType != null)
                            {
                                _parser.ReportError($"'{member.DeclaringType.FullName}.{member.Name}' is no navigation property, cannot expand with slash");
                            }
                            else
                            {
                                _parser.ReportError($"'{member.Name}' is no navigation property, cannot expand with slash");
                            }
                        }
                        else
                        {
                            // $select=complexProperty/subproperty is equal to $select=complexProperty/subproperty/*
                            subClause.IsStarSelect = true;
                        }

                        break;
                    }
                }
            }
        }

        private class ExpandClauseParser
        {
            private readonly QueryExpressionParser _parser;
            private readonly IDictionary<PropertyInfo, ExpandClause> _clauses;
            private readonly ModelStateDictionary _modelStateDictionary;

            public ExpandClauseParser(IOpenApiTypeHandler binder, string value,
                IDictionary<PropertyInfo, ExpandClause> clauses, ModelStateDictionary modelStateDictionary)
            {
                _parser = new QueryExpressionParser(value, binder);
                _clauses = clauses;
                _modelStateDictionary = modelStateDictionary;
            }

            private ExpandClauseParser(QueryExpressionParser parser, IDictionary<PropertyInfo, ExpandClause> clauses)
            {
                _parser = parser;
                _clauses = clauses;
            }

            public void Parse(ParameterExpression it)
            {
                _parser.PushThis(it);
                ExpandItemList();
                _parser.PopThis();
            }

            private void ExpandItemList()
            {
                while (true)
                {
                    ExpandItem();
                    if (_parser.CurrentTokenKind == QueryExpressionTokenKind.Comma)
                    {
                        _parser.NextToken();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            private void ExpandItem()
            {
                if (_parser.CurrentTokenKind == QueryExpressionTokenKind.Identifier)
                {
                    var member = _parser.BindMember((string) _parser.TokenData);

                    if (!(member is PropertyInfo property) ||
                        !IsNavigationProperty(property, out var itemType, out var isCollection))
                    {
                        _parser.ReportError($"Property '{_parser.TokenData}' is no navigation property");
                        return;
                    }

                    if (_clauses.ContainsKey(property))
                    {
                        _parser.ReportError($"Property '{_parser.TokenData}' is already expanded.");
                        return;
                    }

                    var clause = new ExpandClause(itemType, property, isCollection);
                    _clauses[property] = clause;
                    _parser.NextToken();

                    if (_parser.CurrentTokenKind == QueryExpressionTokenKind.OpenParenthesis)
                    {
                        _parser.NextToken();
                        ExpandItemOptions(clause);
                    }
                }
            }

            private void ExpandItemOptions(ExpandClause clause)
            {
                while (_parser.CurrentTokenKind == QueryExpressionTokenKind.Identifier)
                {
                    var option = (string) _parser.TokenData;

                    Action<ExpandClause> handler;
                    switch(option)
                    {
                        case "$filter":
                            handler = ExpandFilter;
                            break;
                        case "$expand":
                            handler = NestedExpand;
                            break;
                        case "$orderby":
                            handler = ExpandOrder;
                            break;
                        case "$top":
                            handler = ExpandTop;
                            break;
                        case "$skip":
                            handler = ExpandSkip;
                            break;
                        default:
                            handler = null;
                            break;
                    };

                    if (handler != null)
                    {
                        _parser.NextToken();
                        if (_parser.CurrentTokenKind != QueryExpressionTokenKind.Equal)
                        {
                            _parser.ReportError($"Expected equals sign after $expand option {option}");
                        }

                        _parser.NextToken();

                        handler(clause);

                        if (_parser.CurrentTokenKind == QueryExpressionTokenKind.CloseParenthesis)
                        {
                            break;
                        }

                        if (_parser.CurrentTokenKind == QueryExpressionTokenKind.Semicolon)
                        {
                            _parser.NextToken();
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (_parser.CurrentTokenKind != QueryExpressionTokenKind.CloseParenthesis)
                {
                    _parser.ReportError($"Expected close parenthesis at position {_parser.Position}");
                }
            }

            private void NestedExpand(ExpandClause clause)
            {
                var nestedParser = new ExpandClauseParser(_parser, clause.NestedExpands);
                clause.NestedExpandParameter = Expression.Parameter(clause.ItemType);
                nestedParser.Parse(clause.NestedExpandParameter);
            }

            private void ExpandFilter(ExpandClause clause)
            {
                clause.Filter = new FilterQueryOption(clause.ItemType);
                clause.Filter.Initialize(_parser, _modelStateDictionary);
            }

            private void ExpandOrder(ExpandClause clause)
            {
                clause.Orderby = new OrderByQueryOption(clause.ItemType);
                clause.Orderby.Initialize(_parser, _modelStateDictionary);
            }

            private void ExpandTop(ExpandClause clause)
            {
                if (!QueryExpressionParser.IsNumeric(_parser.CurrentTokenKind))
                {
                    _parser.ReportError(
                        $"$expand($top) does not specify a numerical value at position {_parser.Position}");
                }

                clause.Top = new TopQueryOption();
                clause.Top.Initialize(_parser.TokenData.ToString(), _modelStateDictionary);

                _parser.NextToken();
            }

            private void ExpandSkip(ExpandClause clause)
            {
                if (!QueryExpressionParser.IsNumeric(_parser.CurrentTokenKind))
                {
                    _parser.ReportError(
                        $"$expand($skip) does not specify a numerical value at position {_parser.Position}");
                }

                clause.Skip = new SkipQueryOption();
                clause.Skip.Initialize(_parser.TokenData.ToString(), _modelStateDictionary);

                _parser.NextToken();
            }
        }


        internal static bool IsNavigationProperty(PropertyInfo property, out Type itemType, out bool isCollection)
        {
            // 1:many
            if (ReflectionHelper.IsEnumerable(property.PropertyType, out itemType))
            {
                isCollection = true;
                return true;
            }

            // * : 0..1
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                itemType = property.PropertyType;
                isCollection = false;
                return true;
            }

            itemType = null;
            isCollection = false;
            return false;
        }
    }
}
