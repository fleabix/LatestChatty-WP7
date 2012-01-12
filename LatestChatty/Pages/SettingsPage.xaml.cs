using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;
using System.IO.IsolatedStorage;

namespace LatestChatty.Pages
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
          
            if (ScheduledActionService.Find("LatestChatty") != null)
            {
                PushEnabled.IsChecked = true;
            }

        }

        private void AddChatty_Click(object sender, RoutedEventArgs e)
        {
            ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("ChattyPage"));

            if (tile == null)
            {
                StandardTileData data = new StandardTileData
                {
                    BackgroundImage = new Uri("Background.png", UriKind.Relative),
                    Title = "GoToChatty"
                };

                ShellTile.Create(new Uri("/Pages/ChattyPage.xaml", UriKind.Relative), data);
            }
       }

        private void TogglePush_Checked(object sender, RoutedEventArgs e)
        {
            if (ScheduledActionService.Find("LatestChatty") == null)
            {
                PeriodicTask task = new PeriodicTask("LatestChatty");
                task.Description = "Periodically checks for replies to threads you posted in LatestChatty.  Updates LiveTile with results.";

                task.ExpirationTime = DateTime.Now.AddDays(14);
                try
                {
                    ScheduledActionService.Add(task);
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Can't schedule agent; either there are too many other agents scheduled or you have disabled this agent in Settings.");
                    return;
                }
            }
        }

        private void TogglePush_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ScheduledActionService.Find("LatestChatty") != null)
                ScheduledActionService.Remove("LatestChatty");
        }

        private void Test(object sender, RoutedEventArgs e)
        {
            ScheduledActionService.LaunchForTest("LatestChatty",TimeSpan.FromSeconds(1));
        }

        private void PagePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListPicker lp = sender as ListPicker;
        }
    }
}