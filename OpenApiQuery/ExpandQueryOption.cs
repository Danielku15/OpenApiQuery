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

namespace OpenApiQuery
{
    public class ExpandQueryOption
    {
        private readonly ParameterExpression _parameter;

        private IDictionary<PropertyInfo, ExpandClause> _clauses;

        public ExpandQueryOption(Type elementType)
        {
            _clauses = new Dictionary<PropertyInfo, ExpandClause>();
            _parameter = Expression.Parameter(elementType);
        }

        public IQueryable<T> ApplyTo<T>(IQueryable<T> queryable)
            where T : new()
        {
            var selectClause = ExpandClause.BuildSelectLambdaExpression(_parameter.Type, _clauses);
            queryable = queryable.Select((Expression<Func<T, T>>) selectClause);
            return queryable;
        }

        public void Initialize(HttpContext httpContext, ILogger logger, ModelStateDictionary modelStateDictionary)
        {
            if (httpContext.Request.Query.TryGetValue("$expand", out var values))
            {
                var binder = httpContext.RequestServices.GetRequiredService<IExpressionBinder>();
                foreach (var value in values)
                {
                    var parser = new ExpandClauseParser(binder, value, _clauses);
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

        private class ExpandClauseParser
        {
            private readonly QueryExpressionParser _parser;
            private readonly IDictionary<PropertyInfo, ExpandClause> _clauses;

            public ExpandClauseParser(IExpressionBinder binder, string value, IDictionary<PropertyInfo, ExpandClause> clauses)
            {
                _parser = new QueryExpressionParser(value, binder);
                _clauses = clauses;
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

                    if (!(member is PropertyInfo property) || !IsNavigationProperty(property, out var itemType, out var isCollection))
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

                    Action<ExpandClause> handler = null;
                    switch (option)
                    {
                        case "$filter":
                            handler = ExpandFilter;
                            break;
                        case "$expand":
                            handler = NestedExpand;
                            break;
                        // TODO: support also $orderby, $skip, $top here? might need some refactoring to generalize the parameter parsing
                    }

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
                clause.FilterParameter = Expression.Parameter(clause.ItemType);
                nestedParser.Parse(clause.FilterParameter);
            }

            private void ExpandFilter(ExpandClause clause)
            {
                clause.FilterParameter = Expression.Parameter(clause.ItemType);
                _parser.PushThis(clause.FilterParameter);
                clause.Filter = _parser.CommonExpr();
                _parser.PopThis();
            }
        }


        internal static bool IsNavigationProperty(PropertyInfo property, out Type itemType, out bool isCollection)
        {
            // 1:many
            if (property.PropertyType.IsGenericType &&
                typeof(IEnumerable<>).MakeGenericType(property.PropertyType.GetGenericArguments()[0])
                    .IsAssignableFrom(property.PropertyType))
            {
                itemType = property.PropertyType.GetGenericArguments()[0];
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