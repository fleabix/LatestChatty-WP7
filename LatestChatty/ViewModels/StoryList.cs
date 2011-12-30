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
using System.ComponentModel;
using LatestChatty.Classes;
using System.Xml.Linq;

namespace LatestChatty.ViewModels
{
    public class StoryList : INotifyPropertyChanged
    {
        public ObservableCollection<Story> Stories { get; set; }

        public StoryList()
        {
            Stories = new ObservableCollection<Story>();
            Refresh();
        }

        void GetStoriesCallback(XDocument response)
        {
            try
            {
                var ObjStory = from x in response.Descendants("story")
                               select new Story(x);

                Stories.Clear();

                foreach (Story story in ObjStory)
                {
                    Stories.Add(story);
                }

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Stories"));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot load stories");
            }
        }

        public void Refresh()
        {
            string request;

            request = CoreServices.Instance.ServiceHost + "stories.xml";

            XMLDownloader download = new XMLDownloader(request, GetStoriesCallback);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
