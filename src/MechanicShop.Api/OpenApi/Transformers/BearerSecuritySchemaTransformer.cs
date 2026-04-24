using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
namespace MechanicShop.Api.OpenApi.Transformers;

public sealed class BearerSecuritySchemaTransformer : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    private const string schemeId = JwtBearerDefaults.AuthenticationScheme;

    Task IOpenApiDocumentTransformer.TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes[schemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter JWT token in the format: Bearer {token}",
            Name = "Authorization",
        };

        return Task.CompletedTask;
    }

    Task IOpenApiOperationTransformer.TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any())
        {
            operation.Security ??= [];

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference(schemeId),
                    new List<string>()
                }
            });
        }

        return Task.CompletedTask;
    }
}
