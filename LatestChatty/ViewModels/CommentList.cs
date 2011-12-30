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
    [DataContract]
    public class CommentList : INotifyPropertyChanged
    {
        [DataMember]
        public int _story = 0;
        [DataMember]
        public int _page = 1;
        [DataMember]
        public ObservableCollection<Comment> Comments { get; set; }

        public CommentList(int story, int page)
        {
            _story = story;
            _page = page;
            Comments = new ObservableCollection<Comment>();
            Refresh();
        }

        void GetCommentsCallback(XDocument response)
        {
            try
            {
                _story = (int)response.Root.Attribute("story_id");

                var ObjChatty = from x in response.Descendants("comment")
                                select new Comment(x, _story);

                Comments.Clear();
                foreach (Comment singleComment in ObjChatty)
                {
                    Comments.Add(singleComment);
                }

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Comments"));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot load comments of story " + _story + ", page " + _page + ".");
            }
        }

        public void Refresh()
        {
            string request;
            
            if (_story == 0)
            {
                request = CoreServices.Instance.ServiceHost + "index.xml";
            }
            else
            {
                if (_page == 0)
                {
                    request = CoreServices.Instance.ServiceHost + _story + ".xml";
                }
                else
                {
                    request = CoreServices.Instance.ServiceHost + _story + "." + _page + ".xml";
                }
            }

            XMLDownloader download = new XMLDownloader(request, GetCommentsCallback);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
