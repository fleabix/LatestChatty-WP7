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
using LatestChatty.Controls;

namespace LatestChatty
{
	public partial class MainPage : PhoneApplicationPage
	{
		LoginControl _login;
		private int _refreshing = 0;

		// Constructor
		public MainPage()
		{
			InitializeComponent();
			CoreServices.Instance.WatchList.RefreshWatchList();
			if (!CoreServices.Instance.LoginVerified)
			{
				LoginText.Text = "login";
			}
			Loaded += new RoutedEventHandler(MainPage_Loaded);
		}

		void MainPage_Loaded(object sender, RoutedEventArgs e)
		{
			Pinned.DataContext = CoreServices.Instance.WatchList;
			MyPosts.DataContext = CoreServices.Instance.MyPosts;
			MyReplies.DataContext = CoreServices.Instance.MyReplies;

			//Need to implement this for the watch list.
			CoreServices.Instance.MyPosts.PropertyChanged += RefreshCompleted;
			CoreServices.Instance.MyReplies.PropertyChanged += RefreshCompleted;

			if (CoreServices.Instance.LoginVerified)
			{
				CoreServices.Instance.MyPosts.Refresh();
				IncrementRefresher();
				CoreServices.Instance.MyReplies.Refresh();
				IncrementRefresher();
			}
		}

		private void Chatty_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.CancelDownloads();
			NavigationService.Navigate(new Uri("/Pages/ChattyPage.xaml", UriKind.Relative));
		}

		private void Stories_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.CancelDownloads();
			NavigationService.Navigate(new Uri("/Pages/HeadlinesPage.xaml", UriKind.Relative));
		}

		private void Messages_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.CancelDownloads();
			NavigationService.Navigate(new Uri("/Pages/MessagesPage.xaml", UriKind.Relative));
		}

		private void Search_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.CancelDownloads();
			NavigationService.Navigate(new Uri("/Pages/SearchPage.xaml", UriKind.Relative));
		}

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.CancelDownloads();
			NavigationService.Navigate(new Uri("/Pages/SettingsPage.xaml", UriKind.Relative));
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.CancelDownloads();
			NavigationService.Navigate(new Uri("/Pages/AboutPage.xaml", UriKind.Relative));
		}

		private void Login_Click(object sender, RoutedEventArgs e)
		{
			if (!CoreServices.Instance.LoginVerified)
			{
				_login = new LoginControl(LoginCallback);
				LayoutRoot.Children.Add(_login);
			}
			else
			{
				CoreServices.Instance.Logout();
				LoginText.Text = "login";
			}
		}

		public void LoginCallback(bool verified)
		{
			if (verified)
			{
				LoginText.Text = "logout";
				CoreServices.Instance.MyPosts.Refresh();
				CoreServices.Instance.MyReplies.Refresh();
				IncrementRefresher();
				IncrementRefresher();
			}
			LayoutRoot.Children.Remove(_login);
			_login = null;
		}

		protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
		{
			if (_login != null)
			{
				LayoutRoot.Children.Remove(_login);
				_login = null;
				e.Cancel = true;
			}
			else
			{
				base.OnBackKeyPress(e);
			}
		}

		private void MyPosts_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.MyPosts.Refresh();
			IncrementRefresher();
		}

		private void MyReplies_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.MyReplies.Refresh();
			IncrementRefresher();
		}

		private void Pinned_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.WatchList.RefreshWatchList();
			//IncrementRefresher();
		}

		private void IncrementRefresher()
		{
			_refreshing++;
			if (_refreshing > 0)
			{
				ProgressBar.Visibility = Visibility.Visible;
				ProgressBar.IsIndeterminate = true;
			}
		}

		private void DecrementRefresher()
		{
			_refreshing--;
			if (_refreshing == 0)
			{
				ProgressBar.Visibility = Visibility.Collapsed;
				ProgressBar.IsIndeterminate = false;
			}
		}

		void RefreshCompleted(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			DecrementRefresher();
		}
	}
}