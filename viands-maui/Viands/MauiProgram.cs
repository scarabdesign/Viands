using Camera.MAUI;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Radzen;
using System.Reflection;
using System.Text.Json;
using Viands.Support;

namespace Viands
{
    public static class MauiProgram
    {
        [JSInvokable("OnJavaScriptMessage")]
        public static void OnJavaScriptMessage(string JSONResult)
        {
            JavaScriptMessageRequest req = JsonSerializer.Deserialize<JavaScriptMessageRequest>(JSONResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            });

            if(req != null && !string.IsNullOrEmpty(req.Method))
            {
                var method = (GlobalCallbacks.CBKeys)Enum.Parse(typeof(GlobalCallbacks.CBKeys), req.Method);
                GlobalCallbacks.Trigger(method, req.Message);
            }
            
        }

        public static MauiApp CreateMauiApp()
        {
            string[] resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("Viands.appsettings.json");

            var config = new ConfigurationBuilder()
                        .AddJsonStream(stream)
                        .Build();

            var builder = MauiApp.CreateBuilder();
            builder.Configuration.AddConfiguration(config);
            builder
                .UseMauiApp<App>()
                .UseMauiCameraView()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddScoped<DialogService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<TooltipService>();
            builder.Services.AddScoped<ContextMenuService>();
            builder.Services.AddTransient<MainPage>();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Services.AddBlazorWebView();
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}