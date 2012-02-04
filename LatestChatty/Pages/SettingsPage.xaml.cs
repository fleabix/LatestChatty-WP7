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
using LatestChatty.Classes;

namespace LatestChatty.Pages
{
	public partial class SettingsPage : PhoneApplicationPage
	{
		private bool loaded = false;

		public SettingsPage()
		{
			InitializeComponent();

			if (ScheduledActionService.Find("LatestChatty") != null)
			{
				PushEnabled.IsChecked = true;
			}

			CommentViewSize browserSize;
			CoreServices.Instance.Settings.TryGetValue<CommentViewSize>(SettingsConstants.CommentSize, out browserSize);
			switch (browserSize)
			{
				case CommentViewSize.Half:
					this.sizePicker.SelectedIndex = 1;
					break;
				case CommentViewSize.Huge:
					this.sizePicker.SelectedIndex = 2;
					break;
				default:
					this.sizePicker.SelectedIndex = 0;
					break;
			}

			bool byDate;
			CoreServices.Instance.Settings.TryGetValue<bool>(SettingsConstants.ThreadNavigationByDate, out byDate);
			this.navigationPicker.SelectedIndex = byDate ? 0 : 1;
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			this.loaded = true;
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
			ScheduledActionService.LaunchForTest("LatestChatty", TimeSpan.FromSeconds(1));
		}

		private void PagePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ListPicker lp = sender as ListPicker;
		}

		private void CommentSizePickerChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!this.loaded) return;

			var picker = sender as ListPicker;
			if (picker != null)
			{
				var sizeText = ((ListPickerItem)picker.SelectedItem).Tag as string;
				switch (sizeText)
				{
					case "Small":
						CoreServices.Instance.Settings[SettingsConstants.CommentSize] = CommentViewSize.Small;
						break;
					case "Half":
						CoreServices.Instance.Settings[SettingsConstants.CommentSize] = CommentViewSize.Half;
						break;
					case "Huge":
						CoreServices.Instance.Settings[SettingsConstants.CommentSize] = CommentViewSize.Huge;
						break;

					default:
						CoreServices.Instance.Settings[SettingsConstants.CommentSize] = CommentViewSize.Small;
						break;
				}
			}
			CoreServices.Instance.Settings.Save();
		}

		private void NextBehaviorPickerChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!this.loaded) return;

			var picker = sender as ListPicker;
			if (picker != null)
			{
				CoreServices.Instance.Settings[SettingsConstants.ThreadNavigationByDate] = Boolean.Parse(((ListPickerItem)picker.SelectedItem).Tag as string);
			}
			CoreServices.Instance.Settings.Save();
		}
	}
}