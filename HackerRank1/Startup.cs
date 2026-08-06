using LibraryService.WebAPI.Application.Interfaces;
using LibraryService.WebAPI.Application.Services;
using LibraryService.WebAPI.Domain.Interfaces;
using LibraryService.WebAPI.Infrastructure.Data;
using LibraryService.WebAPI.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace LibraryService.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Dependency Injection
            services.AddTransient<ILibrariesService, LibrariesService>();
            services.AddTransient<IBooksService, BooksService>();
            services.AddTransient<ILibraryRepository, LibraryRepository>();
            services.AddTransient<IBookRepository, BookRepository>();

            // Configuración de PostgreSQL (Supabase)
            services.AddDbContext<LibraryContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("DefaultConnection")
                ));

            services.AddControllers(options =>
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

            // Swagger
            services.AddSwaggerGen(c =>
            {
                if (!c.SwaggerGeneratorOptions.SwaggerDocs.ContainsKey("v1"))
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "LibraryService API",
                        Version = "v1",
                        Description = "A simple example ASP.NET Core Web API for LibraryService"
                    });
                }
            });
        }

        // This method gets called by the runtime.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LibraryService API v1");
                });
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}