using System.Data.SQLite;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommunityBot.Contracts;
using CommunityBot.Helpers;
using CommunityBot.Persistence;
using CommunityBot.Services;

namespace CommunityBot
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
            services.Configure<BotConfigurationOptions>(Configuration.GetSection(BotConfigurationOptions.SectionName))
                .AddTelegramBotClient()
                .AddUpdateHandlers();
            
            //todo add connection to DI container

            services.AddSingleton<BotService>();
            services.AddSingleton<IChatRepository, ChatRepository>();
            services.AddSingleton<IMediaGroupService, MediaGroupService>();
            ConfigureDatabase(services);
            
            services.AddControllers();
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

            //app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
        
        private static void ConfigureDatabase(IServiceCollection services)
        {
            services.AddTransient(provider =>
            {
                if (!File.Exists("file"))
                {
                    //todo
                }
                var connection = new SQLiteConnection("");
                connection.Open();
                return connection;
            });
        }
    }
}