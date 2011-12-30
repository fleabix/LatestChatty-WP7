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
    public class WatchList : INotifyPropertyChanged
    {
        public ObservableCollection<Comment> Comments { get; set; }

        public WatchList()
        {
            LoadWatchList();
        }

        ~WatchList()
        {
            SaveWatchList();
        }

        public void Add(Comment c)
        {
            Comments.Add(c);
        }

        public void Remove(Comment c)
        {
            Comments.Remove(c);
        }

        public bool IsOnWatchList(Comment c)
        {
            foreach (Comment cs in Comments)
            {
                if (cs.id == c.id)
                {
                    return true;
                }
            }
            return false;
        }

        public bool AddOrRemove(Comment c)
        {
            bool remove = false;
            foreach (Comment cs in Comments)
            {
                if (cs.id == c.id)
                {
                    Comments.Remove(cs);
                    remove = true;
                    break;
                }
            }

            if (remove == false)
            {
                Comments.Add(c);
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Comments"));
            }

            return !remove;
        }

        public void SaveWatchList()
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(ObservableCollection<Comment>));

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("watchlist.txt", FileMode.Create, isf))
                {
                    ser.WriteObject(stream, Comments);
                }
            }
        }

        public void LoadWatchList()
        {
            try
            {
                Comments = new ObservableCollection<Comment>();
                DataContractSerializer ser = new DataContractSerializer(typeof(ObservableCollection<Comment>));

                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("watchlist.txt", FileMode.OpenOrCreate, isf))
                    {
                        Comments = ser.ReadObject(stream) as ObservableCollection<Comment>;
                    }
                }

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Comments"));
                }
            }
            catch
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
