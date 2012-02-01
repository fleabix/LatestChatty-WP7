using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LatestChatty.Classes;
using LatestChatty.Controls;
using LatestChatty.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;

namespace LatestChatty.Pages
{
	public partial class ThreadPage : PhoneApplicationPage
	{
		public Rectangle SelectedFill = null;
		private CommentThread thread;
		private ApplicationBarMenuItem pinMenuItem;

		public ThreadPage()
		{
			System.Diagnostics.Debug.WriteLine("Thread - ctor");
			InitializeComponent();
			this.pinMenuItem = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
			this.commentBrowser.NavigateToString(CoreServices.Instance.CommentBrowserString);
		}

        private void SelectComment(Comment c)
        {
            this.thread.SelectComment(c);
        }

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Thread - OnNavigatedTo - URI: {0}", e.Uri);
			string sStory, sComment;
			var storyId = NavigationContext.QueryString.TryGetValue("Story", out sStory) ? int.Parse(sStory) : 10;
            CoreServices.Instance.SelectedCommentChanged += SelectComment;

			if (NavigationContext.QueryString.TryGetValue("Comment", out sComment))
			{
				var commentId = int.Parse(sComment);
				if (this.thread != null)
				{
					this.thread.PropertyChanged -= ThreadPropertyChanged;
				}

				//Attempt to get the thread from cache.  This means it was the last thread we viewed.
				this.thread = CoreServices.Instance.GetCommentThread(commentId);
				if (this.thread == null)
				{
					//If it wasn't the last thread viewed, we'll load the browser with the javascript needed to make changing text in the browser not flicker.
					this.commentBrowser.NavigateToString(CoreServices.Instance.CommentBrowserString);
					this.thread = new CommentThread(commentId, storyId);
				}

				this.DataContext = this.thread;

                //TODO: Handle when we've been brought here via own posts or replies so that we highlight the correct post.
                // Right now we'll only highlight the root.
                var selectedCommentId = CoreServices.Instance.GetSelectedComment();
                if (selectedCommentId != 0 && this.thread.SelectedComment != null)
                {
                    //Should already be a selected comment, since we had it saved from last time.
                    CoreServices.Instance.SetCurrentSelectedComment(this.thread.SelectedComment);
                }
                else
                {
                    this.thread.SelectComment(commentId);
                }
				//TODO: This is so dirty.
				//When trying to data bind directly to the Text property if the DataContext isn't available right away 
				// (and in this case it never will be), an exception is thrown because an ApplicationBarMenuItem cannot have an empty Text property.
				// So... I guess I'll do it this way for now.  Ugh.
				this.thread.PropertyChanged += ThreadPropertyChanged;

			}
		}

		void ThreadPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals("IsWatched"))
			{
				this.SetPinnedMenuText();
			}
		}

		void SetPinnedMenuText()
		{
			this.pinMenuItem.Text = this.thread.IsWatched ? "unpin thread" : "pin thread";
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
            CoreServices.Instance.SelectedCommentChanged -= SelectComment;
			System.Diagnostics.Debug.WriteLine("Thread - OnNavigatedFrom");
			CoreServices.Instance.CancelDownloads();
			if (this.thread.RootComment.Count > 0)
			{
				CoreServices.Instance.SetCurrentCommentThread(thread);
			}
			base.OnNavigatedFrom(e);
		}

		void ContentText_Navigating(object sender, NavigatingEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Thread - Navigating");

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

		private void RefreshClick(object sender, EventArgs e)
		{
			CommentThread thread = CommentsList.DataContext as CommentThread;
			thread.Refresh();
		}

		private void ReplyClick(object sender, EventArgs e)
		{
			var c = this.thread.SelectedComment;
			if (c != null)
			{
				CoreServices.Instance.ReplyToContext = c;
				CoreServices.Instance.Navigate(new Uri("/Pages/CommentPost.xaml?Story=" + c.storyid, UriKind.Relative));
			}
		}

		private void NextClick(object sender, EventArgs e)
		{
            if (this.thread.SelectedComment != null)
            {
                var next = this.thread.GetFlattenedComments().OrderBy(c => c.id).FirstOrDefault(c => c.id > this.thread.SelectedComment.id);
                if(next != null) this.thread.SelectComment(next);
            }
		}

		private void PreviousClick(object sender, EventArgs e)
		{
            if (this.thread.SelectedComment != null)
            {
                var prev = this.thread.GetFlattenedComments().OrderBy(c => c.id).LastOrDefault(c => c.id < this.thread.SelectedComment.id);
                if (prev != null) this.thread.SelectComment(prev);
            }
		}

		private void PinClick(object sender, EventArgs e)
		{
			thread.TogglePinned();
		}

		private void ShareThreadClick(object sender, EventArgs e)
		{
			var shareTask = new ShareLinkTask
			{
				LinkUri = new Uri("http://www.shacknews.com/chatty/?id=" + thread._id.ToString(), UriKind.Absolute),
				Message = "Check out this thread on shacknews.com",
				Title = "Shack Chatty Thread"
			};
			shareTask.Show();
		}

		private void OpenInBrowserClick(object sender, EventArgs e)
		{
			var browserTask = new WebBrowserTask
			{
				Uri = new Uri("http://www.shacknews.com/chatty/?id=" + thread._id.ToString(), UriKind.Absolute)
			};
			browserTask.Show();
		}
	}
}