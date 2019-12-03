using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OpenApiQuery.Utils;

namespace OpenApiQuery
{
    public class ExpandClause
    {
        private readonly PropertyInfo _navigationProperty;
        private readonly bool _isCollection;

        public Type ItemType { get; }

        // additional expand options
        public FilterQueryOption Filter { get; set; }
        public OrderByQueryOption Orderby { get; set; }
        public TopQueryOption Top { get; set; }
        public SkipQueryOption Skip { get; set; }

        public IDictionary<PropertyInfo, ExpandClause> NestedExpands { get; }
        public ParameterExpression NestedExpandParameter { get; set; }

        public ExpandClause(Type itemType, PropertyInfo property, bool isCollection)
        {
            _navigationProperty = property;
            _isCollection = isCollection;
            NestedExpands = new Dictionary<PropertyInfo, ExpandClause>();
            ItemType = itemType;
        }

        private MemberBinding ToMemberBinding(Expression it, SelectClause selectClause)
        {
            Expression navigationProperty = Expression.MakeMemberAccess(it, _navigationProperty);

            if (_isCollection)
            {
                // NavigationProperty = it.NavigationProperty.Select( arg => new Item { ... } ).ToArray()
                // NavigationProperty = it.NavigationProperty.Select( arg => new Item { ... } ).ToList()

                // apply all constraints
                if (Orderby != null)
                {
                    navigationProperty = Orderby.ApplyTo(navigationProperty);
                }
                if (Filter != null)
                {
                    navigationProperty = Filter.ApplyTo(navigationProperty);
                }
                if (Skip != null)
                {
                    navigationProperty = Skip.ApplyTo(navigationProperty, ItemType);
                }
                if (Top != null)
                {
                    navigationProperty = Top.ApplyTo(navigationProperty, ItemType);
                }

                // build proper select of properties
                var expression = BuildSelectLambdaExpression(ItemType, selectClause, NestedExpands);

                navigationProperty = Expression.Call(null,
                    SelectInfo.MakeGenericMethod(ItemType, ItemType),
                    navigationProperty,
                    expression
                );

                // add ToList/ToArray
                var loadFunction = _navigationProperty.PropertyType.IsArray
                    ? ToArrayInfo.MakeGenericMethod(ItemType)
                    : ToListInfo.MakeGenericMethod(ItemType);

                navigationProperty = Expression.Call(null,
                    loadFunction,
                    navigationProperty
                );
            }
            else
            {
                // NavigationProperty = new Item { ... }
                navigationProperty = BuildMemberInit(ItemType, navigationProperty, selectClause, NestedExpands);
            }

            return Expression.Bind(_navigationProperty, navigationProperty);
        }

        internal static LambdaExpression BuildSelectLambdaExpression(Type itemType,
            SelectClause selectClause,
            IDictionary<PropertyInfo, ExpandClause> expands)
        {
            var arg = Expression.Parameter(itemType, "arg");
            var body = BuildMemberInit(itemType, arg, selectClause, expands);
            var funcType = typeof(Func<,>).MakeGenericType(itemType, itemType);

            // arg => new Item { ... }
            return Expression.Lambda(funcType, body, arg);
        }

        private static MemberInitExpression BuildMemberInit(
            Type itemType,
            Expression source,
            SelectClause select,
            IDictionary<PropertyInfo, ExpandClause> expands)
        {
            // NavigationProperty = it.NavigationProperty.Select( arg => new Item {
            //    Prop1 = arg.Prop1,
            //    Prop2 = arg.Prop2,
            //    Prop3 = arg.Prop3.Select( arg1 => new SubItem { SubProp1 = arg1.SubProp1 } ).ToList()
            // })

            var selectAllProperties = select == null ||
                                      select.IsStarSelect ||
                                      select.SelectClauses == null ||
                                      select.SelectClauses.Count == 0;

            var memberBindings = new List<MemberBinding>();
            foreach (var property in itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (expands.TryGetValue(property, out var expand))
                {
                    // Prop3 = arg.Prop3.Select( arg1 => new SubItem { SubProp1 = arg1.SubProp1 } ).ToList()
                    if (select?.SelectClauses == null || !select.SelectClauses.TryGetValue(property, out var subSelect))
                    {
                        subSelect = new SelectClause
                        {
                            SelectedMember = property,
                            IsStarSelect = true
                        };
                    }
                    memberBindings.Add(expand.ToMemberBinding(source, subSelect));
                }
                else if (SelectExpandQueryOption.IsNavigationProperty(property, out _, out _))
                {
                    // navigation properties that are not expanded, are not loaded
                    // Prop1 = null
                    //memberBindings.Add(Expression.Bind(property, Expression.Constant(null, property.PropertyType)));
                }
                else if (selectAllProperties || select.SelectClauses.ContainsKey(property))
                {
                    // Prop1 = arg.prop1
                    memberBindings.Add(Expression.Bind(property, Expression.MakeMemberAccess(source, property)));
                }
            }

            return Expression.MemberInit(
                Expression.New(itemType),
                memberBindings
            );
        }


        private static readonly MethodInfo SelectInfo =
            ReflectionHelper.GetMethod<IEnumerable<object>, object>(x => x.Select(y => y));

        private static readonly MethodInfo ToArrayInfo =
            ReflectionHelper.GetMethod<IEnumerable<object>, object[]>(x => x.ToArray());

        private static readonly MethodInfo ToListInfo =
            ReflectionHelper.GetMethod<IEnumerable<object>, List<object>>(x => x.ToList());
    }
}
