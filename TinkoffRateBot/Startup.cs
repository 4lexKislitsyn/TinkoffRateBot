using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TinkoffRateBot.Background;
using TinkoffRateBot.BotCommands;
using TinkoffRateBot.DataAccess;
using TinkoffRateBot.DataAccess.Interfaces;
using TinkoffRateBot.Services;

namespace TinkoffRateBot
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
            services.Configure<BotConfiguration>(Configuration.GetSection("Bot"));

            var awsEnvSection = Configuration.GetSection("AWSEnvironment");

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")))
            {
                Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", awsEnvSection.GetValue<string>("AccessKey"));
            }
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")))
            {
                Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", awsEnvSection.GetValue<string>("SecretKey"));
            }
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_REGION")))
            {
                Environment.SetEnvironmentVariable("AWS_REGION", awsEnvSection.GetValue<string>("Region"));
            }

            var awsOptions = Configuration.GetAWSOptions();
            awsOptions.Credentials = new EnvironmentVariablesAWSCredentials();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddTransient<IDynamoDBContext, DynamoDBContext>();
            services.AddTransient<IRepository, DynamoDBRepository>();

            services.Configure<TinkoffRateTimedConfiguration>(Configuration.GetSection(nameof(TinkoffRateTimedHostedService)));
            services.AddHostedService<TinkoffRateTimedHostedService>();
            services.AddHostedService<TelegramBotGetUpdatesHostedService>();

            services.AddTransient<IBotCommand, StartCommand>();
            services.AddTransient<IBotCommand, PauseCommand>();
            services.AddTransient<IBotCommand, DetailedCommand>();
            services.AddTransient<TelegramMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
            await app.ApplicationServices.GetService<IRepository>().InitializeDBAsync(app.ApplicationServices);
        }
    }
}
