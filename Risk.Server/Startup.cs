using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Risk.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatBlazor;

namespace Risk.Server
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServerSideBlazor()
                .AddCircuitOptions(o => 
                {
                    o.DetailedErrors = true;
                });
            services.AddRazorPages();
            services.AddSignalR();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.AllowAnyHeader()
                               .AllowAnyMethod()
                               // to allow browser clients: https://github.com/dotnet/aspnetcore/issues/4457#issuecomment-465669576
                               .SetIsOriginAllowed(host => true)
                               .AllowCredentials();
                    });
            });

            services.AddSingleton<GameRunner>();
            services.AddSingleton<RiskHub>();

            services.AddSingleton(GameInitializer.InitializeGame(
                int.Parse(Configuration["height"] ?? "5"),
                int.Parse(Configuration["width"] ?? "5"),
                int.Parse(Configuration["startingArmies"] ?? "5")));

            services.AddMatBlazor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapHub<RiskHub>("/riskhub");
            });
        }
    }
}
