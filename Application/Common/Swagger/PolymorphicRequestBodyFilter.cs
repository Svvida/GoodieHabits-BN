using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Dtos.Quests;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Application.Common.Swagger
{
    public class PolymorphicRequestBodyFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // --- GUARD CLAUSES: Make sure this is the right endpoint ---
            var questTypeParam = context.ApiDescription.ParameterDescriptions
                .FirstOrDefault(p => p.Name.Equals("questType", StringComparison.OrdinalIgnoreCase));

            var jsonElementParam = context.ApiDescription.ParameterDescriptions
                .FirstOrDefault(p => p.Type == typeof(JsonElement));

            if (questTypeParam == null || jsonElementParam == null)
                return;

            var httpMethod = context.ApiDescription.HttpMethod;
            if (string.IsNullOrEmpty(httpMethod) || !(httpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase) || httpMethod.Equals("PUT", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            // --- CORE LOGIC: Find the application/json schema and modify it ---
            if (!operation.RequestBody.Content.TryGetValue("application/json", out var openApiMediaType))
            {
                return; // No application/json request body to modify
            }

            // Determine which base DTO to use based on the HTTP method
            Type baseType = httpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)
                ? typeof(BaseCreateQuestDto)
                : typeof(BaseUpdateQuestDto);

            var derivedTypes = baseType.GetCustomAttributes<JsonDerivedTypeAttribute>();
            if (!derivedTypes.Any())
                return;

            // This is the key part: We create a list of schemas for the 'oneOf' keyword
            var oneOfSchemas = new List<OpenApiSchema>();
            foreach (var attr in derivedTypes)
            {
                // We need a schema reference for each derived type
                var schema = context.SchemaGenerator.GenerateSchema(attr.DerivedType, context.SchemaRepository);
                oneOfSchemas.Add(new OpenApiSchema { Reference = new OpenApiReference { Id = attr.DerivedType.Name, Type = ReferenceType.Schema } });
            }

            // Now, modify the existing schema to use 'oneOf'
            openApiMediaType.Schema.OneOf = oneOfSchemas;

            // Clear out any old properties, as 'oneOf' takes precedence
            openApiMediaType.Schema.Properties.Clear();
            openApiMediaType.Schema.Type = null;
        }
    }
}
