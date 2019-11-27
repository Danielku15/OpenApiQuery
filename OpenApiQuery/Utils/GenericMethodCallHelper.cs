using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenApiQuery.Utils
{
    /// <summary>
    /// A small helper class to call a generic method using Type variables.
    /// </summary>
    /// <typeparam name="TThis"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TRet"></typeparam>
    internal class GenericMethodCallHelper<TThis, TParam1, TParam2, TRet>
    {
        private readonly ConcurrentDictionary<int, Func<TThis, TParam1, TParam2, TRet>> _cache =
            new ConcurrentDictionary<int, Func<TThis, TParam1, TParam2, TRet>>();

        private readonly MethodInfo _method;

        public GenericMethodCallHelper(Expression<Func<TThis, TRet>> method)
        {
            _method = ((MethodCallExpression) method.Body).Method.GetGenericMethodDefinition();
        }

        public TRet Invoke(TThis instance, TParam1 arg1, TParam2 arg2, params Type[] typeArguments)
        {
            var key = TypeArrayKey(typeArguments);
            if (!_cache.TryGetValue(key, out var method))
            {
                _cache[key] = method = BuildMethodCall(typeArguments);
            }

            return method(instance, arg1, arg2);
        }

        private static int TypeArrayKey(Type[] types)
        {
            var hashCode = types[0].GetHashCode();
            for (var i = 1; i < types.Length; i++)
            {
                hashCode = (hashCode * 397) ^ types[i].GetHashCode();
            }
            return hashCode;
        }

        private Func<TThis, TParam1, TParam2, TRet> BuildMethodCall(Type[] typeArguments)
        {
            return (Func<TThis, TParam1, TParam2, TRet>) MakeGenericMethodCall(_method,
                typeof(TThis), typeof(TRet),
                new[] {typeof(TParam1), typeof(TParam2)}, typeArguments);
        }

        private static Delegate MakeGenericMethodCall(MethodInfo method,
            Type instanceType,
            Type returnType,
            Type[] argumentTypes,
            Type[] typeParameterTypes)
        {
            var instanceParameter = Expression.Parameter(instanceType, "instance");
            var arguments = argumentTypes.Select((t, i) => Expression.Parameter(t, $"arg{i}")).ToArray();

            var funcTypeArguments = new[] {instanceType}.Concat(argumentTypes).Concat(new[] {returnType}).ToArray();
            var funcType = GetFuncBaseType(funcTypeArguments.Length).MakeGenericType(funcTypeArguments);

            var methodToCall = method.MakeGenericMethod(typeParameterTypes);

            var body = Expression.Call(
                instanceParameter,
                methodToCall,
                arguments.Cast<Expression>()
            );

            var allParameters = new[]
            {
                instanceParameter
            }.Concat(arguments);

            return Expression.Lambda(funcType, body, allParameters).Compile();
        }

        private static Type GetFuncBaseType(int count)
        {
            return count switch
            {
                1 => typeof(Func<>),
                2 => typeof(Func<,>),
                3 => typeof(Func<,,>),
                4 => typeof(Func<,,,>),
                5 => typeof(Func<,,,,>),
                6 => typeof(Func<,,,,,>),
                7 => typeof(Func<,,,,,,>),
                8 => typeof(Func<,,,,,,>),
                9 => typeof(Func<,,,,,,>),
                10 => typeof(Func<,,,,,,>),
                11 => typeof(Func<,,,,,,,>),
                12 => typeof(Func<,,,,,,,,>),
                13 => typeof(Func<,,,,,,,,>),
                14 => typeof(Func<,,,,,,,,>),
                15 => typeof(Func<,,,,,,,,>),
                16 => typeof(Func<,,,,,,,,>),
                17 => typeof(Func<,,,,,,,,>),
                18 => typeof(Func<,,,,,,,,,>),
                _ => throw new ArgumentOutOfRangeException(nameof(count))
            };
        }
    }
}
