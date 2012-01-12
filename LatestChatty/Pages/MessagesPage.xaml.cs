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
using LatestChatty.ViewModels;
using LatestChatty.Controls;

namespace LatestChatty.Pages
{
	public partial class MessagesPage : PhoneApplicationPage
	{
		LoginControl _login;
		public MessagesPage()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			if (!CoreServices.Instance.LoginVerified)
			{
				_login = new LoginControl(LoginCallback);
				LayoutRoot.Children.Add(_login);
			}
			else
			{
				LoadPage();
			}
			base.OnNavigatedTo(e);
		}

		public void LoadPage()
		{
			if (CoreServices.Instance.Inbox == null)
			{
				CoreServices.Instance.Inbox = new MessageList(box.inbox);
				ProgressBar.Visibility = Visibility.Visible;
				ProgressBar.IsIndeterminate = true;
				CoreServices.Instance.Inbox.PropertyChanged += BoxLoaded;
			}
			else
			{
				((FrameworkElement)Pivot.Items[0]).DataContext = CoreServices.Instance.Inbox;
				if (CoreServices.Instance.Outbox == null)
				{
					CoreServices.Instance.Outbox = new MessageList(box.outbox);
					ProgressBar.Visibility = Visibility.Visible;
					ProgressBar.IsIndeterminate = true;
					CoreServices.Instance.Outbox.PropertyChanged += BoxLoaded;
				}
				else
				{
					((FrameworkElement)Pivot.Items[1]).DataContext = CoreServices.Instance.Outbox;
					if (CoreServices.Instance.Archive == null)
					{
						CoreServices.Instance.Archive = new MessageList(box.archive);
						ProgressBar.Visibility = Visibility.Visible;
						ProgressBar.IsIndeterminate = true;
						CoreServices.Instance.Archive.PropertyChanged += BoxLoaded;
					}
					else
					{
						((FrameworkElement)Pivot.Items[2]).DataContext = CoreServices.Instance.Archive;
					}
				}
			}
		}

		public void BoxLoaded(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			MessageList messages = sender as MessageList;
			messages.PropertyChanged -= BoxLoaded;

			if (messages == CoreServices.Instance.Inbox)
			{
				((FrameworkElement)Pivot.Items[0]).DataContext = CoreServices.Instance.Inbox;
			}
			else if (messages == CoreServices.Instance.Outbox)
			{
				((FrameworkElement)Pivot.Items[1]).DataContext = CoreServices.Instance.Outbox;
			}
			else
			{
				((FrameworkElement)Pivot.Items[2]).DataContext = CoreServices.Instance.Archive;
			}

			if (CoreServices.Instance.Outbox == null)
			{
				CoreServices.Instance.Outbox = new MessageList(box.outbox);
				((FrameworkElement)Pivot.Items[1]).DataContext = CoreServices.Instance.Outbox;
				ProgressBar.Visibility = Visibility.Visible;
				ProgressBar.IsIndeterminate = true;
				CoreServices.Instance.Outbox.PropertyChanged += BoxLoaded;
			}
			else if (CoreServices.Instance.Archive == null)
			{
				CoreServices.Instance.Archive = new MessageList(box.archive);
				((FrameworkElement)Pivot.Items[2]).DataContext = CoreServices.Instance.Archive;
				ProgressBar.Visibility = Visibility.Visible;
				ProgressBar.IsIndeterminate = true;
				CoreServices.Instance.Archive.PropertyChanged += BoxLoaded;
			}
			else
			{
				ProgressBar.Visibility = Visibility.Collapsed;
				ProgressBar.IsIndeterminate = false;
			}
		}

		private void RefreshClick(object sender, EventArgs e)
		{
			if (!CoreServices.Instance.LoginVerified && _login == null)
			{
				_login = new LoginControl(LoginCallback);
				LayoutRoot.Children.Add(_login);
				return;
			}

			MessageList messages;
			switch (Pivot.SelectedIndex)
			{
				case 0:
					messages = CoreServices.Instance.Inbox;
					break;
				case 1:
					messages = CoreServices.Instance.Outbox;
					break;
				case 2:
					messages = CoreServices.Instance.Archive;
					break;

				default:
					messages = CoreServices.Instance.Inbox;
					break;
			}

			messages.PropertyChanged += BoxLoaded;
			messages.Refresh();
			ProgressBar.Visibility = Visibility.Visible;
			ProgressBar.IsIndeterminate = true;
		}

		public void LoginCallback(bool verified)
		{
			if (verified)
			{
				LoadPage();
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
			}
			else
			{
				base.OnBackKeyPress(e);
			}
		}

		private void NewMessageClick(object sender, EventArgs e)
		{
			CoreServices.Instance.Navigate(new Uri("/Pages/MessagePost.xaml", UriKind.Relative));
		}
	}
}