using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System.Diagnostics;
using Viands.Support;

namespace Viands
{
    public partial class MainPage : ContentPage
    {
        private IConfiguration configuration;
        public MainPage(IConfiguration config)
        {
            MakeConfiguration(config);
            Data.Constants.InitDatabase(() =>
            {
                NavTools.InitTools();
                NavTools.AdjustToolset(NavTools.ToolSetTypes.ListsHome);
                InitializeComponent();
            });
        }

        public async void ShowBarcodeView(Action<List<string>> onclose)
        {
            try
            {
                var barcodeView = new BarcodeView();
                var barcodes = await this.ShowPopupAsync(barcodeView);
                if (barcodeView != null)
                {
                    await barcodeView.StopCamera();
                }
                if(barcodes != null)
                {
                    onclose?.Invoke(barcodes as List<string>);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex); }   
        }

        private void MakeConfiguration(IConfiguration config)
        {
            configuration = config;
            var settings = configuration.GetRequiredSection("Settings").Get<AppSettings>();
            if(settings != null && settings.APIEndpoint.ContainsKey(settings.env))
            {
                Data.Constants.APIEndpoint = settings.APIEndpoint[settings.env];
            }   
        }

        private void Bwv_BlazorWebViewInitialized(object sender, Microsoft.AspNetCore.Components.WebView.BlazorWebViewInitializedEventArgs e)
        {
#if WINDOWS
            e.WebView.CoreWebView2.Settings.IsZoomControlEnabled = false;
#endif
        }

        //protected override void OnSizeAllocated(double width, double height)
        //{
        //    base.OnSizeAllocated(width, height); 
        //}
    }
}