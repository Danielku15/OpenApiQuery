using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace OpenApiQuery
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    internal sealed class OpenApiQueryParameterBindingAttribute : ModelBinderAttribute
    {
        public OpenApiQueryParameterBindingAttribute()
            : base(typeof(OpenApiQueryParameterBinding))
        {
        }

        private class OpenApiQueryParameterBinding : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (bindingContext == null)
                {
                    throw new ArgumentNullException(nameof(bindingContext));
                }

                if (IsQueryOptions(bindingContext.ModelType))
                {
                    var instance = (OpenApiQueryOptions) ActivatorUtilities.CreateInstance(
                        bindingContext.HttpContext.RequestServices,
                        bindingContext.ModelType);

                    try
                    {
                        instance.Initialize(bindingContext.HttpContext, bindingContext.ModelState);
                        bindingContext.Result = ModelBindingResult.Success(instance);
                    }
                    catch
                    {
                        // TODO how to do better reporting?
                        bindingContext.Result = ModelBindingResult.Failed();
                    }
                }

                return Task.CompletedTask;
            }

            private static bool IsQueryOptions(Type bindingContextModelType)
            {
                return bindingContextModelType.IsGenericType &&
                       bindingContextModelType.GetGenericTypeDefinition() == typeof(OpenApiQueryOptions<,>);
            }
        }
    }
}
