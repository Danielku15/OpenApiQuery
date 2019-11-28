using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Serialization
{
    public class OpenApiQueryResultOutputFormatter : TextOutputFormatter
    {
        private static readonly MediaTypeHeaderValue ApplicationJson
            = MediaTypeHeaderValue.Parse("application/json").CopyAsReadOnly();

        private static readonly MediaTypeHeaderValue TextJson
            = MediaTypeHeaderValue.Parse("text/json").CopyAsReadOnly();

        private static readonly MediaTypeHeaderValue ApplicationAnyJsonSyntax
            = MediaTypeHeaderValue.Parse("application/*+json").CopyAsReadOnly();


        public OpenApiQueryResultOutputFormatter()
        {
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add(ApplicationJson);
            SupportedMediaTypes.Add(TextJson);
            SupportedMediaTypes.Add(ApplicationAnyJsonSyntax);
        }


        protected override bool CanWriteType(Type type)
        {
            return type == typeof(OpenApiQueryResult);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context,
            Encoding selectedEncoding)
        {
            var result = (OpenApiQueryResult) context.Object;
            return WriteResponseBodyAsyncOfT.Invoke(this, context, selectedEncoding,
                result.QueryOptions.ElementType);
        }

        private static readonly GenericMethodCallHelper<OpenApiQueryResultOutputFormatter, OutputFormatterWriteContext, Encoding, Task>
            WriteResponseBodyAsyncOfT =  new GenericMethodCallHelper<OpenApiQueryResultOutputFormatter, OutputFormatterWriteContext, Encoding, Task>(
                x => x.WriteResponseBodyAsync<object>(null, null)
            );

        private async Task WriteResponseBodyAsync<T>(OutputFormatterWriteContext context,
            Encoding selectedEncoding)
            where T : new()
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (selectedEncoding == null)
            {
                throw new ArgumentNullException(nameof(selectedEncoding));
            }

            var result = (OpenApiQueryResult) context.Object;
            var queryable = (IQueryable<T>) result.Queryable;
            var appliedQueryResult = await result.QueryOptions.ApplyTo(queryable, context.HttpContext.RequestAborted);

            var httpContext = context.HttpContext;
            var writeStream = httpContext.Response.Body;

            var serializerProvider = httpContext.RequestServices.GetRequiredService<IOpenApiQuerySerializerProvider>();
            var serializer = serializerProvider.GetSerializerForResult(httpContext, result, appliedQueryResult);

            var serializationContext = new OpenApiSerializationContext<T>(result, appliedQueryResult);
            await serializer.SerializeResultAsync(writeStream, serializationContext, httpContext.RequestAborted);

            await writeStream.FlushAsync(httpContext.RequestAborted);
        }
    }
}
