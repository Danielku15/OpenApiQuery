
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenApiQuery
{
    public class ExpandClause
    {
        private readonly PropertyInfo _navigationProperty;
        private readonly bool _isCollection;
        
        public Type ItemType { get; }
        public ParameterExpression FilterParameter { get; set; }
        public Expression Filter { get; set; }

        public IDictionary<PropertyInfo, ExpandClause> NestedExpands { get; }

        public ExpandClause(Type itemType, PropertyInfo property, bool isCollection)
        {
            _navigationProperty = property;
            _isCollection = isCollection;
            NestedExpands = new Dictionary<PropertyInfo, ExpandClause>();
            ItemType = itemType;
        }

        public MemberBinding ToMemberBinding(Expression it)
        {
            Expression navigationProperty = Expression.MakeMemberAccess(it, _navigationProperty);

            // NavigationProperty = it.NavigationProperty

            if (Filter != null && _isCollection)
            {
                // TODO cache generics stuff

                // NavigationProperty = it.NavigationProperty.Where( arg => filter )
                var funcType = typeof(Func<,>).MakeGenericType(ItemType, typeof(bool));
                var expression = Expression.Lambda(funcType,
                    Filter,
                    FilterParameter
                );

                navigationProperty = Expression.Call(null, WhereInfo.MakeGenericMethod(ItemType),
                    navigationProperty,
                    expression
                );
            }

            if (_isCollection)
            {
                // NavigationProperty = it.NavigationProperty.Select( arg => new Item { ... } )
                var expression = BuildSelectLambdaExpression(ItemType, NestedExpands);

                navigationProperty = Expression.Call(null,
                    SelectInfo.MakeGenericMethod(ItemType, ItemType),
                    navigationProperty,
                    expression
                );
            }
            else
            {
                // NavigationProperty = new Item { ... }
                navigationProperty = BuildMemberInit(ItemType, navigationProperty, NestedExpands);
            }

            if (_isCollection)
            {
                // NavigationProperty = it.NavigationProperty.Select( arg => new Item { ... } ).ToArray()
                // NavigationProperty = it.NavigationProperty.Select( arg => new Item { ... } ).ToList()
                var loadFunction = _navigationProperty.PropertyType.IsArray
                    ? ToArrayInfo.MakeGenericMethod(ItemType)
                    : ToListInfo.MakeGenericMethod(ItemType);

                navigationProperty = Expression.Call(null,
                    loadFunction,
                    navigationProperty
                );
            }

            return Expression.Bind(_navigationProperty, navigationProperty);
        }

        internal static LambdaExpression BuildSelectLambdaExpression(Type itemType,
            IDictionary<PropertyInfo, ExpandClause> expands)
        {
            var arg = Expression.Parameter(itemType);
            var body = BuildMemberInit(itemType, arg, expands);
            var funcType = typeof(Func<,>).MakeGenericType(itemType, itemType);

            // arg => new Item { ... } 
            return Expression.Lambda(funcType, body, arg);
        }

        private static MemberInitExpression BuildMemberInit(
            Type itemType,
            Expression source,
            IDictionary<PropertyInfo, ExpandClause> expands)
        {
            // NavigationProperty = it.NavigationProperty.Select( arg => new Item {
            //    Prop1 = arg.Prop1,
            //    Prop2 = arg.Prop2,
            //    Prop3 = arg.Prop3.Select( arg1 => new SubItem { SubProp1 = arg1.SubProp1 } ).ToList()
            // })

            var memberBindings = new List<MemberBinding>();
            foreach (var property in itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanWrite == true)
                {
                    if (expands.TryGetValue(property, out var expand))
                    {
                        // Prop3 = arg.Prop3.Select( arg1 => new SubItem { SubProp1 = arg1.SubProp1 } ).ToList()
                        memberBindings.Add(expand.ToMemberBinding(source));
                    }
                    else if (ExpandQueryOption.IsNavigationProperty(property, out _, out _))
                    {
                        // Prop1 = arg.prop1
                        memberBindings.Add(Expression.Bind(property, Expression.Constant(null, property.PropertyType)));
                    }
                    else
                    {
                        // Prop1 = arg.prop1
                        memberBindings.Add(Expression.Bind(property, Expression.MakeMemberAccess(source, property)));
                    }
                }
            }

            return Expression.MemberInit(
                Expression.New(itemType),
                memberBindings
            );
        }

        private static readonly MethodInfo WhereInfo =
            ReflectionHelper.GetMethod<IEnumerable<object>, IEnumerable<object>>(x => x.Where(y => y == null));

        private static readonly MethodInfo SelectInfo =
            ReflectionHelper.GetMethod<IEnumerable<object>, object>(x => x.Select(y => y));

        private static readonly MethodInfo ToArrayInfo =
            ReflectionHelper.GetMethod<IEnumerable<object>, object[]>(x => x.ToArray());

        private static readonly MethodInfo ToListInfo =
            ReflectionHelper.GetMethod<IEnumerable<object>, List<object>>(x => x.ToList());
    }
}