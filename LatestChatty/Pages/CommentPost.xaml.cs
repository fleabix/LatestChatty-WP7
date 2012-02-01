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
using LatestChatty.Classes;
using Microsoft.Phone.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Text;

namespace LatestChatty.Pages
{
	public partial class CommentPost : PhoneApplicationPage
	{
		int _story;
		Comment _reply;
		Stream _imageStream;

		public CommentPost()
		{
			InitializeComponent();
			CommentViewer.NavigateToString(CoreServices.Instance.CommentBrowserString);
			Loaded += new RoutedEventHandler(CommentPost_Loaded);
		}

		void CommentPost_Loaded(object sender, RoutedEventArgs e)
		{
			if (_reply != null)
			{
				CommentViewer.InvokeScript("setContent", _reply.body);
				if (CommentViewer.Opacity != 1) CommentViewer.Opacity = 1;
			}
			CommentViewer.Navigating += new EventHandler<NavigatingEventArgs>(CommentViewer_Navigating);
		}

		// When page is navigated to set data context to selected item in list
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			string sStory = "";
			if (NavigationContext.QueryString.TryGetValue("Story", out sStory))
			{
				_story = int.Parse(sStory);
			}
			else
			{
				NavigationService.GoBack();
			}

			if (CoreServices.Instance.ReplyToContext != null)
			{
				_reply = CoreServices.Instance.ReplyToContext;
				DataContext = _reply;
			}
			else
			{
				CommentReplyBox.Visibility = Visibility.Collapsed;
			}

			if (CoreServices.Instance.LoginVerified == false)
			{
				Login.Visibility = Visibility.Visible;
			}
		}

		protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
		{
			CoreServices.Instance.ReplyToContext = null;
		}

		private void PostClick(object sender, EventArgs e)
		{
			if (Post.Text.Length <= 5)
			{
				MessageBox.Show("Post something longer.");
				return;
			}
			if (Post.Text.Length > 5000)
			{
				MessageBox.Show("There is a limit of 5000 characters in a single post.");
				return;
			}
			string request = CoreServices.Instance.PostServiceHost + _story + ".json";
			string content = "content_type_id=" + _story + "&content_id=" + _story;

			if (_reply != null)
			{
				content += "&parent_id=" + _reply.id;
			}

			//Will url encoding help new lines? If not, it'll at least help a lot of other stuff that was probably broken...
			//Nope.  Ok, so maybe replacing newline with %0A
			//... newlines in a text box appear to have just \r ... even when Environment.NewLine is \r\n??
			var encodedBody = HttpUtility.UrlEncode(Post.Text.Replace("\r", "\r\n"));
			content += "&body=" + encodedBody;

			System.Diagnostics.Debug.WriteLine("Posting: {0}", encodedBody);
			ProgressBar.Visibility = Visibility.Visible;
			ProgressBar.IsIndeterminate = true;
			Post.IsEnabled = false;

			((Microsoft.Phone.Shell.ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;

			POSTHandler download = new POSTHandler(request, content, PostCallback);
		}

		void PostCallback(bool success)
		{
			ProgressBar.Visibility = Visibility.Collapsed;
			ProgressBar.IsIndeterminate = false;
			Post.IsEnabled = true;

			((Microsoft.Phone.Shell.ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;


			if (success)
			{
				System.Diagnostics.Debug.WriteLine("Post Successful!");
				//Reset the current thread to nothing.  This will force a reload so we see our comment.
				//We'll leave the currently highlighted thread, that way we'll return right to where we responded.
				CoreServices.Instance.SetCurrentCommentThread(null);
				CoreServices.Instance.ReplyToContext = null;
				NavigationService.GoBack();
			}
		}

		//TODO: What's this for?
		void CommentViewer_Navigating(object sender, NavigatingEventArgs e)
		{
			string s = e.Uri.ToString();

			if (s.Contains("shacknews.com/chatty?id="))
			{
				int c = int.Parse(s.Split('=')[1].Split('#')[0]);
				CoreServices.Instance.Navigate(new Uri("/Pages/ThreadPage.xaml?Comment=" + c, UriKind.Relative));
				e.Cancel = true;
				return;
			}

			WebBrowserTask task = new WebBrowserTask();
			task.Uri = new Uri(s);
			task.Show();
			e.Cancel = true;
		}

		private void AttachClick(object sender, EventArgs e)
		{
			PhotoChooserTask selectphoto = null;
			selectphoto = new PhotoChooserTask();
			selectphoto.Completed += new EventHandler<PhotoResult>(PhotoChooser_Completed);
			selectphoto.Show();
		}

		string boundary = "----------" + DateTime.Now.Ticks.ToString();

		void PhotoChooser_Completed(object sender, PhotoResult e)
		{
			if (e.TaskResult == TaskResult.OK)
			{
				_imageStream = e.ChosenPhoto;
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri("http://chattypics.com/upload.php"));
				request.Method = "POST";
				request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);

				IAsyncResult token = request.BeginGetRequestStream(new AsyncCallback(BeginPostCallback), request);
				ProgressBar.Visibility = Visibility.Visible;
				ProgressBar.IsIndeterminate = true;
			}
		}


		public void BeginPostCallback(IAsyncResult result)
		{
			HttpWebRequest request = result.AsyncState as HttpWebRequest;
			Stream requestStream = request.EndGetRequestStream(result);
			StreamWriter writer = new StreamWriter(requestStream);

			byte[] ba = new byte[_imageStream.Length];
			_imageStream.Read(ba, 0, (int)_imageStream.Length);

			writer.Write("--");
			writer.WriteLine(boundary);
			writer.WriteLine(@"Content-Disposition: form-data; name=""userfile[]""; filename=""WP7Upload.jpg""");
			writer.WriteLine(@"Content-Type: image/jpeg");
			writer.WriteLine(@"Content-Length: " + ba.Length);
			writer.WriteLine();
			writer.Flush();
			Stream output = writer.BaseStream;

			output.Write(ba, 0, ba.Length);
			output.Flush();
			writer.WriteLine();

			writer.Write("--");
			writer.Write(boundary);
			writer.WriteLine("--");
			writer.Flush();
			writer.Close();

			request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
		}

		public void ResponseCallback(IAsyncResult result)
		{
			try
			{
				HttpWebRequest request = result.AsyncState as HttpWebRequest;
				WebResponse response = request.EndGetResponse(result);
				StreamReader readStream = new StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8"));

				string s = readStream.ReadToEnd();
				string fileLoc = Regex.Match(s, "http://chattypics\\.com/files/WP7Upload_[^_]+\\.jpg").Groups[0].ToString();
				Deployment.Current.Dispatcher.BeginInvoke(() =>
				{
					Post.Text += "\n" + fileLoc;
					ProgressBar.Visibility = Visibility.Collapsed;
					ProgressBar.IsIndeterminate = false;
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.InnerException);
			}
		}
	}
}