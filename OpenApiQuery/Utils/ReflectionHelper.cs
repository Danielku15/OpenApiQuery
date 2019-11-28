using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenApiQuery.Utils
{
    internal static class ReflectionHelper
    {
        public static MemberInfo GetMember<T, TRet>(Expression<Func<T, TRet>> expr)
        {
            return ((MemberExpression)expr.Body).Member;
        }

        public static MethodInfo GetMethod<T, TRet>(Expression<Func<T, TRet>> expr)
        {
            var method = ((MethodCallExpression)expr.Body).Method;
            return method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
        }


        public static bool ImplementsEnumerable(Type type, out Type itemType)
        {
            // 1:many
            var enumerable = type.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (enumerable != null)
            {
                itemType = enumerable.GetGenericArguments()[0];
                return true;
            }

            itemType = null;
            return false;
        }

        public static bool IsEnumerable(Type type, out Type itemType)
        {
            // 1:many
            if (type.IsGenericType &&
                typeof(IEnumerable<>).MakeGenericType(type.GetGenericArguments()[0])
                    .IsAssignableFrom(type))
            {
                itemType = type.GetGenericArguments()[0];
                return true;
            }

            itemType = null;
            return false;
        }

        public static bool ImplementsDictionary(Type type, out Type keyType, out Type valueType)
        {
            if (type.IsGenericType &&
                typeof(IDictionary<,>).MakeGenericType(type.GetGenericArguments()[0], type.GetGenericArguments()[1])
                    .IsAssignableFrom(type))
            {
                keyType = type.GetGenericArguments()[0];
                valueType = type.GetGenericArguments()[1];
                return true;
            }

            keyType = null;
            valueType = null;
            return false;
        }
    }
}
