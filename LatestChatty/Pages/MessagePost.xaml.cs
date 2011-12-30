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
using System.Windows.Navigation;
using System.IO;
using System.Text.RegularExpressions;

namespace LatestChatty.Pages
{
    public partial class MessagePost : PhoneApplicationPage
    {
        public MessagePost()
        {
            InitializeComponent();
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string subject, to;
            NavigationContext.QueryString.TryGetValue("Subject", out subject);
            NavigationContext.QueryString.TryGetValue("To", out to);

            if (subject != null)
            {
                Subject.Text = "Re: " + subject;
            }
            if (to != null)
            {
                Recipient.Text = to;
            }
        }

        private void MessageClick(object sender, EventArgs e)
        {
            string uri = CoreServices.Instance.ServiceHost + "Messages/Send/";

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "POST";

            request.Credentials = CoreServices.Instance.Credentials;
            IAsyncResult token = request.BeginGetRequestStream(new AsyncCallback(GetPostWriteCallback), request);
        }

        public void GetPostWriteCallback(IAsyncResult result)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                string s;
                HttpWebRequest request = result.AsyncState as HttpWebRequest;

                Stream requestStream = request.EndGetRequestStream(result);
                StreamWriter streamWriter = new StreamWriter(requestStream);
                s = Regex.Replace(Body.Text, "&", "%26");
                streamWriter.Write("body=" + s);
                s = Regex.Replace(Subject.Text, "&", "%26");
                streamWriter.Write("&subject=" + s);
                s = Regex.Replace(Recipient.Text, "&", "%26");
                streamWriter.Write("&to=" + s);
                streamWriter.Flush();
                streamWriter.Close();

                request.BeginGetResponse(new AsyncCallback(GetPostResultCallback), request);
            });
        }

        private void GetPostResultCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = ar.AsyncState as HttpWebRequest;
                WebResponse response = request.EndGetResponse(ar);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    NavigationService.GoBack();
                });
            }
            catch
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Message Send Failed!");
                });
            }
        }
    }
}