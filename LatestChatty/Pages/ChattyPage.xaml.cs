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
        private int _story = 17;
        private int _pageCount = 3;
        public List<CommentList> Pages { get; private set; }
        public ChattyPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
 	        string sStory;
            if (NavigationContext.QueryString.TryGetValue("Story", out sStory))
            {
                _story = int.Parse(sStory);
            }

            _pageCount = CoreServices.Instance.GetPageCount();

            if (Pages == null || Pages.Count != _pageCount || Pivot.Items.Count != _pageCount)
            {
                // Balance Pivot Items with PageCount
                if (Pivot.Items.Count != _pageCount)
                {
                    if (Pivot.Items.Count < _pageCount)
                    {
                        for (int i = Pivot.Items.Count; i < _pageCount; i++)
                        {
                            PivotItem item = new PivotItem();
                            item.Header = "page" + (i + 1);
                            item.Content = new CommentListControl();
                            Pivot.Items.Add(item);
                        }
                    }
                    else
                    {
                        for (int i = Pivot.Items.Count; i > _pageCount; i--)
                        {
                            Pivot.Items.RemoveAt(i - 1);
                        }
                    }
                }

                // Restore from CoreServices cache
                Pages = CoreServices.Instance.GetStoryComments(_story);

                // Download
                if (Pages == null || Pages[0].Comments.Count == 0)
                {
                    Pages = new List<CommentList>();
                    AddPage(1);
                    ProgressBar.Visibility = Visibility.Visible;
                }
                else
                {
                    int smaller = Math.Min(_pageCount, Pages.Count);
                    for (int i = 0; i < smaller; i++)
                    {
                        ((FrameworkElement)Pivot.Items[i]).DataContext = Pages[i];
                    }

                    if (_pageCount > Pages.Count)
                    {
                        AddPage(Pages.Count + 1);
                    }                    
                }
            }
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            CoreServices.Instance.AddStoryComments(_story, Pages);
            base.OnNavigatedFrom(e);
        }

        public void PageLoaded(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Pages.Count == 1)
            {
                _story = Pages[0].Comments.First().storyid;
            }

            Pages[Pages.Count - 1].PropertyChanged -= PageLoaded;
            if (Pages.Count < _pageCount)
            {
                AddPage(Pages.Count + 1);
            }
            else
            {
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void AddPage(int pageNum)
        {
            CommentList Page = new CommentList(_story, pageNum);

            Page.PropertyChanged += PageLoaded;
            Pages.Add(Page);
            ((FrameworkElement)Pivot.Items[pageNum - 1]).DataContext = Page;
        }

        private void RefreshClick(object sender, EventArgs e)
        {
            Pages[Pivot.SelectedIndex].PropertyChanged += PageLoaded;
            Pages[Pivot.SelectedIndex].Refresh();
            ProgressBar.Visibility = Visibility.Visible;
        }

        private void PostClick(object sender, EventArgs e)
        {
            CoreServices.Instance.ReplyToContext = null;
            CoreServices.Instance.Navigate(new Uri("/Pages/CommentPost.xaml?Story=" + _story, UriKind.Relative));
        }
    }
}