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
using LatestChatty.Classes;

namespace LatestChatty.Controls
{
	public partial class CommentListControl : UserControl
	{
		ScrollViewer listScrollViewer;

		public CommentListControl()
		{
			InitializeComponent();
			this.listScrollViewer = FindChildOfType<ScrollViewer>(commentListBox);
		}

		private void SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ListBox lbSender = sender as ListBox;
			// If selected index is -1 (no selection) do nothing
			if (lbSender.SelectedIndex == -1)
				return;

			Comment c = lbSender.SelectedItem as Comment;

			CoreServices.Instance.Navigate(new Uri("/Pages/ThreadPage.xaml?Comment=" + c.id + "&Story=" + c.storyid, UriKind.Relative));
			lbSender.SelectedIndex = -1;
		}

		static T FindChildOfType<T>(DependencyObject root) where T : class
		{
			var queue = new Queue<DependencyObject>();
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				DependencyObject current = queue.Dequeue();
				for (int i = VisualTreeHelper.GetChildrenCount(current) - 1; 0 <= i; i--)
				{
					var child = VisualTreeHelper.GetChild(current, i);
					var typedChild = child as T;
					if (typedChild != null)
					{
						return typedChild;
					}
					queue.Enqueue(child);
				}
			}
			return null;
		}
	}
}
