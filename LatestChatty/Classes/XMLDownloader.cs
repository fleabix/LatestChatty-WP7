using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace LatestChatty.Classes
{
    public class XMLDownloader : GETDownloader
    {
        public delegate void XMLDownloaderCallback(XDocument response);
        private XMLDownloaderCallback _delegate;

        public XMLDownloader(string getURI, XMLDownloaderCallback callback) : base(getURI, null)
        {
            _delegate = callback;
        }

        protected override void InvokeDelegate(IAsyncResult result)
        {
            try
            {
                WebResponse response = ((HttpWebRequest)result.AsyncState).EndGetResponse(result);
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseString = reader.ReadToEnd();
                XDocument XMLResponse = XDocument.Parse(responseString);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _delegate(XMLResponse);
                });
            }
            catch
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _delegate(null);
                });
            }
        }
    }
}
