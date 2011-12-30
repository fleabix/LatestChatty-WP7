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
    public partial class SearchPage : PhoneApplicationPage
    {
        SearchResultList _results;
        public SearchPage()
        {
            InitializeComponent();
        }

        private void Search_Click(object sender, EventArgs e)
        {
            string s = "";
            if (term.Text != "")
            {
                s += "SearchTerm=" + term.Text + "&";
            }

            if (author.Text != "")
            {
                s += "Author=" + author.Text + "&";
            }

            if (parentauthor.Text != "")
            {
                s += "ParentAuthor=" + parentauthor.Text;
            }

            _results = new SearchResultList(s);
            _results.PropertyChanged += ResultsLoaded;
            ProgressBar.Visibility = Visibility.Visible;
        }

        public void ResultsLoaded(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            Results.DataContext = _results;
        }

    }
}