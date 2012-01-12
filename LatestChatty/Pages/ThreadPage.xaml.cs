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
		private bool shouldStartWebBrowser = false;
		private CommentThread thread;
		private ApplicationBarMenuItem pinMenuItem;

		public ThreadPage()
		{
			InitializeComponent();
			this.pinMenuItem = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
			this.commentBrowser.NavigateToString(CoreServices.Instance.CommentBrowserString);
			Loaded += new RoutedEventHandler(ThreadPage_Loaded);
		}

		void ThreadPage_Loaded(object sender, RoutedEventArgs e)
		{
			//Ultimately it would be sweet to have two way binding with SelectedItem on the thread view and the SelectedComment on the CommentThread object.
			CoreServices.Instance.SelectedCommentChanged = (c) => { this.shouldStartWebBrowser = true; this.thread.SelectComment(c); };
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("OnNavigatedTo");
			//TODO: This stuff probably doesn't work quite right.  I haven't looked into persisting when navigated away from.
			string sStory, sComment;
			var storyId = NavigationContext.QueryString.TryGetValue("Story", out sStory) ? int.Parse(sStory) : 10;

			if (NavigationContext.QueryString.TryGetValue("Comment", out sComment))
			{
				var commentId = int.Parse(sComment);
				if (this.thread != null)
				{
					this.thread.PropertyChanged -= ThreadPropertyChanged;
				}

				this.thread = CoreServices.Instance.GetCommentThread(commentId);
				if (this.thread == null)
				{
					this.thread = new CommentThread(commentId, storyId);
				}
				else
				{
					this.thread.SelectComment(CoreServices.Instance.GetSelectedComment());
				}

				//TODO: This is so dirty.
				//When trying to data bind directly to the Text property if the DataContext isn't available right away 
				// (and in this case it never will be), an exception is thrown because an ApplicationBarMenuItem cannot have an empty Text property.
				// So... I guess I'll do it this way for now.  Ugh.
				this.thread.PropertyChanged += ThreadPropertyChanged;
				this.DataContext = this.thread;
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
			System.Diagnostics.Debug.WriteLine("OnNavigatedFrom");
			CoreServices.Instance.AddCommentThread(this.thread.RootComment.First().id, thread);
			base.OnNavigatedFrom(e);
		}

		void ContentText_Navigating(object sender, NavigatingEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine(string.Format("Navigating: {0}", this.shouldStartWebBrowser));

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
			MessageBox.Show("Not implemented.");
			//TODO: I don't want to write the recursive search.  And this way didn't work because the listbox is nested the same way.  Boo.
			var newSelectedIndex = Math.Min(CommentsList.Items.Count, CommentsList.SelectedIndex + 1);
			var newComment = CommentsList.Items[newSelectedIndex] as Comment;
			this.thread.SelectComment(newComment);
		}

		private void PreviousClick(object sender, EventArgs e)
		{
			MessageBox.Show("Not implemented.");
			var newSelectedIndex = Math.Max(0, CommentsList.SelectedIndex - 1);
			var newComment = CommentsList.Items[newSelectedIndex] as Comment;
			this.thread.SelectComment(newComment);
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