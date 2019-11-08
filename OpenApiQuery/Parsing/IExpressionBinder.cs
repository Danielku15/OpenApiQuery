using System.Collections.Generic;
using System.Reflection;

namespace OpenApiQuery.Parsing
{
    public interface IExpressionBinder
    {
        MemberInfo BindMember(System.Linq.Expressions.Expression instance, string memberName);
        System.Linq.Expressions.Expression BindFunctionCall(string identifier, List<System.Linq.Expressions.Expression> arguments);
    }
}