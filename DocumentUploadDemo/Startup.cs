using System.Data;
using System.Data.SQLite;
using DocumentUploadCore.Data;
using DocumentUploadCore.Library;
using DocumentUploadDemo.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SQLiteDataSource;

namespace DocumentUploadDemo
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

            services.AddSingleton<IDocumentManagementService, DocumentManagementService>();
            services.AddSingleton<IDbConnection>(new SQLiteConnection("Data Source=documentUploader.db"));
            services.AddSingleton<IDocumentRepository, SQLiteDocumentRepository>();
            services.AddSingleton<IFileUploadRequestFactory, FileUploadRequestFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
