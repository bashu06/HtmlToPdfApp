using DinkToPdf;
using DinkToPdf.Contracts;
using HtmlToPdfApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace HtmlToPdfApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Increase Kestrel limits for large requests
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
            });

            // Configure form options for larger payloads
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            // Add controllers with JSON options
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            // Register HTML to PDF converter
            var context = new CustomAssemblyLoadContext();
            var libraryPath = Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.so");

            try
            {
                context.LoadUnmanagedLibrary(libraryPath);
                services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            }
            catch (Exception ex)
            {
                // Log the error but allow the application to continue
                Console.WriteLine($"Error loading wkhtmltopdf library: {ex.Message}");
                throw; // Re-throw to fail startup if library can't be loaded
            }

            // Register the PDF service
            services.AddScoped<HtmlToPdfConverter>();

            // Register Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "HTML to PDF API",
                    Version = "v1",
                    Description = "Convert HTML content to PDF documents"
                });
            });

            // Add CORS support
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            // Enable CORS
            app.UseCors("AllowAll");

            app.UseRouting();

            // Enable request logging
            app.Use(async (context, next) =>
            {
                // Log the start of the request
                logger.LogInformation($"Request started: {context.Request.Method} {context.Request.Path}");

                await next();

                // Log the end of the request
                logger.LogInformation($"Request completed: {context.Response.StatusCode}");
            });

            // Enable Swagger and SwaggerUI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "HTML to PDF API v1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at app root
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(string absolutePath)
        {
            try
            {
                return LoadUnmanagedDll(absolutePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load unmanaged library: {ex.Message}");
                throw;
            }
        }

        protected override System.Reflection.Assembly Load(System.Reflection.AssemblyName assemblyName)
        {
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            try
            {
                Console.WriteLine($"Loading library: {unmanagedDllName}");
                return NativeLibrary.Load(unmanagedDllName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading native library: {ex.Message}");
                throw;
            }
        }
    }
}