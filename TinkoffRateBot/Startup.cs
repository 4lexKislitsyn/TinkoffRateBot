using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TinkoffRateBot.Background;
using TinkoffRateBot.BotCommands;
using TinkoffRateBot.DataAccess;
using TinkoffRateBot.DataAccess.Interfaces;
using TinkoffRateBot.Services;

namespace TinkoffRateBot
{
    public class Startup
    {
        private const string EnvTokenVariableName = "TinkoffRateBot:Token";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            HostEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddMvc().AddNewtonsoftJson();

            RegisterAwsServices(services);

            services.Configure<TinkoffRateTimedConfiguration>(Configuration.GetSection(nameof(TinkoffRateTimedHostedService)));
            services.AddHostedService<TinkoffRateTimedHostedService>();

            RegisterTelegramBotServices(services);

            services.Configure<BasicAuthHandlerConfiguration>(Configuration.GetSection(nameof(BasicAuthHandler)));
            services.AddAuthentication(AuthenticationSchemes.Basic.ToString())
                .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>(AuthenticationSchemes.Basic.ToString(), AuthenticationSchemes.Basic.ToString(), null);
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(AuthenticationSchemes.Basic.ToString())
                    .RequireAuthenticatedUser()
                    .Build();
            });
        }

        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            await app.ApplicationServices.GetService<IRepository>().InitializeDBAsync(app.ApplicationServices);
        }

        private static void UpdateEnvironmentVariable(string environmentKey, string configurationKey, IConfigurationSection section)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(environmentKey)))
            {
                Environment.SetEnvironmentVariable(environmentKey, section.GetValue<string>(configurationKey));
            }
        }

        private void RegisterAwsServices(IServiceCollection services)
        {
            var awsEnvSection = Configuration.GetSection("AWSEnvironment");

            void UpdateEnvironmentVariable(string environmentKey, string configurationKey)
                => Startup.UpdateEnvironmentVariable(environmentKey, configurationKey, awsEnvSection);

            UpdateEnvironmentVariable("AWS_ACCESS_KEY_ID", "AccessKey");
            UpdateEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "SecretKey");
            UpdateEnvironmentVariable("AWS_REGION", "Region");

            var awsOptions = Configuration.GetAWSOptions();
            awsOptions.Credentials = new EnvironmentVariablesAWSCredentials();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonDynamoDB>();

            services.AddTransient<IDynamoDBContext, DynamoDBContext>();
            services.AddTransient<IRepository, DynamoDBRepository>();
        }

        private void RegisterTelegramBotServices(IServiceCollection services)
        {
            services.AddTransient<IBotCommand, StartCommand>();
            services.AddTransient<IBotCommand, PauseCommand>();
            services.AddTransient<IBotCommand, ThresholdCommand>();
            services.AddTransient<TelegramMessageSender>();
            services.AddTransient<TelegramUpdateHandler>();

            var token = Environment.GetEnvironmentVariable(EnvTokenVariableName) ?? Configuration.GetSection("Bot").GetValue<string>("Token");
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException($"Provide token for bot via environment variable \"{EnvTokenVariableName}\"");
            }
            services.AddTransient<ITelegramBotClient, TelegramBotClient>(provider => new TelegramBotClient(token));
        }
    }
}
