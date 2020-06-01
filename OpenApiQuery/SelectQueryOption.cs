using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OpenApiQuery.Parsing;

namespace OpenApiQuery
{
    public class SelectQueryOption
    {
        public ParameterExpression Parameter { get; set; }
        public SelectClause SelectClause { get; set; }

        public SelectQueryOption(Type elementType)
        {
            Parameter = Expression.Parameter(elementType, "it");
        }

        public Expression ApplyTo(Expression expression)
        {
            if (SelectClause != null)
            {
                return expression;
            }

            return expression;
        }

        internal void Initialize(QueryExpressionParser parser, ModelStateDictionary modelState)
        {
            var selectParser = new SelectClauseParser(parser);
            try
            {
                SelectClause = selectParser.Parse(Parameter);
            }
            catch (Exception e)
            {
                modelState.TryAddModelException(QueryOptionKeys.OrderbyKeys.First(), e);
            }
        }
    }
}
