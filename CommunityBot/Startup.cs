using System.Data.SQLite;
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
            
            services.AddSingleton<BotService>();
            services.AddSingleton<IChatRepository, SqliteChatRepository>();
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
            services.AddSingleton(provider =>
            {
                //todo use directory from docket volume
                var connection = new SQLiteConnection("DataSource=\"C:/Users/gjmrd/projects/CommunityBot/database.sqlite\";");
                connection.Open();
                return connection;
            });
        }
    }
}