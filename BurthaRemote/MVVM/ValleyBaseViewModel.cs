using BurthaRemote;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MVVM
{
    public class ValleyBaseViewModel : ObservableRecipient
    {

        private bool _thinking;
        private string _thinkingText;

        public bool Thinking
        {
            get => _thinking;
            set => SetProperty(ref _thinking, value);
        }

        public string ThinkingText
        {
            get => _thinkingText;
            set => SetProperty(ref _thinkingText, value);
        }

        /// <summary>
        /// Standard dialog, to keep consistency.
        /// Long term, better to put this into base viewmodel class, along with MVVM stuff (NotifyProperyCHanged, etc) and inherrit it.
        /// Note that the Secondarytext can be un-assigned, then the econdary button won't be presented.
        /// Result is true if the user presses primary text button
        /// </summary>
        /// <param name="title">
        /// The title of the message dialog
        /// </param>
        /// <param name="message">
        /// THe main body message displayed within the dialog
        /// </param>
        /// <param name="primaryText">
        /// Text to be displayed on the primary button (which returns true when pressed).
        /// If not set, defaults to 'OK'
        /// </param>
        /// <param name="secondaryText">
        /// The (optional) secondary button text.
        /// If not set, it won't be presented to the user at all.
        /// </param>
        public async Task<bool> ShowDialog(string title, string message, string primaryText = "OK", string secondaryText = null)
        {
            bool result = false;

            try
            {
                if (App.rootFrame != null)
                {
                    var d = new ContentDialog();

                    d.Title = title;
                    d.Content = message;
                    d.PrimaryButtonText = primaryText;

                    RevealBorderBrush myBrush = new RevealBorderBrush();
                    myBrush.Color = (Color)App.rootFrame.Resources["SystemAccentColor"];
                    myBrush.FallbackColor = Color.FromArgb(255, 202, 24, 37);
                    myBrush.Opacity = 0.6;
                    d.BorderBrush = myBrush;
                    d.BorderThickness = new Windows.UI.Xaml.Thickness(2);

                    d.Background = (SolidColorBrush)App.rootFrame.Resources["ApplicationPageBackgroundThemeBrush"];

                    if (!string.IsNullOrEmpty(secondaryText))
                    {
                        d.SecondaryButtonText = secondaryText;
                    }

                    Task<bool> dialogTask = App.dispatcherQueue.EnqueueAsync(async () =>
                    {
                        var dr = await d.ShowAsync();
                        return (dr == ContentDialogResult.Primary);
                    });

                    result = await dialogTask;
                }
            }
            catch (Exception ex)
            {
                Microsoft.AppCenter.Crashes.Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    { "Class", this.GetType().Name },
                    { "Method", System.Reflection.MethodBase.GetCurrentMethod().Name },
                    { "ExceptionVar", "ex"}
                });
            }
            finally
            {

            }

            return result;

        }


    }
}
