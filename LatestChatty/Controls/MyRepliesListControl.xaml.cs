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
    public partial class MyRepliesListControl : UserControl
    {
        public MyRepliesListControl()
        {
            InitializeComponent();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lbSender = sender as ListBox;
            // If selected index is -1 (no selection) do nothing
            if (lbSender.SelectedIndex == -1)
                return;

            SearchResult sr = lbSender.SelectedItem as SearchResult;

            CoreServices.Instance.Navigate(new Uri("/Pages/ThreadPage.xaml?Comment=" + sr.id + "&Story=" + sr.storyid, UriKind.Relative));
            lbSender.SelectedIndex = -1;
        }
    }
}
