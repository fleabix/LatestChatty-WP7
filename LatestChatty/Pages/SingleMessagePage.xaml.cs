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
using LatestChatty.Classes;
using Microsoft.Phone.Tasks;

namespace LatestChatty.Pages
{
	public partial class SingleMessagePage : PhoneApplicationPage
	{
		int _message;
		MessageList _box;
		public SingleMessagePage()
		{
			InitializeComponent();
			this.MessageViewer.NavigateToString(CoreServices.Instance.CommentBrowserString);
			Loaded += new RoutedEventHandler(SingleMessagePage_Loaded);
		}

		void SingleMessagePage_Loaded(object sender, RoutedEventArgs e)
		{
			MessageViewer.InvokeScript("setContent", ((Message)DataContext).body);
			if (MessageViewer.Opacity != 1) MessageViewer.Opacity = 1;
			MessageViewer.Navigating += new EventHandler<NavigatingEventArgs>(MessageViewer_Navigating);
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			bool found = false;
			string s;
			if (NavigationContext.QueryString.TryGetValue("ID", out s))
			{
				_message = int.Parse(s);
			}
			else
			{
				NavigationService.GoBack();
			}

			if (NavigationContext.QueryString.TryGetValue("Box", out s))
			{
				box whichbox = (box)Enum.Parse(typeof(box), s, true);
				switch (whichbox)
				{
					case box.inbox:
						{
							_box = CoreServices.Instance.Inbox;
							break;
						}
					case box.outbox:
						{
							_box = CoreServices.Instance.Outbox;
							break;
						}
					case box.archive:
						{
							_box = CoreServices.Instance.Archive;
							break;
						}

					default:
						{
							_box = CoreServices.Instance.Inbox;
							break;
						}
				}
			}
			else
			{
				NavigationService.GoBack();
			}

			foreach (Message m in _box.Messages)
			{
				if (m.id == _message)
				{
					this.DataContext = m;
					found = true;
					break;
				}
			}

			if (!found)
			{
				NavigationService.GoBack();
			}

			Messages.DataContext = _box;
			CoreServices.Instance.SelectedMessageChanged = SelectedMessageChanged;
			base.OnNavigatedTo(e);
		}

		protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
		{
			CoreServices.Instance.SelectedMessageChanged = null;
			base.OnNavigatingFrom(e);
		}

		public void SelectedMessageChanged(Message newSelection)
		{
			this.DataContext = newSelection;
			MessageViewer.InvokeScript("setContent", (newSelection.body));
			if (MessageViewer.Opacity != 1) MessageViewer.Opacity = 1;
		}

		void MessageViewer_Navigating(object sender, NavigatingEventArgs e)
		{
			string s = e.Uri.ToString();

			if (s.Contains("shacknews.com/chatty?id="))
			{
				int c = int.Parse(s.Split('=')[1].Split('#')[0]);
				CoreServices.Instance.Navigate(new Uri("/Pages/ThreadPage.xaml?Comment=" + c, UriKind.Relative));
				e.Cancel = true;
				return;
			}

			WebBrowserTask task = new WebBrowserTask();
			task.Uri = new Uri(s);
			task.Show();
			e.Cancel = true;
		}

		private void MessageClick(object sender, EventArgs e)
		{
			Message m = (Message)DataContext;
			CoreServices.Instance.Navigate(new Uri("/Pages/MessagePost.xaml?Subject=" + m.subject + "&To=" + m.from, UriKind.Relative));
		}
	}
}