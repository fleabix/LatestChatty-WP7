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

namespace LatestChatty.Pages
{
    public partial class HeadlinesPage : PhoneApplicationPage
    {
        public HeadlinesPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            bool create = false;
            StoryList headlines = CoreServices.Instance.GetHeadlines(ref create);
            if (create)
            {
                ProgressBar.Visibility = Visibility.Visible;
                headlines.PropertyChanged += headlines_PropertyChanged;
            }
            Headlines.DataContext = headlines;
        }

        void headlines_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ((StoryList)sender).PropertyChanged -= headlines_PropertyChanged;
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            StoryList headlines = Headlines.DataContext as StoryList;
            headlines.Refresh();
        }
    }
}