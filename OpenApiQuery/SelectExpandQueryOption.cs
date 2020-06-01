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
            _parameter = Expression.Parameter(elementType, "it");
            RootSelectClause = new SelectClause
            {
                IsStarSelect = true
            };
        }

        public IQueryable<T> ApplyTo<T>(IQueryable<T> queryable)
        {
            var selectClause = GetSelectClause<T>();
            queryable = queryable.Select(selectClause);

            return queryable;
        }

        public Expression<Func<T, T>> GetSelectClause<T>() =>
            (Expression<Func<T, T>>)
            ExpandClause.BuildSelectLambdaExpression(
                _parameter.Type,
                RootSelectClause,
                _expandClauses);

        public void Initialize(HttpContext httpContext, ILogger logger, ModelStateDictionary modelStateDictionary)
        {
            InitializeExpand(httpContext, logger, modelStateDictionary);
            InitializeSelect(httpContext, logger, modelStateDictionary);
        }

        private void InitializeSelect(HttpContext httpContext, ILogger logger,
            ModelStateDictionary modelStateDictionary)
        {
            if (httpContext.Request.Query.TryGetValues(QueryOptionKeys.SelectKeys, out var values))
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
                        logger.LogError(e, "failed to parse select clause");
                        modelStateDictionary.TryAddModelException(QueryOptionKeys.SelectKeys.First(), e);
                    }
                }
            }
        }

        private void InitializeExpand(HttpContext httpContext, ILogger logger,
            ModelStateDictionary modelStateDictionary)
        {
            if (httpContext.Request.Query.TryGetValues(QueryOptionKeys.ExpandKeys, out var values))
            {
                var binder = httpContext.RequestServices.GetRequiredService<IOpenApiTypeHandler>();
                foreach (var value in values) // .SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parser = new ExpandClauseParser(binder, value, _expandClauses, modelStateDictionary);
                    try
                    {
                        parser.Parse(_parameter);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "failed to parse expand clause");
                        modelStateDictionary.TryAddModelException(QueryOptionKeys.ExpandKeys.First(), e);
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
                        if (_parser.CurrentTokenKind == QueryExpressionTokenKind.CloseParenthesis)
                        {
                            _parser.NextToken();
                        }
                    }
                }
            }

            private void ExpandItemOptions(ExpandClause clause)
            {
                while (_parser.CurrentTokenKind == QueryExpressionTokenKind.Identifier)
                {
                    var option = (string) _parser.TokenData;

                    Action<ExpandClause> handler = 0 switch
                    {
                        _ when QueryOptionKeys.SelectKeys.Contains(option) => ExpandSelect,
                        _ when QueryOptionKeys.FilterKeys.Contains(option) => ExpandFilter,
                        _ when QueryOptionKeys.ExpandKeys.Contains(option) => NestedExpand,
                        _ when QueryOptionKeys.TopKeys.Contains(option) => ExpandTop,
                        _ when QueryOptionKeys.SkipKeys.Contains(option) => ExpandSkip,
                        _ when QueryOptionKeys.OrderbyKeys.Contains(option) => ExpandOrder,
                        _ => null
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
                clause.NestedExpandParameter = Expression.Parameter(clause.ItemType, "it");
                nestedParser.Parse(clause.NestedExpandParameter);
            }

            private void ExpandSelect(ExpandClause clause)
            {
                clause.Select = new SelectQueryOption(clause.ItemType);
                clause.Select.Initialize(_parser, _modelStateDictionary);
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
