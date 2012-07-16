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

namespace LatestChatty.ViewModels
{
    public enum box
    {
        inbox,
        outbox,
        archive
    }

    public class MessageList
    {
        public box _box = box.inbox;
        public ObservableCollection<Message> Messages { get; set; }

        public MessageList(box whichbox)
        {
            _box = whichbox;
            Messages = new ObservableCollection<Message>();
            Refresh();
        }

        void GetMessagesCallback(XDocument response)
        {
            try
            {
                var ObjMessages = from x in response.Descendants("message")
                                select new Message(x);

                Messages.Clear();
                foreach (Message singleMessage in ObjMessages)
                {
                    Messages.Add(singleMessage);
                }

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Comments"));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot load messages of " + _box.ToString());
            }
        }

        public void Refresh()
        {
            NetworkCredential nc = CoreServices.Instance.Credentials;
            string request = CoreServices.Instance.ServiceHost + "Messages/?username=" + nc.UserName + "&password=" + nc.Password + "&box=" + _box.ToString() + "&page=1";

						CoreServices.Instance.QueueDownload(request, GetMessagesCallback);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
