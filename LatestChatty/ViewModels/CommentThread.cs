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
using System.Xml.Linq;
using LatestChatty.Classes;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace LatestChatty.ViewModels
{
    [DataContract]
    public class CommentThread
    {
        [DataMember]
        public int _id = 0;
        [DataMember]
        public int _story = 0;
        [DataMember]
        public ObservableCollection<Comment> RootComment { get; set; }

        public CommentThread(int id, int story)
        {
            _id = id;
            _story = story;
            RootComment = new ObservableCollection<Comment>();
            Refresh();
        }

        void GetCommentsCallback(XDocument response)
        {
            try
            {
                XElement x = response.Elements("comments").Elements("comment").First();

                RootComment.Clear();
                RootComment.Add(new Comment(x, _story));

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("RootComment"));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot load thread of story " + _story + ", comment " + _id + ".");
            }
        }

        public void Refresh()
        {
            string request = CoreServices.Instance.ServiceHost + "thread/" + _id + ".xml";

            XMLDownloader download = new XMLDownloader(request, GetCommentsCallback);
        }

        public Comment GetComment(int id)
        {
            Comment c = RootComment.First();
            if (c.id != id)
            {
                return c.GetChild(id);
            }
            return c;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
