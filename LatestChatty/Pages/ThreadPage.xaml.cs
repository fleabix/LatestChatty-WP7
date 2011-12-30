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
using System.Windows.Navigation;
using LatestChatty.ViewModels;
using System.IO;
using LatestChatty.Classes;
using Microsoft.Phone.Tasks;
using LatestChatty.Controls;

namespace LatestChatty.Pages
{
    public partial class ThreadPage : PhoneApplicationPage
    {
        public Rectangle SelectedFill = null;
        private int _story;
        private bool shouldStartWebBrowser = false;
        private bool staticLoad = false;
        private int _comment;
        private Comment _c;
        private CommentThread _thread;

        Microsoft.Phone.Shell.ApplicationBarIconButton _PinButton;

        public ThreadPage()
        {
            InitializeComponent();
            _PinButton = ApplicationBar.Buttons[2] as Microsoft.Phone.Shell.ApplicationBarIconButton;

            Loaded += new RoutedEventHandler(ThreadPage_Loaded);
        }

        void ThreadPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!shouldStartWebBrowser)
            {
                CommentViewer.NavigateToString("<HTML><body bgcolor='#222222'/></HTML>");
            }

            if (staticLoad)
            {
                ThreadCreated(_thread, new System.ComponentModel.PropertyChangedEventArgs("RootComment"));
            }

            CommentViewer.Navigating += new EventHandler<NavigatingEventArgs>(ContentText_Navigating);
            CoreServices.Instance.SelectedCommentChanged = SelectedCommentChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string sStory, sComment;
            if (NavigationContext.QueryString.TryGetValue("Story", out sStory))
            {
                _story = int.Parse(sStory);
            }
            else
            {
                _story = 10;
            }
            if (NavigationContext.QueryString.TryGetValue("Comment", out sComment))
            {
                _comment = int.Parse(sComment);
                _thread = CoreServices.Instance.GetCommentThread(_comment);
                if (_thread == null || _thread.RootComment.Count == 0)
                {
                    _thread = new CommentThread(_comment, _story);
                    _thread.PropertyChanged += ThreadCreated;
                    ProgressBar.Visibility = Visibility.Visible;
                }
                else
                {
                    _comment = CoreServices.Instance.GetSelectedComment();
                    staticLoad = true;
                }
                CommentsList.DataContext = _thread;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            CoreServices.Instance.AddCommentThread(_comment, _thread);
            base.OnNavigatedFrom(e);
        }

        void ThreadCreated(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                _thread.PropertyChanged -= ThreadCreated;
                _c = _thread.GetComment(_comment);
                ShowComment(_c);
                ProgressBar.Visibility = Visibility.Collapsed;
                CompositionTarget.Rendering += UpdateViewer;
            }
            catch(Exception)
            {
            }
        }

        public void SelectedCommentChanged(Comment newSelection)
        {
            ShowComment(newSelection);
        }

        private void ShowComment(Comment c)
        {
            _comment = c.id;
            CommentHeader.DataContext = c;
            CommentViewer.NavigateToString(CoreServices.Instance.AddCommentHTML(c.body));
            if (CoreServices.Instance.IsOnWatchedList(c))
            {
                _PinButton.IconUri = new Uri("/Images/sticky_notes.png", UriKind.Relative);
                _PinButton.Text = "Unpin Thread";
            }
            else
            {
                _PinButton.IconUri = new Uri("/Images/PinIcon.png", UriKind.Relative);
                _PinButton.Text = "Pin Thread";
            }
            shouldStartWebBrowser = true;
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

        private void RefreshClick(object sender, EventArgs e)
        {
            CommentThread thread = CommentsList.DataContext as CommentThread;
            thread.PropertyChanged += ThreadCreated;
            thread.Refresh();
            ProgressBar.Visibility = Visibility.Visible;
        }

        private void ReplyClick(object sender, EventArgs e)
        {
            Comment c = CommentHeader.DataContext as Comment;
            if (c != null)
            {
                CoreServices.Instance.ReplyToContext = c;
                CoreServices.Instance.Navigate(new Uri("/Pages/CommentPost.xaml?Story=" + c.storyid, UriKind.Relative));
            }
        }

        private void PinClick(object sender, EventArgs e)
        {
            bool pinned = CoreServices.Instance.AddOrRemoveWatch(((Comment)CommentHeader.DataContext));

            if (pinned)
            {
                _PinButton.IconUri = new Uri("/Images/sticky_notes.png", UriKind.Relative);
                _PinButton.Text = "Unpin Thread";
            }
            else
            {
                _PinButton.IconUri = new Uri("/Images/PinIcon.png", UriKind.Relative);
                _PinButton.Text = "Pin Thread";
            }

        }

        protected void UpdateViewer(object sender, EventArgs e)
        {
            bool found = false;
            Comment c = _c;
            NestedListBoxItem item = (NestedListBoxItem)(CommentsList.ItemContainerGenerator.ContainerFromItem(c));
            if (item != null)
            {
                item.NestedListBoxItem_Click(null, new RoutedEventArgs());
            }
            else
            {
                for (int i = 0; i < CommentsList.Items.Count; i++)
                {
                    item = (NestedListBoxItem)(CommentsList.ItemContainerGenerator.ContainerFromIndex(i));
                    item = item.GetChild(c);
                    if (item != null)
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (found)
            {
                item.NestedListBoxItem_Click(null, new RoutedEventArgs());
            }
            CompositionTarget.Rendering -= UpdateViewer;
            _c = null;
        }

    }
}