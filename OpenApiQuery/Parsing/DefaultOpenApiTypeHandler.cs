using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Parsing
{
    public class DefaultOpenApiTypeHandler : IOpenApiTypeHandler
    {
        private ConcurrentDictionary<Type, IOpenApiType> _typeCache = new ConcurrentDictionary<Type, IOpenApiType>();

        public IOpenApiType ResolveType(Type clrType)
        {
            if (!_typeCache.TryGetValue(clrType, out var type))
            {
                _typeCache[clrType] = type = BuildOpenApiType(clrType);
            }

            return type;
        }

        private IOpenApiType BuildOpenApiType(Type clrType)
        {
            var apiType = new OpenApiType(clrType, clrType.Name);
            if (clrType.Assembly == typeof(object).Assembly)
            {
                return apiType;
            }

            var properties = clrType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(IsPropertyForApi);

            foreach (var property in properties)
            {
                var it = Expression.Parameter(typeof(object), "it");
                var get = Expression.Lambda<Func<object, object>>(
                    Expression.Convert(Expression.MakeMemberAccess(Expression.Convert(it, clrType), property), typeof(object)),
                    it
                ).Compile();

                var jsonName = property.Name;
                apiType.Properties[jsonName] = new OpenApiTypeProperty(property, jsonName, property.PropertyType, get);
            }

            return apiType;
        }

        private static bool IsPropertyForApi(PropertyInfo propertyInfo)
        {
            return propertyInfo.CanWrite && propertyInfo.GetCustomAttribute(typeof(JsonIgnoreAttribute)) == null;
        }

        public MemberInfo BindMember(Expression instance, string memberName)
        {
            var type = instance.Type;
            var member = type.GetMembers(BindingFlags.Instance | BindingFlags.Public).SingleOrDefault(m =>
                memberName.Equals(m.Name, StringComparison.InvariantCultureIgnoreCase));
            if (member == null)
            {
                throw new BindException($"Could not find member '{memberName}' on type '{type.Name}'");
            }

            return member;
        }

        private static readonly MethodInfo StringConcat =
            ReflectionHelper.GetMethod<object[], string>(x => string.Concat(x));

        private static readonly MethodInfo StringContains =
            ReflectionHelper.GetMethod<string, bool>(x => x.Contains(""));

        private static readonly MethodInfo StringEndsWith =
            ReflectionHelper.GetMethod<string, bool>(x => x.EndsWith(""));

        private static readonly MethodInfo StringStartsWith =
            ReflectionHelper.GetMethod<string, bool>(x => x.StartsWith(""));

        private static readonly MethodInfo StringIndexOf =
            // ReSharper disable once StringIndexOfIsCultureSpecific.1
            ReflectionHelper.GetMethod<string, int>(x => x.IndexOf(""));

        private static readonly MethodInfo StringSubstringOneParam =
            ReflectionHelper.GetMethod<string, string>(x => x.Substring(0));

        private static readonly MethodInfo StringSubstringTwoParam =
            ReflectionHelper.GetMethod<string, string>(x => x.Substring(0, 1));

        private static readonly MethodInfo EnumerableContains =
            ReflectionHelper.GetMethod<IEnumerable<object>, bool>(x => x.Contains(null));

        private static readonly PropertyInfo StringLength =
            (PropertyInfo) ReflectionHelper.GetMember<string, int>(x => x.Length);

        private static readonly MethodInfo EnumerableCount =
            ReflectionHelper.GetMethod<IEnumerable<object>, int>(x => x.Count());

        public Expression BindFunctionCall(string identifier,
            List<Expression> arguments)
        {
            switch (identifier)
            {
                // string functions
                case "concat":
                    if (arguments.Count != 0)
                    {
                        throw new BindException("concat needs at least 1 parameter");
                    }

                    return Expression.Call(null, StringConcat, arguments);
                case "contains":
                    if (arguments.Count != 2)
                    {
                        throw new BindException("contains needs 2 arguments");
                    }

                    if (arguments[0].Type == typeof(string))
                    {
                        return Expression.Call(arguments[0], StringContains, arguments[1]);
                    }
                    else
                    {
                        return Expression.Call(arguments[0], EnumerableContains, arguments[1]);
                    }
                case "endsWith":
                    if (arguments.Count != 2)
                    {
                        throw new BindException("endsWith needs 2 argument");
                    }

                    return Expression.Call(arguments[0], StringEndsWith, arguments[1]);
                case "indexOf":
                    if (arguments.Count != 2)
                    {
                        throw new BindException("indexOf needs 2 arguments");
                    }

                    return Expression.Call(arguments[0], StringIndexOf, arguments[1]);
                case "length":
                    if (arguments.Count != 1)
                    {
                        throw new BindException("length needs 1 argument");
                    }

                    if (arguments[0].Type == typeof(string))
                    {
                        return Expression.MakeMemberAccess(arguments[0], StringLength);
                    }
                    else
                    {
                        return Expression.Call(null, EnumerableCount, arguments[0]);
                    }
                case "startsWith":
                    if (arguments.Count != 2)
                    {
                        throw new BindException("startsWith needs 2 argument");
                    }

                    return Expression.Call(arguments[0], StringStartsWith, arguments[1]);
                case "substring":
                    if (arguments.Count != 2 && arguments.Count != 3)
                    {
                        throw new BindException("substring needs 2 or 3 argument");
                    }

                    if (arguments.Count == 2)
                    {
                        return Expression.Call(arguments[0], StringSubstringOneParam,
                            arguments[1]);
                    }
                    else
                    {
                        return Expression.Call(arguments[0], StringSubstringTwoParam,
                            arguments[1], arguments[2]);
                    }
                case "matchesPattern":
                    break;
                case "toLower":
                    break;
                case "toUpper":
                    break;
                case "trim":
                    break;

//                // arithmetic functions
//                case "ceiling":
//                    break;
//                case "floor":
//                    break;
//                case "round":
//                    break;
//
//                // type functions
//                case "cast":
//                    break;
//                case "isof":
//                    break;

                default:
                    throw new BindException($"Could not find any function '{identifier}'");
            }

            return null;
        }

    }
}
