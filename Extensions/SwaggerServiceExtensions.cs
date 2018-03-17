using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace EvanLindseyApi.Extensions
{
    public static class SwaggerServiceExtensions
    {
        private const string API_TITLE = "evanlindsey.net API";
        private const string API_VERSION = "v1";

        public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
        {
            services.AddSwaggerGen(action =>
                    {
                        action.SwaggerDoc("v1", new Info { Title = API_TITLE, Version = API_VERSION });
                        action.AddSecurityDefinition("Bearer", new ApiKeyScheme
                        {
                            Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                            Name = "Authorization",
                            In = "header",
                            Type = "apiKey"
                        });
                        action.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                        {
                            { "Bearer", new string[] { } }
                        });
                    });
            return services;
        }

        public static IApplicationBuilder UseSwaggerDocs(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", API_TITLE + " " + API_VERSION));
            return app;
        }
    }
}
