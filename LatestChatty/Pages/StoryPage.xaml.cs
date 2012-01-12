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
	public partial class StoryPage : PhoneApplicationPage
	{
		private int _story;
		private bool shouldStartWebBrowser = false;

		public StoryPage()
		{
			InitializeComponent();
			CommentViewer.NavigateToString(CoreServices.Instance.CommentBrowserString);
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			string sStory;
			if (NavigationContext.QueryString.TryGetValue("Story", out sStory))
			{
				_story = int.Parse(sStory);
			}

			bool create = false;
			StoryDetail detail = CoreServices.Instance.GetStoryDetail(_story, ref create);
			if (create)
			{
				ProgressBar.Visibility = Visibility.Visible;
				ProgressBar.IsIndeterminate = true;
				detail.PropertyChanged += detail_PropertyChanged;
			}
			else
			{
				DataContext = detail.Detail;
				Loaded += new RoutedEventHandler(StoryPage_Loaded);
			}
			base.OnNavigatedTo(e);
		}

		void StoryPage_Loaded(object sender, RoutedEventArgs e)
		{
			CommentViewer.InvokeScript("setContent", ((Story)DataContext).body);
			if (CommentViewer.Opacity != 1) CommentViewer.Opacity = 1;
			CommentViewer.Navigating += new EventHandler<NavigatingEventArgs>(ContentText_Navigating);
			CommentFooter.Visibility = Visibility.Visible;
			shouldStartWebBrowser = true;
		}

		void detail_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			try
			{
				StoryDetail s = sender as StoryDetail;
				s.PropertyChanged -= detail_PropertyChanged;
				ProgressBar.Visibility = Visibility.Collapsed;
				ProgressBar.IsIndeterminate = false;
				DataContext = s.Detail;
				CommentViewer.InvokeScript("setContent", (s.Detail.body));
				if (CommentViewer.Opacity != 1) CommentViewer.Opacity = 1;
				CommentViewer.Navigating += new EventHandler<NavigatingEventArgs>(ContentText_Navigating);
				CommentFooter.Visibility = Visibility.Visible;
				shouldStartWebBrowser = true;
			}
			catch
			{
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.Navigate(new Uri("/Pages/ChattyPage.xaml?Story=" + _story, UriKind.Relative));
		}

		void ContentText_Navigating(object sender, NavigatingEventArgs e)
		{
			if (shouldStartWebBrowser)
			{
				string s = e.Uri.ToString();

				if (s.Contains("shacknews.com/chatty?id="))
				{
					int c = int.Parse(s.Split('=')[1].Split('#')[0]);
					CoreServices.Instance.Navigate(new Uri("/Pages/ThreadPage.xaml?Comment=" + c, UriKind.Relative));
					e.Cancel = true;
					shouldStartWebBrowser = false;
					return;
				}

				WebBrowserTask task = new WebBrowserTask();
				task.Uri = new Uri(s);
				task.Show();
				e.Cancel = true;
				shouldStartWebBrowser = false;
			}
		}
	}
}