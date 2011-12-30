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
using Microsoft.Phone.Controls;
using System.IO;

namespace LatestChatty.Pages
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();

            var res = App.GetResourceStream(new Uri("About.txt", UriKind.Relative));
            AboutText.Text = new StreamReader(res.Stream).ReadToEnd();

            res = App.GetResourceStream(new Uri("Help.txt", UriKind.Relative));
            HelpText.Text = new StreamReader(res.Stream).ReadToEnd();
        }
    }
}