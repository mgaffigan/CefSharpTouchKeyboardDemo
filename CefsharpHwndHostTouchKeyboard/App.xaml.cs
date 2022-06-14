using CefSharp;
using CefSharp.Wpf.HwndHost;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CefsharpHwndHostTouchKeyboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ConfigureCef();

            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private static void ConfigureCef()
        {
            Cef.EnableHighDPISupport();

            var settings = new CefSettings();

            // https://stackoverflow.com/questions/64495513/cefsharp-with-winform-onfocusednodechanged-never-called-when-the-user-changes-t
            CefSharpSettings.FocusedNodeChangedEnabled = true;

            // https://bugs.chromium.org/p/chromium/issues/detail?id=491516
            settings.CefCommandLineArgs.Add("disable-usb-keyboard-detect", "1");

            Cef.Initialize(settings);
        }
    }
}
