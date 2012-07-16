using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using LatestChatty.Classes;
using System.Xml.Linq;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace LatestChatty.ViewModels
{
    public class SearchResultList : INotifyPropertyChanged
    {
        string _search;
        public ObservableCollection<SearchResult> SearchResults { get; set; }

        public SearchResultList(string search)
        {
            _search = search;
            SearchResults = new ObservableCollection<SearchResult>();
            Refresh();
        }

        void GetCommentsCallback(XDocument response)
        {
            try
            {
                var ObjChatty = from x in response.Descendants("result")
                                select new SearchResult(x);

                SearchResults.Clear();
                foreach (SearchResult singleResult in ObjChatty)
                {
                    SearchResults.Add(singleResult);
                }

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SearchResults"));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot load search results.");
            }
        }

        public void Refresh()
        {
            string request = CoreServices.Instance.ServiceHost + "Search/?" + _search;
						CoreServices.Instance.QueueDownload(request, GetCommentsCallback);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
