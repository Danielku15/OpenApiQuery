using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using OpenApiQuery;

namespace OpenApiQuery.NSwag.Processors
{    
    public class OpenApiQueryOperationProcessor : IOperationProcessor
    {
        private string _notice =
@"> ### **oData Enabled Action**
> This controller is enabled for oData filtering. All of the query parameters are optional. If no filter is
> provided, it will return all matching records (which may have a limit based on the API implementation).
> For more information about oData filtering, see the following document:
>
> https://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part2-url-conventions.html";

        Type ResponseType = typeof(OpenApiQueryResult);
        Type QueryOptionsType = typeof(OpenApiQueryOptions<>);
        Type ApplyResultType = typeof(OpenApiQueryApplyResult<>);
        bool _addNotice = false;


        /// <summary>
        /// Construct the operation processor
        /// </summary>
        public OpenApiQueryOperationProcessor()
        {
        }

        /// <summary>
        /// Construct the operation processor
        /// </summary>
        /// <param name="addDefaultNotice">If true, the default oData notice will be added to all oData enabled operations</param>
        public OpenApiQueryOperationProcessor(bool addDefaultNotice)
        {
            _addNotice = addDefaultNotice;
        }

        /// <summary>
        /// Construct the operation processor
        /// </summary>
        /// <param name="notice">Notice to add to the top of all oData enabled operations</param>
        public OpenApiQueryOperationProcessor(string notice)
        {
            _notice = notice;
        }

        public bool Process(OperationProcessorContext context)
        {
            MethodInfo opMethod = context.MethodInfo;

            // only affect operations that use the correct response type and also have the correct parameters
            if (opMethod.ReturnType == ResponseType)
            {
                List<ParameterInfo> parameters =
                    opMethod.GetParameters()
                            .Where(x => x.ParameterType.IsGenericType == true
                                     && x.ParameterType.GenericTypeArguments.Count() == 1
                                     && x.ParameterType.GetGenericTypeDefinition() == QueryOptionsType)
                            .ToList();

                if (parameters.Count() == 1)
                {
                    ParameterInfo queryParameter = parameters[0];
                    
                    //
                    // Here we override the parameters so that instead of it showing an object
                    // that is required to be passed as a single parameter, it shows each individual
                    // oData filter parameters as its own parameter.
                    //

                    IList<OpenApiParameter> opParams = context.OperationDescription.Operation.Parameters;
                    JsonSchema optionsSchema = opParams[0].ActualSchema;
                    var props = optionsSchema.InheritedSchema.ActualProperties;

                    opParams.Clear();
                    foreach (var prop in props)
                    {
                        opParams.Add(new OpenApiParameter()
                        {
                            Name = prop.Key,
                            Schema = prop.Value,
                            Kind = OpenApiParameterKind.Query
                        });
                    }

                    
                    //
                    // Update the return type to reflect what actually gets written to the body
                    //

                    // get the generic provided
                    Type queryType = queryParameter.ParameterType.GenericTypeArguments[0];

                    // generate the generic return type
                    Type resultType = ApplyResultType.MakeGenericType(new Type[] { queryType });

                    JsonSchema schema = context.SchemaGenerator.Generate(resultType, context.SchemaResolver);

                    IDictionary<string, OpenApiResponse> responses = context.OperationDescription.Operation.Responses;

                    if (responses.Any(x => x.Key == "200"))
                        responses["200"].Schema = schema;


                    //
                    // optionally set some additional information regarding this operation
                    //
                    // Now we add the extra info at the top
                    if (_addNotice)
                    {
                        string opDescription = context.OperationDescription.Operation.Description;

                        if (_notice != null)
                        {
                            if (opDescription == null)
                                opDescription = _notice;
                            else
                                opDescription += Environment.NewLine + Environment.NewLine + _notice;
                        }

                        context.OperationDescription.Operation.Description = opDescription;
                    }
                }
            }

            return true;
        }
    }

    public class OpenApiQueryNameGenerator : ISchemaNameGenerator
    {
        public string Generate(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(OpenApiQueryOptions<>))
                return type.Name.Substring(0, type.Name.IndexOf('`'));
            else
                return type.Name;
        }
    }
}