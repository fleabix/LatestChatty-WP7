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
using System.Collections.Generic;
using LatestChatty.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Navigation;
using LatestChatty.Classes;
using System.IO;
using System.Runtime.Serialization;
using System.IO.IsolatedStorage;

namespace LatestChatty
{
    public class CoreServices
    {
        public CoreServices()
        {
            LoadWebBrowserString();
            LoadLoginInformation();
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
        private List<CommentList> _storyComment;
        public void AddStoryComments(int story, List<CommentList> comments)
        {
            _storyComment = comments;
        }

        public List<CommentList> GetStoryComments(int story)
        {
            if (_storyComment != null && _storyComment[0] != null && _storyComment[0]._story == story)
            {
                return _storyComment;
            }
            return null;
        }

        public void SaveCurrentStoryComments()
        {
            if (_storyComment != null)
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(List<CommentList>));

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
                DataContractSerializer ser = new DataContractSerializer(typeof(List<CommentList>));

                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("currentstorycomments.txt", FileMode.Open, isf))
                    {
                        _storyComment = ser.ReadObject(stream) as List<CommentList>;
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
        public SelectedCommentChangedEvent SelectedCommentChanged;
        public Rectangle SelectedCommentHighlight;

        private int _selectedComment;
        private CommentThread _commentThread;

        public void AddCommentThread(int comment, CommentThread thread)
        {
            _selectedComment = comment;
            _commentThread = thread;
        }

        public CommentThread GetCommentThread(int comment)
        {
            if (_commentThread != null && _commentThread._id == comment)
            {
                return _commentThread;
            }
            return null;
        }

        public int GetSelectedComment()
        {
            return _selectedComment;
        }

        public void SaveCurrentCommentThread()
        {
            if (_commentThread != null)
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(CommentThread));

                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("currentcommentthread.txt", FileMode.Create, isf))
                    {
                        ser.WriteObject(stream, _commentThread);
                    }
                }

                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("currentselectedcomment.txt", FileMode.Create, isf))
                    {
                        StreamWriter sw = new StreamWriter(stream);
                        sw.WriteLine(_selectedComment);
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
                        _commentThread = ser.ReadObject(stream) as CommentThread;
                    }
                }

                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("currentselectedcomment.txt", FileMode.Open, isf))
                    {
                        StreamReader sr = new StreamReader(stream);
                        _selectedComment = int.Parse(sr.ReadLine());
                        sr.Close();
                    }
                }
            }
            catch
            {
            }
        }

        #endregion

        #region WebBrowserControlHelper
        private string _commentHTMLPrepend;
        private string _commentHTMLAppend;
        private void LoadWebBrowserString()
        {
            var resource = Application.GetResourceStream(new Uri("stylesheet.css", UriKind.Relative));
            StreamReader streamReader = new StreamReader(resource.Stream);
            string css = streamReader.ReadToEnd();
            streamReader.Close();

            _commentHTMLPrepend = "<html><head><meta name='viewport' content='user-scalable=no'/><style type='text/css'>" + css + "</style><body><div class='body'>";
            _commentHTMLAppend = "</div></body></html>";
        }

        public string AddCommentHTML(string s)
        {
            return _commentHTMLPrepend + s + _commentHTMLAppend;
        }
        #endregion

        #region NavigationHelper
        public void Navigate(Uri uri)
        {
            ((Page)((App)App.Current).RootFrame.Content).NavigationService.Navigate(uri);
        }
        public Comment ReplyToContext;
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
        }

        public void Deactivated()
        {
            SaveCurrentStoryComments();
            SaveCurrentCommentThread();
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
            return WatchList.AddOrRemove(c);
        }
        #endregion

        #region PageCount
        public void SavePageCount(int count)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("pageCount"))
            {
                settings["pageCount"] = count;
            }
            else
            {
                settings.Add("pageCount", count);
            }
            settings.Save();
        }

        public int GetPageCount()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("pageCount"))
            {
                return int.Parse(settings["pageCount"].ToString());
            }
            else
            {
                return 3;
            }
        }
        #endregion

        #region MyPosts
        public MyPostsList MyPosts = new MyPostsList();
        public MyRepliesList MyReplies = new MyRepliesList();
        #endregion
    }
}
