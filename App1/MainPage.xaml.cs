using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Service.GotHere += s =>
            {
                button.Content = s;
            };
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var result = Service.DoThreadedWork(Dispatcher);

            button.Content = result.ToString();
        }
    }

    public static class Service
    {
        public static ContentDialogResult DoThreadedWork(CoreDispatcher dispatcher)
        {
            var result = ContentDialogResult.None;
            var thread = new Thread(DoWork);
            var waiter = new ManualResetEvent(false);

            thread.Start();

            waiter.WaitOne();

            return result;

            async void DoWork()
            {
                var client = new HttpClient();

                var response = await client.GetAsync(new Uri("https://www.microsoft.com"));

                if (dispatcher.HasThreadAccess)
                {
                    result = await Handle("Not dispatched.");
                    return;
                }

                result = await dispatcher.AwaitableRunAsync(async () => await Handle("Dispatched"));
                
                async Task<ContentDialogResult> Handle(string message)
                {
                    GotHere?.Invoke(message);

                    var dialog = new ContentDialog
                    {
                        Content = response.StatusCode,
                        PrimaryButtonText = "Open",
                        SecondaryButtonText = "Cancel"
                    };

                    return await dialog.ShowAsync();
                }
            }
        }

        public static event Action<string> GotHere;
    }
}
