using Foundation;
using UIKit;

namespace Viands
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();


        [Export("application:supportedInterfaceOrientationsForWindow:")]
        public UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
        {
            if (forWindow.WindowScene != null && forWindow.WindowScene.Title == "PerformOrientation")
            {
                return UIInterfaceOrientationMask.All;
            }
            else
            {
                return application.SupportedInterfaceOrientationsForWindow(forWindow);
            }
        }
    }
}