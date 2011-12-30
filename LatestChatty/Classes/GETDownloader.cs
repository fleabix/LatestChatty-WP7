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

namespace LatestChatty.Classes
{
    public class GETDownloader
    {
        private string _uri;
        public delegate void GETDelegate(IAsyncResult result);
        GETDelegate _delegate;

        public GETDownloader(string getURI, GETDelegate callback)
        {
            _uri = getURI;
            _delegate = callback;

            Thread t = new Thread(this.WorkerThread);
            t.Start();
        }

        private void WorkerThread()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_uri);
            request.Method = "GET";
            request.Headers[HttpRequestHeader.CacheControl] = "no-cache";
            request.Credentials = CoreServices.Instance.Credentials;

            IAsyncResult token = request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
        }

        public void ResponseCallback(IAsyncResult result)
        {
            InvokeDelegate(result);
            
        }

        virtual protected void InvokeDelegate(IAsyncResult result)
        {
            _delegate(result);
        }
    }
}
