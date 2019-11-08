using System;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenApiQuery
{
    internal static class ReflectionHelper
    {
        public static MemberInfo GetMember<T, TRet>(Expression<Func<T, TRet>> expr)
        {
            return ((MemberExpression) expr.Body).Member;
        }
        
        public static MethodInfo GetMethod<T, TRet>(Expression<Func<T, TRet>> expr)
        {
            var method = ((MethodCallExpression) expr.Body).Method;
            return method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
        }
    }
}