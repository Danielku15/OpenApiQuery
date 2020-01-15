using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Parsing
{
    public class DefaultOpenApiTypeHandler : IOpenApiTypeHandler
    {
        private readonly ConcurrentDictionary<Type, IOpenApiType> _typeCache =
            new ConcurrentDictionary<Type, IOpenApiType>();

        private readonly ConcurrentDictionary<string, IOpenApiType> _typeByNameCache =
            new ConcurrentDictionary<string, IOpenApiType>();

        public IOpenApiType ResolveType(Type clrType)
        {
            if (!_typeCache.TryGetValue(clrType, out var type))
            {
                _typeCache[clrType] = type = BuildOpenApiType(clrType);
                if (type != null)
                {
                    _typeByNameCache[type.JsonName] = type;
                }
            }

            return type;
        }

        public IOpenApiType ResolveType(string jsonName)
        {
            if (jsonName == null)
            {
                return null;
            }

            if (!_typeByNameCache.TryGetValue(jsonName, out var type))
            {
                // TOOD: how to resolve type correctly without security issues?
                return null;
            }

            return type;
        }

        private IOpenApiType BuildOpenApiType(Type clrType)
        {
            var apiType = new OpenApiType(clrType, clrType.Name);
            if (clrType.Assembly == typeof(object).Assembly)
            {
                return null;
            }

            var properties = clrType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .Where(IsPropertyForApi);

            foreach (var property in properties)
            {
                var it = Expression.Parameter(typeof(object), "it");
                var get = Expression.Lambda<Func<object, object>>(
                    Expression.Convert(Expression.MakeMemberAccess(Expression.Convert(it, clrType), property),
                        typeof(object)),
                    it
                ).Compile();

                var value = Expression.Parameter(typeof(object), "value");
                var set = Expression.Lambda<Action<object, object>>(
                    Expression.Assign(Expression.MakeMemberAccess(Expression.Convert(it, clrType), property),
                        Expression.Convert(value, property.PropertyType)),
                    it,
                    value
                ).Compile();

                var jsonName = property.Name;
                apiType.RegisterProperty(new OpenApiTypeProperty(property, jsonName, property.PropertyType, get, set));
            }

            return apiType;
        }

        private static bool IsPropertyForApi(PropertyInfo propertyInfo)
        {
            return propertyInfo.CanWrite && propertyInfo.GetCustomAttribute(typeof(JsonIgnoreAttribute)) == null;
        }

        public PropertyInfo BindProperty(Expression instance, string memberName)
        {
            if (instance == null)
            {
                return null;
            }

            var type = instance.Type;
            var apiType = ResolveType(type);
            if (apiType == null)
            {
                return null;
            }

            if (!apiType.TryGetProperty(memberName, out var property))
            {
                return null;
            }

            return property.ClrProperty;
        }

        private static readonly MethodInfo StringConcat =
            ReflectionHelper.GetMethod<object[], string>(x => string.Concat(x));

        private static readonly MethodInfo EnumerableConcat =
            ReflectionHelper.GetMethod<IEnumerable<object>, IEnumerable<object>>(x => x.Concat(new object[0]));

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

        private static readonly MethodInfo EnumerableSkip =
            ReflectionHelper.GetMethod<IEnumerable<object>, IEnumerable<object>>(x => x.Skip(1));

        private static readonly MethodInfo EnumerableTake =
            ReflectionHelper.GetMethod<IEnumerable<object>, IEnumerable<object>>(x => x.Take(1));

        private static readonly PropertyInfo StringLength =
            (PropertyInfo)ReflectionHelper.GetMember<string, int>(x => x.Length);

        private static readonly MethodInfo EnumerableCount =
            ReflectionHelper.GetMethod<IEnumerable<object>, int>(x => x.Count());

        private static readonly MethodInfo StringToLowerInvariant =
            ReflectionHelper.GetMethod<string, string>(x => x.ToLowerInvariant());

        private static readonly MethodInfo StringToUpperInvariant =
            ReflectionHelper.GetMethod<string, string>(x => x.ToUpperInvariant());

        private static readonly MethodInfo StringTrim =
            ReflectionHelper.GetMethod<string, string>(x => x.Trim());

        private static readonly MemberInfo DateTimeOffsetDate =
            ReflectionHelper.GetMember<DateTimeOffset, DateTime>(x => x.Date);

        private static readonly MemberInfo DateTimeOffsetTime =
            ReflectionHelper.GetMember<DateTimeOffset, TimeSpan>(x => x.TimeOfDay);

        private static readonly MemberInfo DateTimeOffsetOffset =
            ReflectionHelper.GetMember<DateTimeOffset, TimeSpan>(x => x.Offset);

        private static readonly MemberInfo DateTimeOffsetDay =
            ReflectionHelper.GetMember<DateTimeOffset, int>(x => x.Day);

        private static readonly MemberInfo DateTimeOffsetMonth =
            ReflectionHelper.GetMember<DateTimeOffset, int>(x => x.Month);

        private static readonly MemberInfo DateTimeOffsetYear =
            ReflectionHelper.GetMember<DateTimeOffset, int>(x => x.Year);

        private static readonly MemberInfo DateTimeOffsetHour =
            ReflectionHelper.GetMember<DateTimeOffset, int>(x => x.Hour);

        private static readonly MemberInfo DateTimeOffsetMinute =
            ReflectionHelper.GetMember<DateTimeOffset, int>(x => x.Minute);

        private static readonly MemberInfo DateTimeOffsetSecond =
            ReflectionHelper.GetMember<DateTimeOffset, int>(x => x.Second);

        private static readonly MemberInfo DateTimeOffsetMilliseconds =
            ReflectionHelper.GetMember<DateTimeOffset, int>(x => x.Millisecond);

        private static readonly MemberInfo DateTimeDate =
            ReflectionHelper.GetMember<DateTime, DateTime>(x => x.Date);

        private static readonly MemberInfo DateTimeTime =
            ReflectionHelper.GetMember<DateTime, TimeSpan>(x => x.TimeOfDay);

        private static readonly MemberInfo DateTimeDay =
            ReflectionHelper.GetMember<DateTime, int>(x => x.Day);

        private static readonly MemberInfo DateTimeMonth =
            ReflectionHelper.GetMember<DateTime, int>(x => x.Month);

        private static readonly MemberInfo DateTimeYear =
            ReflectionHelper.GetMember<DateTime, int>(x => x.Year);

        private static readonly MemberInfo DateTimeHour =
            ReflectionHelper.GetMember<DateTime, int>(x => x.Hour);

        private static readonly MemberInfo DateTimeMinute =
            ReflectionHelper.GetMember<DateTime, int>(x => x.Minute);

        private static readonly MemberInfo DateTimeSecond =
            ReflectionHelper.GetMember<DateTime, int>(x => x.Second);

        private static readonly MemberInfo DateTimeMilliseconds =
            ReflectionHelper.GetMember<DateTime, int>(x => x.Millisecond);

        private static readonly MemberInfo TimeSpanTotalMinutes =
            ReflectionHelper.GetMember<TimeSpan, double>(x => x.TotalMinutes);

        private static readonly MethodInfo MathCeiling =
            ReflectionHelper.GetMethod(() => Math.Ceiling((double)1));

        private static readonly MethodInfo MathFloor =
            ReflectionHelper.GetMethod(() => Math.Floor((double)1));

        private static readonly MethodInfo MathRound =
            ReflectionHelper.GetMethod(() => Math.Round((double)1));

        private static readonly MethodInfo ObjectToString =
            ReflectionHelper.GetMethod<object, string>(o => o.ToString());

        public Expression BindFunctionCall(
            string identifier,
            List<Expression> arguments)
        {
            void ValidateParameterCount(params int[] count)
            {
                if (count.Length == 1 && arguments.Count != count[0])
                {
                    throw new BindException($"{identifier} needs {count[0]} parameters");
                }

                if (!count.Contains(arguments.Count))
                {
                    var counts = string.Join(", ", count.Take(count.Length - 1)) + " or " + count.Last();
                    throw new BindException($"{identifier} needs {count[0]} parameters");
                }
            }

            void InvalidParameterTypes(string supportedTypes)
            {
                throw new BindException(
                    $"Unsupported parameters provided to function '{identifier}', supported types: {supportedTypes}");
            }


            Type itemType;
            switch (identifier)
            {
                // string functions
                case "concat":
                    if (arguments.Count < 1)
                    {
                        throw new BindException($"{identifier} needs at least 1 parameter");
                    }

                    if (ReflectionHelper.IsEnumerable(arguments[0].Type, out itemType))
                    {
                        var result = arguments[0];
                        var concatMethod = EnumerableConcat.MakeGenericMethod(itemType);
                        // ReSharper disable once LoopCanBeConvertedToQuery this one is clearer
                        foreach (var arg in arguments.Skip(1))
                        {
                            result = Expression.Call(null, concatMethod, result, arg);
                        }

                        return result;
                    }
                    else if (arguments[0].Type == typeof(string))
                    {
                        return Expression.Call(null, StringConcat, Expression.NewArrayInit(typeof(object), arguments));
                    }
                    else
                    {
                        InvalidParameterTypes("strings, enumerables");
                        return null;
                    }

                case "contains":
                    ValidateParameterCount(2);

                    if (arguments[0].Type == typeof(string))
                    {
                        return Expression.Call(arguments[0], StringContains, arguments[1]);
                    }
                    else if (ReflectionHelper.IsEnumerable(arguments[0].Type, out itemType))
                    {
                        return Expression.Call(null,
                            EnumerableContains.MakeGenericMethod(itemType),
                            arguments[0],
                            arguments[1]);
                    }
                    else
                    {
                        InvalidParameterTypes("strings, enumerables");
                        return null;
                    }

                case "endswith":
                    ValidateParameterCount(2);
                    return Expression.Call(arguments[0], StringEndsWith, arguments[1]);

                case "indexof":
                    ValidateParameterCount(2);
                    return Expression.Call(arguments[0], StringIndexOf, arguments[1]);

                case "length":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(string))
                    {
                        return Expression.MakeMemberAccess(arguments[0], StringLength);
                    }
                    else if (ReflectionHelper.IsEnumerable(arguments[0].Type, out itemType))
                    {
                        return Expression.Call(null, EnumerableCount.MakeGenericMethod(itemType), arguments[0]);
                    }
                    else
                    {
                        InvalidParameterTypes("strings, enumerables");
                        return null;
                    }
                case "startswith":
                    ValidateParameterCount(2);
                    return Expression.Call(arguments[0], StringStartsWith, arguments[1]);

                case "substring":
                    ValidateParameterCount(2, 3);

                    if (arguments[0].Type == typeof(string))
                    {
                        if (arguments.Count == 2)
                        {
                            return Expression.Call(arguments[0],
                                StringSubstringOneParam,
                                arguments[1]);
                        }

                        return Expression.Call(arguments[0],
                            StringSubstringTwoParam,
                            arguments[1],
                            arguments[2]);
                    }
                    else if (ReflectionHelper.IsEnumerable(arguments[0].Type, out itemType))
                    {
                        if (arguments.Count == 2)
                        {
                            return Expression.Call(null,
                                EnumerableSkip.MakeGenericMethod(itemType),
                                arguments[0],
                                arguments[1]);
                        }

                        var skip = Expression.Call(null,
                            EnumerableSkip.MakeGenericMethod(itemType),
                            arguments[0],
                            arguments[1]);

                        return Expression.Call(null,
                            EnumerableTake.MakeGenericMethod(itemType),
                            skip,
                            arguments[2]);
                    }
                    else
                    {
                        InvalidParameterTypes("strings, enumerables");
                        return null;
                    }

                case "tolower":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(string))
                    {
                        return Expression.Call(arguments[0], StringToLowerInvariant);
                    }
                    else
                    {
                        InvalidParameterTypes("strings");
                        return null;
                    }
                case "toupper":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(string))
                    {
                        return Expression.Call(arguments[0], StringToUpperInvariant);
                    }
                    else
                    {
                        InvalidParameterTypes("strings");
                        return null;
                    }
                case "trim":
                    ValidateParameterCount(1);


                    if (arguments[0].Type == typeof(string))
                    {
                        return Expression.Call(arguments[0], StringTrim);
                    }
                    else
                    {
                        InvalidParameterTypes("strings");
                        return null;
                    }

                // date and time functions
                case "date":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(DateTime))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeDate);
                    }
                    else if (arguments[0].Type == typeof(DateTimeOffset))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeOffsetDate);
                    }
                    else
                    {
                        InvalidParameterTypes("DateTime, DateTimeOffset");
                        return null;
                    }

                case "time":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(DateTime))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeTime);
                    }
                    else if (arguments[0].Type == typeof(DateTimeOffset))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeOffsetTime);
                    }
                    else
                    {
                        InvalidParameterTypes("DateTime, DateTimeOffset");
                        return null;
                    }
                case "totaloffsetminutes":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(DateTimeOffset))
                    {
                        return Expression.MakeMemberAccess(
                            Expression.MakeMemberAccess(arguments[0], DateTimeOffsetOffset),
                            TimeSpanTotalMinutes);
                    }
                    else
                    {
                        InvalidParameterTypes("DateTimeOffset");
                        return null;
                    }
                case "day":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(DateTime))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeDay);
                    }
                    else if (arguments[0].Type == typeof(DateTimeOffset))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeOffsetDay);
                    }
                    else
                    {
                        InvalidParameterTypes("DateTime, DateTimeOffset");
                        return null;
                    }
                case "month":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(DateTime))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeMonth);
                    }
                    else if (arguments[0].Type == typeof(DateTimeOffset))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeOffsetMonth);
                    }
                    else
                    {
                        InvalidParameterTypes("DateTime, DateTimeOffset");
                        return null;
                    }
                case "year":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(DateTime))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeYear);
                    }
                    else if (arguments[0].Type == typeof(DateTimeOffset))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeOffsetYear);
                    }
                    else
                    {
                        InvalidParameterTypes("DateTime, DateTimeOffset");
                        return null;
                    }
                case "hour":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(DateTime))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeHour);
                    }
                    else if (arguments[0].Type == typeof(DateTimeOffset))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeOffsetHour);
                    }
                    else
                    {
                        InvalidParameterTypes("DateTime, DateTimeOffset");
                        return null;
                    }
                case "minute":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(DateTime))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeMinute);
                    }
                    else if (arguments[0].Type == typeof(DateTimeOffset))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeOffsetMinute);
                    }
                    else
                    {
                        InvalidParameterTypes("DateTime, DateTimeOffset");
                        return null;
                    }
                case "second":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(DateTime))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeSecond);
                    }
                    else if (arguments[0].Type == typeof(DateTimeOffset))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeOffsetSecond);
                    }
                    else
                    {
                        InvalidParameterTypes("DateTime, DateTimeOffset");
                        return null;
                    }
                case "fractionalseconds":
                    ValidateParameterCount(1);

                    if (arguments[0].Type == typeof(DateTime))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeMilliseconds);
                    }
                    else if (arguments[0].Type == typeof(DateTimeOffset))
                    {
                        return Expression.MakeMemberAccess(arguments[0], DateTimeOffsetMilliseconds);
                    }
                    else
                    {
                        InvalidParameterTypes("DateTime, DateTimeOffset");
                        return null;
                    }

                case "maxdatetime":
                    ValidateParameterCount(0);
                    return Expression.Constant(DateTimeOffset.MaxValue);
                case "mindatetime":
                    ValidateParameterCount(0);
                    return Expression.Constant(DateTimeOffset.MinValue);
                case "now":
                    ValidateParameterCount(0);
                    return Expression.Constant(DateTimeOffset.UtcNow);

                // arithmetic functions
                case "ceiling":
                    ValidateParameterCount(1);

                    var ceilArg = arguments[0].Type == typeof(double)
                        ? arguments[0]
                        : Expression.Convert(arguments[0], typeof(double));
                    return Expression.Call(null, MathCeiling, ceilArg);

                case "floor":
                    ValidateParameterCount(1);

                    var floorArg = arguments[0].Type == typeof(double)
                        ? arguments[0]
                        : Expression.Convert(arguments[0], typeof(double));
                    return Expression.Call(null, MathFloor, floorArg);
                case "round":
                    ValidateParameterCount(1);

                    var roundArg = arguments[0].Type == typeof(double)
                        ? arguments[0]
                        : Expression.Convert(arguments[0], typeof(double));
                    return Expression.Call(null, MathRound, roundArg);

                // type functions
                case "cast":
                    ValidateParameterCount(2);

                    var castType = ParseTargetType(arguments[1]);
                    if (castType == null)
                    {
                        throw new BindException("No proper type for cast specified");
                    }

                    return BindCast(arguments[0], castType);

                case "isof":
                    ValidateParameterCount(2);

                    var typeCheckType = ParseTargetType(arguments[1]);
                    if (typeCheckType == null)
                    {
                        throw new BindException("No proper type for type check specified");
                    }

                    return Expression.TypeIs(arguments[0], typeCheckType);

                default:
                    throw new BindException($"Could not find any function '{identifier}'");
            }

            return null;
        }

        private Type ParseTargetType(Expression argument)
        {
            var targetType = (argument as ConstantExpression)?.Value as Type;
            if (targetType != null)
            {
                return targetType;
            }

            var typeName = (argument as ConstantExpression)?.Value?.ToString();
            var apiType = ResolveType(typeName);
            if (apiType != null)
            {
                return apiType.ClrType;
            }

            targetType = QueryExpressionParser.GetBuiltInTypeByName(typeName);
            if (targetType != null)
            {
                return targetType;
            }

            return null;
        }

        private Expression BindCast(Expression argument, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return Expression.Call(argument, ObjectToString);
            }

            return Expression.Convert(argument, targetType);
        }
    }
}
