using LibraryService.WebAPI.Data;
using LibraryService.WebAPI.Features.Books.CreateBook;
using LibraryService.WebAPI.Features.Books.DeleteBook;
using LibraryService.WebAPI.Features.Books.GetBooks;
using LibraryService.WebAPI.Features.Books.UpdateBook;
using LibraryService.WebAPI.Features.Libraries.CreateLibrary;
using LibraryService.WebAPI.Features.Libraries.DeleteLibrary;
using LibraryService.WebAPI.Features.Libraries.GetLibraries;
using LibraryService.WebAPI.Features.Libraries.GetLibraryById;
using LibraryService.WebAPI.Features.Libraries.UpdateLibrary;
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
            // Register the vertical slice feature handlers
            services.AddTransient<GetLibrariesHandler>();
            services.AddTransient<GetLibraryByIdHandler>();
            services.AddTransient<CreateLibraryHandler>();
            services.AddTransient<UpdateLibraryHandler>();
            services.AddTransient<DeleteLibraryHandler>();
            services.AddTransient<GetBooksHandler>();
            services.AddTransient<CreateBookHandler>();
            services.AddTransient<UpdateBookHandler>();
            services.AddTransient<DeleteBookHandler>();

            services.AddDbContext<LibraryContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddControllers(options =>
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

            // Add Swagger generation
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                // Enable middleware to serve swagger-ui, specifying the Swagger JSON endpoint.
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