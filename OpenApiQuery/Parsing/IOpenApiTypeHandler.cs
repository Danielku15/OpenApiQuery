using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenApiQuery.Parsing
{
    public interface IOpenApiTypeHandler
    {
        IOpenApiType ResolveType(Type clrType);
        IOpenApiType ResolveType(string jsonName);

        PropertyInfo BindProperty(System.Linq.Expressions.Expression instance, string memberName);

        System.Linq.Expressions.Expression BindFunctionCall(string identifier,
            List<System.Linq.Expressions.Expression> arguments);

    }
}
