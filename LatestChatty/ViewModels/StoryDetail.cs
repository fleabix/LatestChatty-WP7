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
using LatestChatty.Classes;
using System.ComponentModel;
using System.Xml.Linq;

namespace LatestChatty.ViewModels
{
    public class StoryDetail : INotifyPropertyChanged
    {
        private int _story;
        public Story Detail { get; set; }

        public StoryDetail(int id)
        {
            _story = id;
            Refresh();
        }

        void GetStoryDetailCallback(XDocument response)
        {
            try
            {
                XElement x = response.Elements("story").First();

                Detail = new Story(x);

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Stories"));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot load story " + _story + ".");
            }
        }

        public void Refresh()
        {
            string request;

            request = CoreServices.Instance.ServiceHost + "/stories/" + _story + ".xml";

						CoreServices.Instance.QueueDownload(request, GetStoryDetailCallback);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
