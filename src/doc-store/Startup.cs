using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using doc_store.Store;
using Swashbuckle.AspNetCore.Swagger;
using CustomSerilogFormatter;
using CorrelationId;
using SerilogEnricher;
using PerformanceLog;
using HealthCheck;

namespace doc_store
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            CustomSerilogConfigurator.Setup("doc-store", false);
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var identityServer = Configuration["IdentityServerUrl"];

            // Add framework services.
            services.AddMvc();
            services.AddLogging();

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IDocumentStore, DocumentStore>();

            services.AddSwaggerGen();

            services.AddSwaggerGen(options =>
           {
               options.SwaggerDoc("v1", new Info
               {
                   Version = "v1",
                   Title = "doc-store",
                   Description = "manage the document store",
                   TermsOfService = "None",
                   Contact = new Contact { Name = "Bastian Töpfer", Email = "bastian.toepfer@gmail.com", Url = "http://github.com/schwamster/docStack" }
               });

               options.AddSecurityDefinition("oauth2", new OAuth2Scheme
               {
                   Type = "oauth2",
                   Flow = "implicit",
                   AuthorizationUrl = $"{identityServer}/connect/authorize",
                   Scopes = new Dictionary<string, string>
                   {
                        { "doc-store", "doc-store" }
                   }
               });

                // Assign scope requirements to operations based on AuthorizeAttribute
                options.OperationFilter<SecurityRequirementsOperationFilter>();

                //Determine base path for the application.
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;

                //Set the comments path for the swagger json and ui.
                options.IncludeXmlComments(GetXmlCommentsPath());
           });

        }

        private string GetXmlCommentsPath()
        {
            var app = PlatformServices.Default.Application;
            return System.IO.Path.Combine(app.ApplicationBasePath, System.IO.Path.ChangeExtension(app.ApplicationName, "xml"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var identityServer = Configuration["IdentityServerUrl"];

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCorrelationIdMiddleware(new CorrelationIdMiddlewareOptions());
            app.UseSerilogEnricherMiddleware();
            app.UsePerformanceLog(new PerformanceLogOptions());
            app.UseHealthcheckEndpoint(new HealthCheckOptions() { Message = "Its alive" });
            
            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                Authority = $"{identityServer}",
                ApiName = "doc-store",
                RequireHttpsMetadata = false,
            });

            app.UseMvc();

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();

            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUi(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "doc-stack-app-api");

                //c.ConfigureOAuth2("doc-stack-app-api-swagger", null, "swagger-ui-realm", "Swagger UI");
            });

        }
    }
}
