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

namespace LatestChatty.Controls
{
    public partial class StoryListControl : UserControl
    {
        public StoryListControl()
        {
            InitializeComponent();
        }

        private void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected index is -1 (no selection) do nothing
            if (MainListBox.SelectedIndex == -1)
                return;

            // Navigate to the new page

            CoreServices.Instance.Navigate(new Uri("/Pages/StoryPage.xaml?Story=" + ((Story)MainListBox.SelectedItem).id, UriKind.Relative));

            // Reset selected index to -1 (no selection)
            MainListBox.SelectedIndex = -1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CoreServices.Instance.Navigate(new Uri("/Pages/ChattyPage.xaml?Story=" + ((Story)((HyperlinkButton)sender).DataContext).id, UriKind.Relative));
        }
    }
}
