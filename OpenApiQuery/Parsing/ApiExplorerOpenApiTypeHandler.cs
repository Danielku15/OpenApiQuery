using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Parsing
{
    public class ApiExplorerOpenApiTypeHandler : DefaultOpenApiTypeHandler
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly object _typeCacheLock = new object();
        private bool _typeCacheInitialized;
        private Dictionary<string, Type> _nameToTypeCache;
        private ISet<Type> _typeCache;

        // not nice to inject the service provider but we
        // cannot inject IApiDescriptionGroupCollectionProvider directly due to circular dependencies
        // from ASP.net core side
        public ApiExplorerOpenApiTypeHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override IOpenApiType ResolveType(Type clrType)
        {
            EnsureApiTypeCacheInitialized();
            return base.ResolveType(clrType);
        }

        protected override IOpenApiType BuildOpenApiType(Type clrType)
        {
            // only API types
            if (!_typeCache.Contains(clrType))
            {
                return null;
            }

            return base.BuildOpenApiType(clrType);
        }

        private void EnsureApiTypeCacheInitialized()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if (!_typeCacheInitialized) // preliminiary bool check with lock -> once initialized we do not need to lock
            {
                // not yet initialized, we lock to ensure we initialize only once
                lock (_typeCacheLock)
                {
                    // check again so that only the first once aquiring the lock actually initializes
                    if (!_typeCacheInitialized)
                    {
                        InitializeTypeCache();
                    }

                    _typeCacheInitialized = true;
                }
            }
        }

        private void InitializeTypeCache()
        {
            _nameToTypeCache = new Dictionary<string, Type>();
            _typeCache = new HashSet<Type>();

            var apiDescriptionGroupCollectionProvider =
                _serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
            var groups = apiDescriptionGroupCollectionProvider.ApiDescriptionGroups;
            foreach (var group in groups.Items)
            {
                foreach (var apiDescription in group.Items)
                {
                    RegisterTypes(apiDescription);
                }
            }
        }

        private void RegisterTypes(ApiDescription apiDescription)
        {
            // register all responses
            foreach (var responseType in apiDescription.SupportedResponseTypes)
            {
                RegisterType(responseType.ModelMetadata.ModelType);
            }

            // register all input parameters
            foreach (var parameter in apiDescription.ParameterDescriptions)
            {
                RegisterType(parameter.ModelMetadata.ModelType);
            }
        }

        private void RegisterType(Type clrType)
        {
            if (typeof(OpenApiQueryOptions).IsAssignableFrom(clrType) && clrType.IsGenericType)
            {
                RegisterType(clrType.GetGenericArguments()[0]);
            }
            else if (clrType.IsGenericType &&
                     (clrType.GetGenericTypeDefinition() == typeof(Single<>) ||
                      clrType.GetGenericTypeDefinition() == typeof(Multiple<>) ||
                      clrType.GetGenericTypeDefinition() == typeof(Delta<>))
            )
            {
                RegisterType(clrType.GetGenericArguments()[0]);
            }
            else if (ReflectionHelper.ImplementsEnumerable(clrType, out var itemType))
            {
                RegisterType(itemType);
            }
            else if (ReflectionHelper.ImplementsDictionary(clrType, out var keyType, out var valueType))
            {
                RegisterType(keyType);
                RegisterType(valueType);
            }
            else if (IsBuiltInType(clrType))
            {
                // no type registration in case of default types
            }
            else if (clrType.IsEnum)
            {
                RegisterEnum(clrType);
            }
            else
            {
                RegisterClass(clrType);
            }
        }

        private static readonly ISet<Type> BuiltInTypes = new HashSet<Type>
        {
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(string),
            typeof(char),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset),
            typeof(Guid),
            typeof(Uri),
        };

        private bool IsBuiltInType(Type clrType)
        {
            return BuiltInTypes.Contains(clrType);
        }

        private void RegisterClass(Type clrType)
        {
            _nameToTypeCache[GetJsonName(clrType)] = clrType;
            _typeCache.Add(clrType);

            // register subtypes
            var knownSubTypes = clrType.Assembly.GetTypes().Where(t => t.IsSubclassOf(clrType));
            foreach (var knownSubType in knownSubTypes)
            {
                RegisterClass(knownSubType);
            }
        }

        private void RegisterEnum(Type clrType)
        {
            _nameToTypeCache[GetJsonName(clrType)] = clrType;
            _typeCache.Add(clrType);
        }

        private string GetJsonName(Type clrType)
        {
            return clrType.Name.Split('`').First();
        }

        public override IOpenApiType ResolveType(string jsonName)
        {
            EnsureApiTypeCacheInitialized();

            if (_nameToTypeCache.TryGetValue(jsonName, out var clrType))
            {
                return base.ResolveType(clrType);
            }

            return null;
        }
    }
}
