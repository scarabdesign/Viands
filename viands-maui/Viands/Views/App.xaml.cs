using Microsoft.JSInterop;
using System.Diagnostics;
using Viands.Support;

namespace Viands
{
    public partial class App : Application
    {
        public Window AppWindow;
        public Size WindowSize { 
            get; 
            set; 
        }

        public App(MainPage page)
        {
            InitializeComponent();
            MainPage = page;
        }

        //public U Create<U, V>(V constructorArgs)
        //{
        //    var instance = (U)Activator.CreateInstance(typeof(U), constructorArgs);
        //    //OnCreated?.Invoke();
        //    return instance;
        //}

        protected override Window CreateWindow(IActivationState activationState)
        {
            DeviceDisplay.Current.MainDisplayInfoChanged += Current_MainDisplayInfoChanged;

            AppWindow = base.CreateWindow(activationState);
            AppWindow.SizeChanged += AppWindow_SizeChanged;
            return AppWindow;
        }

        private void Current_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            OnResize();
        }

        private void AppWindow_SizeChanged(object sender, EventArgs e)
        {
            OnResize();
        }

        private void OnResize()
        {
            var currentSize = GetWindowSize();
            if (currentSize != WindowSize || DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                WindowSize = currentSize;
                GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.WindowResized, currentSize);
                GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.RefreshState, null);
            }
        }

        public Size GetWindowSize()
        {
            return new Size(AppWindow.Width, AppWindow.Height);
        }

    }

}