using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using OpenApiQuery.Parsing;
using OpenApiQuery.Utils;

namespace OpenApiQuery
{
    public class SelectClauseParser
    {
        private readonly QueryExpressionParser _parser;

        public SelectClauseParser(IOpenApiTypeHandler binder, string value)
        {
            _parser = new QueryExpressionParser(value, binder);
        }

        internal SelectClauseParser(QueryExpressionParser parser)
        {
            _parser = parser;
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
                    var member = _parser.BindMember((string)_parser.TokenData) as PropertyInfo;
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
                            _parser.PushThis(Expression.Parameter(expressionType, "it"));
                            SelectItem(subClause);
                            _parser.PopThis();
                        }
                        else if (member.DeclaringType != null)
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
