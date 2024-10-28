global using FluentResults;
using Bank.Client.Authentications;
using Bank.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace Bank.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });


            // Set configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json") 
                .Build();
            builder.Configuration.AddConfiguration(config);


            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();

            builder.Services.AddHttpClient("ServerApi")
                  .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration["ServerUrl"] ?? ""))
                  .AddHttpMessageHandler<AuthenticationHandler>();


            // Adding authentication services
            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();            
            builder.Services.AddSingleton<AuthenticationService>();
            builder.Services.AddTransient<AuthenticationHandler>();

            // Adding transaction service
            builder.Services.AddSingleton<TransactionService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
