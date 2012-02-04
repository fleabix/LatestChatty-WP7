using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using LatestChatty.Classes;
using LatestChatty.ViewModels;

namespace LatestChatty
{
	public class CoreServices
	{
		public CoreServices()
		{
			SetCommentBrowserString();
			LoadLoginInformation();
			LoadReplyCounts();
			LoadSettings();
		}

		~CoreServices()
		{
		}

		#region Singleton
		private static CoreServices _coreServices = null;
		public static CoreServices Instance
		{
			get
			{
				if (_coreServices == null)
				{
					_coreServices = new CoreServices();
				}
				return _coreServices;
			}
		}
		#endregion

		#region Settings
		public readonly IsolatedStorageSettings Settings = IsolatedStorageSettings.ApplicationSettings;
		private void LoadSettings()
		{
			if (!this.Settings.Contains(SettingsConstants.CommentSize))
			{
				this.Settings.Add(SettingsConstants.CommentSize, CommentViewSize.Small);
			}
			if (!this.Settings.Contains(SettingsConstants.ThreadNavigationByDate))
			{
				this.Settings.Add(SettingsConstants.ThreadNavigationByDate, true);
			}
		}
		#endregion

		#region ServiceHost
		public string ServiceHost
		{
			get
			{
				return "http://shackapi.stonedonkey.com/";
			}
		}

		public string PostServiceHost
		{
			get
			{
				return " http://www.shacknews.com/api/chat/create/";
			}
		}
		#endregion

		#region StoryCommentCache
		private CommentList _storyComment;
		public void AddStoryComments(int story, CommentList comments)
		{
			_storyComment = comments;
		}

		public CommentList GetStoryComments(int story)
		{
			if (_storyComment != null && _storyComment._story == story)
			{
				return _storyComment;
			}
			return null;
		}

		public void SaveCurrentStoryComments()
		{
			if (_storyComment != null)
			{
				DataContractSerializer ser = new DataContractSerializer(typeof(CommentList));

				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("currentstorycomments.txt", FileMode.Create, isf))
					{
						ser.WriteObject(stream, _storyComment);
					}
				}
			}
		}

		public void LoadCurrentStoryComments()
		{
			try
			{
				DataContractSerializer ser = new DataContractSerializer(typeof(CommentList));

				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("currentstorycomments.txt", FileMode.Open, isf))
					{
						_storyComment = ser.ReadObject(stream) as CommentList;
					}
				}
			}
			catch
			{
			}
		}
		#endregion

		#region ThreadPageHelper
		public delegate void SelectedCommentChangedEvent(Comment newSelection);
		//public SelectedCommentChangedEvent SelectedCommentChanged;
		public Rectangle SelectedCommentHighlight;

		private int _selectedCommentId;
		private CommentThread _currentCommentThread;

		public void SetCurrentCommentThread(CommentThread thread)
		{
			_currentCommentThread = thread;
		}

		public void SetCurrentSelectedComment(Comment c)
		{	
			System.Diagnostics.Debug.WriteLine("Setting global selected comment Id {0}", c.id);
			_selectedCommentId = c.id;

			//if (this.SelectedCommentChanged != null)
			//{
			//          Deployment.Current.Dispatcher.BeginInvoke(() => this.SelectedCommentChanged(c));
			//}
		}
		
		public CommentThread GetCommentThread(int comment)
		{
			if (_currentCommentThread != null && _currentCommentThread.SeedCommentId == comment)
			{
				return _currentCommentThread;
			}
			return null;
		}

		public int GetSelectedComment()
		{
			return _selectedCommentId;
		}

		public void SaveCurrentCommentThread()
		{
			if (_currentCommentThread != null)
			{
				DataContractSerializer ser = new DataContractSerializer(typeof(CommentThread));

				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("currentcommentthread.txt", FileMode.Create, isf))
					{
						ser.WriteObject(stream, _currentCommentThread);
					}
				}

				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("currentselectedcomment.txt", FileMode.Create, isf))
					{
						StreamWriter sw = new StreamWriter(stream);
						sw.WriteLine(_selectedCommentId);
						sw.Close();
					}
				}
			}
		}

		public void LoadCurrentCommentThread()
		{
			try
			{
				DataContractSerializer ser = new DataContractSerializer(typeof(CommentThread));

				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("currentcommentthread.txt", FileMode.Open, isf))
					{
						System.Diagnostics.Debug.WriteLine("Loading comment thread from persistent storage.");
						_currentCommentThread = ser.ReadObject(stream) as CommentThread;
					}
				}

				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("currentselectedcomment.txt", FileMode.Open, isf))
					{
						System.Diagnostics.Debug.WriteLine("Loading selected comment id from persistent storage.");
						StreamReader sr = new StreamReader(stream);
						_selectedCommentId = int.Parse(sr.ReadLine());
						sr.Close();
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Exception occurred deserializing current comment thread. {0}", ex);
			}
		}

		#endregion

		#region WebBrowserControlHelper
		public string CommentBrowserString { get; private set; }
		private void SetCommentBrowserString()
		{
			var resource = Application.GetResourceStream(new Uri("stylesheet.css", UriKind.Relative));
			StreamReader streamReader = new StreamReader(resource.Stream);
			string css = streamReader.ReadToEnd();
			streamReader.Close();
			//Without the scrolling in there, very weird rendering happens when you've scrolled down on a large post and then switch to a short post.
			this.CommentBrowserString = "<html><head><meta name='viewport' content='user-scalable=no'/><script type=\"text/javascript\">function setContent(s) { document.body.scrollTop = 0; document.body.scrollLeft = 0; document.getElementById('commentBody').innerHTML = s; } </script><style type='text/css'>" + css + "</style><body><div id='commentBody' class='body'></div></body></html>";
		}
		#endregion

		#region NavigationHelper
		public void Navigate(Uri uri)
		{
			((Page)((App)App.Current).RootFrame.Content).NavigationService.Navigate(uri);
		}
		public Comment ReplyToContext;
		#endregion

		#region API Helper
		private readonly API_Helper apiHelper = new API_Helper();
		public void QueueDownload(string uri, LatestChatty.Classes.XMLDownloader.XMLDownloaderCallback callback)
		{
			this.apiHelper.AddDownload(uri, callback);
		}

		public void CancelDownloads()
		{
			this.apiHelper.CancelDownloads();
		}

		#endregion

		#region LoginHelper
		public delegate void LoginCallback(bool verified);

		NetworkCredential _nc = new NetworkCredential();
		private bool _loginVerified = false;
		public bool LoginVerified
		{
			get
			{
				return _loginVerified;
			}
		}

		public NetworkCredential Credentials
		{
			get
			{
				return _nc;
			}
		}

		private LoginCallback _loginCallback;

		public void TryLogin(string username, string password, LoginCallback callback)
		{
			_nc.UserName = username;
			_nc.Password = password;
			_loginCallback = callback;
			_loginVerified = true;

			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				_loginCallback(true);
				_loginCallback = null;

				CoreServices.Instance.SaveLoginInformation();
			});

			// Removing authentication for now
			/*
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.shackchatty.com/auth");
			request.Method = "POST";

			request.Credentials = _nc;
			IAsyncResult token = request.BeginGetResponse(new AsyncCallback(GetLoginCallback), request);
			 * */
		}

		public void GetLoginCallback(IAsyncResult result)
		{
			try
			{
				WebResponse response = ((HttpWebRequest)result.AsyncState).EndGetResponse(result);
				_loginVerified = true;
			}
			catch (Exception)
			{
				_nc.Password = "";
				_loginVerified = false;
			}

			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				_loginCallback(_loginVerified);
				_loginCallback = null;
			});
		}

		public void SaveLoginInformation()
		{
			IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
			if (settings.Contains("username"))
			{
				settings["username"] = _nc.UserName;
			}
			else
			{
				if (_nc.UserName != "")
				{
					settings.Add("username", _nc.UserName);
				}
			}

			if (settings.Contains("password"))
			{
				settings["password"] = _nc.Password;
			}
			else
			{
				if (_nc.Password != "")
				{
					settings.Add("password", _nc.Password);
				}
			}
			settings.Save();
		}

		public void LoadLoginInformation()
		{
			IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
			if (settings.Contains("username"))
			{
				_nc.UserName = settings["username"].ToString();
			}

			if (settings.Contains("password"))
			{
				_nc.Password = settings["password"].ToString();
				_loginVerified = true;
			}
		}

		public void Logout()
		{
			_nc.UserName = "";
			_nc.Password = "";
			_loginVerified = false;
			SaveLoginInformation();
		}
		#endregion

		#region Tombstone
		public void Activated()
		{
			LoadCurrentStoryComments();
			LoadCurrentCommentThread();
			//Should already be loaded, no?
			//LoadReplyCounts();
		}

		public void Deactivated()
		{
			SaveCurrentStoryComments();
			SaveCurrentCommentThread();
			SaveReplyCounts();
		}
		#endregion

		#region Headlines
		private StoryList _headlines;
		public StoryList GetHeadlines(ref bool create)
		{
			if (_headlines == null)
			{
				_headlines = new StoryList();
				create = true;
			}
			return _headlines;
		}
		#endregion

		#region StoryDetail
		private List<StoryDetail> _storyDetails = new List<StoryDetail>();
		public StoryDetail GetStoryDetail(int id, ref bool create)
		{
			foreach (StoryDetail s in _storyDetails)
			{
				if (s.Detail.id == id)
				{
					create = false;
					return s;
				}
			}

			StoryDetail sd = new StoryDetail(id);
			_storyDetails.Add(sd);
			create = true;
			return sd;
		}
		#endregion

		#region Messages
		public MessageList Inbox { get; set; }
		public MessageList Outbox { get; set; }
		public MessageList Archive { get; set; }

		public delegate void SelectedMessageChangedEvent(Message newSelection);
		public SelectedMessageChangedEvent SelectedMessageChanged;
		#endregion

		#region WatchList
		public WatchList WatchList = new WatchList();

		public bool IsOnWatchedList(Comment c)
		{
			return WatchList.IsOnWatchList(c);
		}

		public bool AddOrRemoveWatch(Comment c)
		{
			var result = WatchList.AddOrRemove(c);
			WatchList.SaveWatchList(); //Force saving right away.
			return result;
		}
		#endregion

		#region Reply Count Cache
		private Dictionary<int, int> knownReplyCounts = new System.Collections.Generic.Dictionary<int, int>();
		private void LoadReplyCounts()
		{
			System.Diagnostics.Debug.WriteLine("Loading reply counts.");
			try
			{
				DataContractSerializer ser = new DataContractSerializer(knownReplyCounts.GetType());

				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("postcounts.txt", FileMode.Open, isf))
					{
						this.knownReplyCounts = ser.ReadObject(stream) as Dictionary<int, int>;
					}
				}
			}
			catch
			{
				//Something failed, make a new dictionary and start over.
				this.knownReplyCounts = new System.Collections.Generic.Dictionary<int, int>();
			}

			System.Diagnostics.Debug.WriteLine("Loaded {0} reply counts.", this.knownReplyCounts.Count);
			//Since we're already doing expensive operations, let's do this here.
			//Prevent the list from getting gigantic and taking forever to search through.  We'll trim down to 2000 by postid.  Keeping the most recent posts.
			//There's probably a faster way to do this...  I'll figure this out at some point, because loading these takes a long time.
			if (this.knownReplyCounts.Count > 2000)
			{
                System.Diagnostics.Debug.WriteLine("Trimming reply counts.");
				var keepCounts = this.knownReplyCounts.OrderByDescending(r => r.Key).Take(2000).ToList();
				this.knownReplyCounts.Clear();
				foreach (var item in keepCounts)
				{
					this.knownReplyCounts.Add(item.Key, item.Value);
				}
			}
		}

		public void SaveReplyCounts()
		{
			System.Diagnostics.Debug.WriteLine("Saving reply counts.");
			DataContractSerializer ser = new DataContractSerializer(knownReplyCounts.GetType());

			using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
			{
				using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("postcounts.txt", FileMode.Create, isf))
				{
					ser.WriteObject(stream, this.knownReplyCounts);
				}
			}
		}

		/// <summary>
		/// Checks if we have previously checked the reply count for a thread ever.
		/// </summary>
		/// <param name="threadId">The thread id.</param>
		/// <returns></returns>
		public bool PostSeenBefore(int threadId)
		{
			return this.knownReplyCounts.ContainsKey(threadId);
		}

		/// <summary>
		/// Checks to see if the comment is in the cache and if the current reply count is greater than the last known.
		/// </summary>
		/// <param name="threadId">The thread id.</param>
		/// <param name="currentReplyCount">The current reply count.</param>
		/// <param name="updateCount">if set to <c>true</c> the count will be updated if it is not the same.</param>
		/// <returns>
		/// The number of new posts, -1 if it's new.
		/// </returns>
		public int NewReplyCount(int threadId, int currentReplyCount, bool updateCount)
		{
			//We COULD save reply counts as we get them, but that's a lot of excessive writing... let's not do that.
			if (this.knownReplyCounts.ContainsKey(threadId))
			{
				var newReplyCount = currentReplyCount - this.knownReplyCounts[threadId];
				if (newReplyCount > 0)
				{
					System.Diagnostics.Debug.WriteLine("{0} new replies for thread id {1}", newReplyCount, threadId);
					if (updateCount)
					{
						System.Diagnostics.Debug.WriteLine("Updating reply count for thread id {0}", threadId);
						this.knownReplyCounts[threadId] = currentReplyCount;
					}
				}
				return newReplyCount;
			}
			if (updateCount)
			{
				//Haven't seen this thread before, add it to the cache.
				System.Diagnostics.Debug.WriteLine("Thread id {0} is unknown, adding to cache with {1} replies", threadId, currentReplyCount);
				this.knownReplyCounts.Add(threadId, currentReplyCount);
			}
			return -1;
		}

		#endregion
		#region MyPosts
		public MyPostsList MyPosts = new MyPostsList();
		public MyRepliesList MyReplies = new MyRepliesList();
		#endregion
	}
}
