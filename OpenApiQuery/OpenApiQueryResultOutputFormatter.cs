using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace OpenApiQuery
{
    public class OpenApiQueryResultOutputFormatter : TextOutputFormatter
    {
        private static readonly MethodInfo WriteResponseBodyAsyncInfo =
            typeof(OpenApiQueryResultOutputFormatter).GetMethod(nameof(WriteResponseBodyAsync),
                BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly ConcurrentDictionary<Type,
                Func<OpenApiQueryResultOutputFormatter, OutputFormatterWriteContext, Encoding, Task>>
            WriteResponseBodyAsyncCache =
                new ConcurrentDictionary<Type,
                    Func<OpenApiQueryResultOutputFormatter, OutputFormatterWriteContext, Encoding, Task>>();

        private static Task WriteResponseBodyAsyncOfT(
            OpenApiQueryResultOutputFormatter formatter,
            OutputFormatterWriteContext context,
            Type elementType,
            Encoding selectedEncoding)
        {
            if (!WriteResponseBodyAsyncCache.TryGetValue(elementType, out var taskFactory))
            {
                WriteResponseBodyAsyncCache[elementType] = taskFactory = BuildWriteResponseBodyAsync(elementType);
            }

            return taskFactory(formatter, context, selectedEncoding);
        }

        private static Func<OpenApiQueryResultOutputFormatter, OutputFormatterWriteContext, Encoding, Task>
            BuildWriteResponseBodyAsync(Type elementType)
        {
            // We generate a lambda which calls the generic method with the correct type as generic
            //  Task WriteResponseBodyAsyncCacheOfT(OpenApiQueryResultOutputFormatter formatter,
            //         OutputFormatterWriteContext context,
            //         Type elementType,
            //         Encoding encoding)
            //  {
            //      return formatter.WriteResponseBodyAsync<elementType>(context, encoding);
            //  }

            var formatterParameter = Expression.Parameter(typeof(OpenApiQueryResultOutputFormatter), "formatter");
            var contextParameter = Expression.Parameter(typeof(OutputFormatterWriteContext), "context");
            var encodingParameter = Expression.Parameter(typeof(Encoding), "encoding");

            var methodToCall = WriteResponseBodyAsyncInfo.MakeGenericMethod(elementType);
            var body = Expression.Call(
                formatterParameter,
                methodToCall,
                contextParameter,
                encodingParameter
            );

            return Expression
                .Lambda<Func<OpenApiQueryResultOutputFormatter, OutputFormatterWriteContext, Encoding, Task>>(
                    body,
                    formatterParameter, contextParameter, encodingParameter
                ).Compile();
        }

        private static readonly MediaTypeHeaderValue ApplicationJson
            = MediaTypeHeaderValue.Parse("application/json").CopyAsReadOnly();

        private static readonly MediaTypeHeaderValue TextJson
            = MediaTypeHeaderValue.Parse("text/json").CopyAsReadOnly();

        private static readonly MediaTypeHeaderValue ApplicationAnyJsonSyntax
            = MediaTypeHeaderValue.Parse("application/*+json").CopyAsReadOnly();

        public JsonSerializerOptions SerializerOptions { get; }

        public OpenApiQueryResultOutputFormatter(JsonSerializerOptions serializerSettings)
        {
            SerializerOptions = serializerSettings;

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
            return WriteResponseBodyAsyncOfT(this, context, result.QueryOptions.ElementType, selectedEncoding);
        }

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

            // TODO: respect select options
            var httpContext = context.HttpContext;
            var writeStream = httpContext.Response.Body;
            var objectType = appliedQueryResult.GetType();
            
            await JsonSerializer.SerializeAsync(writeStream, appliedQueryResult, objectType, SerializerOptions);
            await writeStream.FlushAsync();
        }
    }
}