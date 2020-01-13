using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OpenApiQuery.Parsing
{
    public class FunctionBinder
    {
        private static readonly IList<ParameterExpression> NoArguments = new List<ParameterExpression>().AsReadOnly();

        private readonly IDictionary<string, IList<FunctionDefinition>> _functions =
            new ConcurrentDictionary<string, IList<FunctionDefinition>>();

        private readonly IDictionary<(string functionName, ParameterTypes), FunctionDefinition> _overloadLookup =
            new ConcurrentDictionary<(string functionName, ParameterTypes), FunctionDefinition>();

        public Expression Bind(string functionName, IList<Expression> arguments)
        {
            var parameterTypes = new ParameterTypes(arguments.Select(arg => arg.Type));
            if (_overloadLookup.TryGetValue((functionName, parameterTypes), out var overload))
            {
                return overload.Rewrite(arguments);
            }

            throw new BindException($"Could not find any function '{functionName}' and given parameters");
        }

        // TODO: ensure proper lambda support
        public void RegisterEnumerableFunction<TRet>(string name, Expression<Func<IEnumerable<object>, TRet>> function)
        {
            RegisterFunction(name, (LambdaExpression)function);
        }

        public void RegisterEnumerableFunction<TRet>(string name, Expression<Func<IEnumerable<object>, IEnumerable<object>, TRet>> function)
        {
            RegisterFunction(name, (LambdaExpression)function);
        }
        public void RegisterEnumerableFunction<TRet>(string name, Expression<Func<IEnumerable<object>, object, TRet>> function)
        {
            RegisterFunction(name, (LambdaExpression)function);
        }
        public void RegisterEnumerableFunction<TArg1, TRet>(string name, Expression<Func<IEnumerable<object>, TArg1, TRet>> function)
        {
            RegisterFunction(name, (LambdaExpression)function);
        }
        public void RegisterEnumerableFunction<TArg1, TArg2, TRet>(string name, Expression<Func<IEnumerable<object>, TArg1, TArg2, TRet>> function)
        {
            RegisterFunction(name, (LambdaExpression)function);
        }

        public void RegisterFunction<TRet>(string name, Expression<Func<TRet>> function)
        {
            RegisterFunction(name, (LambdaExpression)function);
        }

        public void RegisterFunction<TArg1, TRet>(string name, Expression<Func<TArg1, TRet>> function)
        {
            RegisterFunction(name, (LambdaExpression)function);
        }

        public void RegisterFunction<TArg1, TArg2, TRet>(string name, Expression<Func<TArg1, TArg2, TRet>> function)
        {
            RegisterFunction(name, (LambdaExpression)function);
        }

        public void RegisterFunction<TArg1, TArg2, TArg3, TRet>(
            string name,
            Expression<Func<TArg1, TArg2, TArg3, TRet>> function)
        {
            RegisterFunction(name, (LambdaExpression)function);
        }


        public void RegisterFunction<TArray, TRet>(
            string name,
            Expression<Func<TArray[], TRet>> function,
            bool requireOne = true)
        {
            RegisterFunction(new FunctionDefinition(name, function.Parameters, function.Body, true));
        }

        private void RegisterFunction(string name, LambdaExpression function)
        {
            RegisterFunction(new FunctionDefinition(name, function.Parameters, function.Body));
        }

        private void RegisterFunction(FunctionDefinition functionDefinition)
        {
            if (!_functions.TryGetValue(functionDefinition.Name, out var overloads))
            {
                _functions[functionDefinition.Name] = overloads = new List<FunctionDefinition>();
            }

            // TODO: check for existing overloads regarding parameters
            overloads.Add(functionDefinition);

            var parameters = new ParameterTypes(functionDefinition.Parameters.Select(p => p.Type));
            _overloadLookup[(functionDefinition.Name, parameters)] = functionDefinition;
        }

        private struct ParameterTypes : IEquatable<ParameterTypes>
        {
            private readonly int _hashCode;

            public ParameterTypes(IEnumerable<Type> types)
            {
                var typeArray = types.ToArray();

                if (typeArray.Length == 0)
                {
                    _hashCode = 0;
                }
                else
                {
                    var hashCode = typeArray[0].GetHashCode();
                    for (var i = 1; i < typeArray.Length; i++)
                    {
                        hashCode = (hashCode * 397) ^ typeArray[0].GetHashCode();
                    }

                    _hashCode = hashCode;
                }
            }

            public bool Equals(ParameterTypes other)
            {
                return _hashCode == other._hashCode;
            }

            public override bool Equals(object obj)
            {
                return obj is ParameterTypes other && Equals(other);
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            public static bool operator ==(ParameterTypes left, ParameterTypes right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(ParameterTypes left, ParameterTypes right)
            {
                return !left.Equals(right);
            }
        }

        private class FunctionDefinition
        {
            public string Name { get; }
            public IList<ParameterExpression> Parameters { get; }
            public Expression Body { get; }
            public bool RequireOneArgument { get; }

            public FunctionDefinition(
                string name,
                IList<ParameterExpression> parameters,
                Expression body,
                bool requireOneArgument = false)
            {
                Name = name;
                Parameters = parameters;
                Body = body;
                RequireOneArgument = requireOneArgument;
            }

            public Expression Rewrite(IList<Expression> arguments)
            {
                var rewriter = new ParameterRewriter(Parameters, arguments);
                return rewriter.Visit(Body);
            }

            internal class ParameterRewriter : ExpressionVisitor
            {
                private readonly IList<ParameterExpression> _parameters;
                private readonly IList<Expression> _arguments;

                public ParameterRewriter(IList<ParameterExpression> parameters, IList<Expression> arguments)
                {
                    _parameters = parameters;
                    _arguments = arguments;
                }

                protected override Expression VisitParameter(ParameterExpression node)
                {
                    var param = FindParameter(node.Name);
                    if (param != null)
                    {
                        return param;
                    }
                    return base.VisitParameter(node);
                }

                private Expression FindParameter(string name)
                {
                    for (var i = 0; i < _parameters.Count; i++)
                    {
                        if (_parameters[i].Name == name)
                        {
                            return _arguments[i];
                        }
                    }

                    return null;
                }
            }
        }
    }
}
