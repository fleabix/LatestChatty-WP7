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
using System.IO.IsolatedStorage;
using System.IO;

namespace LatestChatty.ViewModels
{
    public class MyRepliesList : INotifyPropertyChanged
    {
        public ObservableCollection<SearchResult> SearchResults { get; set; }

        public MyRepliesList()
        {
            LoadMyReplies();
        }

        ~MyRepliesList()
        {
            SaveMyReplies();
        }

        void GetCommentsCallback(XDocument response)
        {
            try
            {
                bool updatedSetting = false;
                var ObjChatty = from x in response.Descendants("result")
                                select new SearchResult(x);

                SearchResults.Clear();
                foreach (SearchResult singleResult in ObjChatty)
                {
                    if (!updatedSetting)
                    {
                        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                        if (settings.Contains("lastReplySeen"))
                        {
                            settings["lastReplySeen"] = singleResult.id;
                        }
                        else
                        {
                            settings.Add("lastReplySeen", singleResult.id);
                        }
                        updatedSetting = true;
                    }

                    SearchResults.Add(singleResult);
                }

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SearchResults"));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot load comments posted by author " + CoreServices.Instance.Credentials.UserName + ".");
            }
        }

        public void Refresh()
        {
            if (CoreServices.Instance.LoginVerified)
            {
                string request = CoreServices.Instance.ServiceHost + "Search/?ParentAuthor=" + CoreServices.Instance.Credentials.UserName;
								CoreServices.Instance.QueueDownload(request, GetCommentsCallback);
            }
        }

        public void SaveMyReplies()
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(ObservableCollection<SearchResult>));

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("myreplies.txt", FileMode.Create, isf))
                {
                    ser.WriteObject(stream, SearchResults);
                }
            }
        }

        public void LoadMyReplies()
        {
            try
            {
                SearchResults = new ObservableCollection<SearchResult>();
                DataContractSerializer ser = new DataContractSerializer(typeof(ObservableCollection<SearchResult>));

                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("myreplies.txt", FileMode.OpenOrCreate, isf))
                    {
                        SearchResults = ser.ReadObject(stream) as ObservableCollection<SearchResult>;
                    }
                }

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SearchResults"));
                }
            }
            catch
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
