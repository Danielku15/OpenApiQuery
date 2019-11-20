using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenApiQuery.Parsing;

namespace OpenApiQuery
{
    public class OrderByQueryOption
    {
        public string RawValue { get; set; }
        public ParameterExpression Parameter { get; set; }
        public IList<OrderByClause> Clauses { get; set; }

        public OrderByQueryOption(Type elementType)
        {
            Clauses = new List<OrderByClause>();
            Parameter = Expression.Parameter(elementType);
        }

        public IQueryable<T> ApplyTo<T>(IQueryable<T> queryable)
        {
            bool isFirstClause = true;
            foreach (var clause in Clauses)
            {
                queryable = clause.ApplyTo(queryable, Parameter, isFirstClause);
                isFirstClause = false;
            }

            return queryable;
        }

        public void Initialize(HttpContext httpContext, ILogger logger, ModelStateDictionary modelState)
        {
            if (httpContext.Request.Query.TryGetValue("$orderby", out var values))
            {
                if (values.Count == 1)
                {
                    var binder = httpContext.RequestServices.GetRequiredService<IOpenApiQueryExpressionBinder>();
                    RawValue = values[0];
                    var parser = new OrderByClauseParser(binder, values[0], Clauses);
                    try
                    {
                        parser.Parse(Parameter);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Failed to parse orderby clause");
                        modelState.TryAddModelException("$orderby", e);
                    }
                }
                else
                {
                    modelState.TryAddModelError("$orderby",
                        "Multiple orderby clauses found, onle one can be specified.");
                }
            }
        }


        private class OrderByClauseParser
        {
            private readonly QueryExpressionParser _parser;
            private readonly IList<OrderByClause> _clauses;

            public OrderByClauseParser(IOpenApiQueryExpressionBinder binder, string value, IList<OrderByClause> clauses)
            {
                _parser = new QueryExpressionParser(value, binder);
                _clauses = clauses;
            }

            public void Parse(ParameterExpression it)
            {
                _parser.PushThis(it);
                OrderByItemList();
                _parser.PopThis();
            }

            private void OrderByItemList()
            {
                while (true)
                {
                    var expression = _parser.CommonExpr();
                    var direction = OrderByDirection.Acending;

                    if (_parser.CurrentTokenKind == QueryExpressionTokenKind.Keyword)
                    {
                        var order = (string) _parser.TokenData;
                        switch (order)
                        {
                            case "asc":
                                direction = OrderByDirection.Acending;
                                _parser.NextToken();
                                break;
                            case "desc":
                                direction = OrderByDirection.Descending;
                                _parser.NextToken();
                                break;
                            default:
                                _parser.ReportError(
                                    $"Invalid token '{order}' after sort expression, expecting 'asc' or 'desc'");
                                return;
                        }
                    }

                    var item = new OrderByClause(expression, direction);
                    _clauses.Add(item);

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
        }
    }
}