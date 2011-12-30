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
using LatestChatty.ViewModels;

namespace LatestChatty.Controls
{
    public partial class MessageListControl : UserControl
    {
        public MessageListControl()
        {
            InitializeComponent();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lbSender = sender as ListBox;
            // If selected index is -1 (no selection) do nothing
            if (lbSender.SelectedIndex == -1)
                return;

            Message m = lbSender.SelectedItem as Message;
            box b;
            if (((MessageList)DataContext) == CoreServices.Instance.Inbox)
            {
                b = box.inbox;
            }
            else if (((MessageList)DataContext) == CoreServices.Instance.Outbox)
            {
                b = box.outbox;
            }
            else
            {
                b = box.archive;
            }

            if (CoreServices.Instance.SelectedMessageChanged != null)
            {
                CoreServices.Instance.SelectedMessageChanged(m);
            }
            else
            {
                CoreServices.Instance.Navigate(new Uri("/Pages/SingleMessagePage.xaml?ID=" + m.id + "&Box=" + b.ToString(), UriKind.Relative));
            }
            lbSender.SelectedIndex = -1;
        }
    }
}
