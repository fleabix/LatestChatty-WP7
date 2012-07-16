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
		public delegate void GETDelegate(IAsyncResult result);
		GETDelegate _delegate;
		HttpWebRequest _request;
		protected bool cancelled;

		public string Uri { get; private set; }


		public GETDownloader(string getURI, GETDelegate callback)
		{
			this.Uri = getURI;
			_delegate = callback;
		}

		public void Start()
		{
			Thread t = new Thread(this.WorkerThread);
			t.Start();
		}

		private void WorkerThread()
		{
			_request = (HttpWebRequest)HttpWebRequest.Create(this.Uri);
			_request.Method = "GET";
			_request.Headers[HttpRequestHeader.CacheControl] = "no-cache";
			_request.Credentials = CoreServices.Instance.Credentials;

			try
			{
				IAsyncResult token = _request.BeginGetResponse(new AsyncCallback(ResponseCallback), _request);
			}
			catch (WebException wex)
			{
				//TODO: Catch cancellation exception and throw everything else.
				System.Diagnostics.Debugger.Break();
			}
		}

		public void Cancel()
		{
			_request.Abort();
			System.Diagnostics.Debug.WriteLine("Cancelling download for {0}", _request.RequestUri);
			this.cancelled = true;
		}

		public void ResponseCallback(IAsyncResult result)
		{
			if (!this.cancelled)
			{
				InvokeDelegate(result);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("Skipping callback because request was cancelled.");
			}
		}

		virtual protected void InvokeDelegate(IAsyncResult result)
		{
			_delegate(result);
		}
	}
}
