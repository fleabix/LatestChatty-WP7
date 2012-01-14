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
using System.Collections.ObjectModel;
using LatestChatty.ViewModels;
using System.IO.IsolatedStorage;
using LatestChatty.Controls;

namespace LatestChatty.Pages
{
	public partial class ChattyPage : PhoneApplicationPage
	{
		private int storyId = 17;
		private CommentList comments;
		public ChattyPage()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			string sStory;
			if (NavigationContext.QueryString.TryGetValue("Story", out sStory))
			{
				storyId = int.Parse(sStory);
			}

			if (this.comments == null || this.comments.Comments.Count == 0)
			{
				this.comments = new CommentList(storyId, 1);
			}

			this.DataContext = this.comments;

			base.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
		{
			if (this.comments.Comments.Count > 0)
			{
				CoreServices.Instance.AddStoryComments(storyId, comments);
			}
			CoreServices.Instance.CancelDownloads();
			base.OnNavigatedFrom(e);
		}

		private void RefreshClick(object sender, EventArgs e)
		{
			this.comments.Refresh();
		}

		private void PostClick(object sender, EventArgs e)
		{
			CoreServices.Instance.ReplyToContext = null;
			CoreServices.Instance.Navigate(new Uri("/Pages/CommentPost.xaml?Story=" + storyId, UriKind.Relative));
		}
	}
}