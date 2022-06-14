using CefSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.UI.ViewManagement;

namespace CefsharpHwndHostTouchKeyboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Lazy<(IInputPaneInterop ipi, IInputPane2 ip)> sip;

        public MainWindow()
        {
            InitializeComponent();

            sip = new Lazy<(IInputPaneInterop ipi, IInputPane2 ip)>(() =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                var ipi = InputPane.As<IInputPaneInterop>();
                var ip = ipi.GetForWindow(hwnd, typeof(IInputPane2).GUID);
                return (ipi, ip);
            });

            var oskSubject = new Subject<bool>();
            cwb.RenderProcessMessageHandler = new OskRenderProcessMessageHandler(oskSubject.OnNext);
            oskSubject
                .Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOn(SynchronizationContext.Current ?? throw new InvalidOperationException("No syncctx"))
                .Subscribe(PopOsk);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (sip.IsValueCreated)
            {
                var (ipi, ip) = sip.Value;
                Marshal.FinalReleaseComObject(ip);
                Marshal.FinalReleaseComObject(ipi);
            }
            base.OnClosed(e);
        }

        private void PopOsk(bool shouldShow)
        {
            var (_, ip) = sip.Value;
            if (shouldShow)
            {
                Debug.WriteLine($"Showing SIP");
                ip.TryShow();
            }
            else
            {
                Debug.WriteLine($"Hiding SIP");
                ip.TryHide();
            }
        }
    }
    
    internal class OskRenderProcessMessageHandler : IRenderProcessMessageHandler
    {
        private readonly Action<bool> SetOsk;

        public OskRenderProcessMessageHandler(Action<bool> popOsk)
        {
            this.SetOsk = popOsk;
        }

        public void OnContextCreated(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
            // nop
        }

        public void OnContextReleased(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
            // nop
        }

        public void OnFocusedNodeChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IDomNode? node)
        {
            SetOsk(node != null && "input".Equals(node.TagName, StringComparison.InvariantCultureIgnoreCase));
        }

        public void OnUncaughtException(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, JavascriptException exception)
        {
            // nop
        }
    }

    [ComImport, Guid("75CF2C57-9195-4931-8332-F0B409E916AF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IInputPaneInterop
    {
        void _VtblGap1_3();

        IInputPane2 GetForWindow([In] IntPtr appWindow, [In] ref Guid riid);
    }

    [ComImport, Guid("8A6B3F26-7090-4793-944C-C3F2CDE26276"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IInputPane2
    {
        void _VtblGap1_3();

        bool TryShow();
        bool TryHide();
    }
}
