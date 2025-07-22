using System.Reflection;
using System.Text.Json.Serialization;
using Application.Dtos.Quests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Application.Common.Swagger
{
    public class PolymorphicResponseFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (string.IsNullOrEmpty(context.ApiDescription.HttpMethod) || !context.ApiDescription.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                return;

            var hasQuestTypeParameter = context.ApiDescription.ParameterDescriptions
                .Any(p => p.Name.Equals("questType", StringComparison.OrdinalIgnoreCase) &&
                          p.Source == BindingSource.Path);

            if (!hasQuestTypeParameter)
                return;

            var baseType = GetBaseDtoType(context.MethodInfo.ReturnType);
            if (baseType is null || baseType != typeof(BaseGetQuestDto))
                return;

            if (!operation.Responses.TryGetValue("200", out var response))
                return;

            var deriverTypes = baseType.GetCustomAttributes<JsonDerivedTypeAttribute>();
            if (!deriverTypes.Any())
                return;

            response.Content.Clear();

            foreach (var attr in deriverTypes)
            {
                var schema = context.SchemaGenerator.GenerateSchema(attr.DerivedType, context.SchemaRepository);

                var key = attr.TypeDiscriminator?.ToString() ?? attr.DerivedType.Name;

                response.Content.Add(
                    key,
                    new OpenApiMediaType { Schema = schema });
            }
        }

        // Helper method to dig through Task<ActionResult<T>> to find T
        private static Type? GetBaseDtoType(Type returnType)
        {
            if (returnType.IsGenericType)
            {
                var genericArgs = returnType.GetGenericArguments();
                foreach (var arg in genericArgs)
                {
                    // Check if the generic argument is our base DTO or is itself a generic
                    // that might contain our base DTO (like ActionResult<T>)
                    if (arg == typeof(BaseGetQuestDto))
                        return arg;
                    if (arg.IsGenericType && arg.GetGenericTypeDefinition() == typeof(ActionResult<>))
                    {
                        var innerType = arg.GetGenericArguments().FirstOrDefault();
                        if (innerType == typeof(BaseGetQuestDto))
                        {
                            return innerType;
                        }
                    }

                    // Recursively check deeper
                    var nestedType = GetBaseDtoType(arg);
                    if (nestedType != null)
                        return nestedType;
                }
            }
            return null;
        }
    }
}
