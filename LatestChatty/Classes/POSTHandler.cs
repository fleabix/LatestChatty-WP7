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
using System.Threading;
using System.IO;

namespace LatestChatty.Classes
{
    public class POSTHandler
    {
        private string _content;
        private string _uri;
        public delegate void POSTDelegate(bool success);
        POSTDelegate _delegate;

        public POSTHandler(string postURI, string content, POSTDelegate callback)
        {
            _uri = postURI;
            _content = content;
            _delegate = callback;

            Thread t = new Thread(this.WorkerThread);
            t.Start();
        }

        private void WorkerThread()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Credentials = CoreServices.Instance.Credentials;

            IAsyncResult token = request.BeginGetRequestStream(new AsyncCallback(BeginPostCallback), request);
        }

        public void BeginPostCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;

            Stream requestStream = request.EndGetRequestStream(result);
            StreamWriter streamWriter = new StreamWriter(requestStream);
            streamWriter.Write(_content);
            streamWriter.Flush();
            streamWriter.Close();

            request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
        }

        public void ResponseCallback(IAsyncResult result)
        {
            try
            {
                HttpWebRequest request = result.AsyncState as HttpWebRequest;
                WebResponse response = request.EndGetResponse(result);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _delegate(true);
                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Posting failed because: {0}", ex);
                    MessageBox.Show("Posting Failed!");
                    _delegate(false);
                });


            }
        }
    }
}
