using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RESTAPI.Authorization;
using RESTAPI.Cache;
using RESTAPI.Database;
using RESTAPI.Export;
using RESTAPI.Hashing;
using RESTAPI.Mailing;
using RESTAPI.Storage;
using System;

namespace RESTAPI
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
            services.AddControllers();

            services
                .AddSingleton<IDatabaseAccess, ElasticDatabaseAccess>()
                .AddSingleton<IStorageProvider, MinioStorageProvider>()
                .AddSingleton<IAuthorization, JWTAuthorization>()
                .AddSingleton<IHasher, Argon2Hasher>()
                .AddSingleton<ICacheProvider>(new InternalCacheProvider(TimeSpan.FromMinutes(10)))
                .AddSingleton<IExportWorkerHandler, ExportWorkerHandler>()
                .AddSingleton<IMailService, MailService>()
                ;

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "voidsearch API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors(c =>
                    c.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger");
                options.RoutePrefix = "swagger";
            });
        }
    }
}
