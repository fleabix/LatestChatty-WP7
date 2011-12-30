using Microsoft.Phone.Scheduler;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;
using System.Net;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Windows;
using System.Text.RegularExpressions;

namespace RepliesAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>

        int _lastReplySeen = 0;
        protected override void OnInvoke(ScheduledTask task)
        {
            string username;

            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("username"))
            {
                username = settings["username"].ToString();
            }
            else
            {
                ShellToast toast = new ShellToast();
                toast.Title = "LatestChatty: ";
                toast.Content = "Please login to receive reply notifications";
                toast.NavigationUri = new Uri("/", UriKind.Relative);
                toast.Show();
                NotifyComplete();
                return;
            }

            if (settings.Contains("lastReplySeen"))
            {
                _lastReplySeen = int.Parse(settings["lastReplySeen"].ToString());
            }

            string uri = "http://shackapi.stonedonkey.com/Search/?ParentAuthor=" + username;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "GET";
            request.Headers[HttpRequestHeader.CacheControl] = "no-cache";

            IAsyncResult token = request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
        }

        public void ResponseCallback(IAsyncResult result)
        {
            try
            {
                int count = 0;
                int idFirst = 0;
                int story = 0;
                bool first = true;
                string comment = "";
                string author = "";

                WebResponse response = ((HttpWebRequest)result.AsyncState).EndGetResponse(result);
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseString = reader.ReadToEnd();
                XDocument XMLResponse = XDocument.Parse(responseString);
                var arrayResults = XMLResponse.Descendants("result");

                foreach (var singleResult in arrayResults)
                {
                    int id = (int)singleResult.Attribute("id");
                    if (id == _lastReplySeen)
                    {
                        break;
                    }

                    if (first)
                    {
                        story = (int)singleResult.Attribute("story_id");
                        idFirst = id;
                        comment = Regex.Replace(((string)singleResult.Element("body")).Trim(), " target=\"_blank\"", string.Empty);
                        author = (string)singleResult.Attribute("author");
                        first = false;
                    }
                    count++;
                }

                ShellTile chattyTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("ChattyPage"));
                ShellTile defaultTile = chattyTile != null ? chattyTile : ShellTile.ActiveTiles.FirstOrDefault();

                if (defaultTile != null)
                {
                    StandardTileData NewTileData = new StandardTileData();

                    NewTileData.Count = count;

                    if (count > 0)
                    {
                        NewTileData.BackTitle = author;
                        NewTileData.BackContent = comment;
                    }

                    defaultTile.Update(NewTileData);
                }

                IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                int lastReplyToasted = 0;
                if (settings.Contains("lastReplyToasted"))
                {
                    lastReplyToasted = int.Parse(settings["lastReplyToasted"].ToString());
                }

                if (lastReplyToasted != idFirst)
                {
                    ShellToast toast = new ShellToast();
                    toast.Title = "LatestChatty: ";
                    toast.Content = comment;
                    toast.NavigationUri = new Uri("/Pages/ThreadPage.xaml?Comment=" + idFirst + "&Story=" + story, UriKind.Relative);
                    toast.Show();

                    if (settings.Contains("lastReplyToasted"))
                    {
                        settings["lastReplyToasted"] = idFirst;
                    }
                    else
                    {
                        settings.Add("lastReplyToasted", idFirst);
                    }
                    settings.Save();
                }
            }
            catch
            {
            }
            
            NotifyComplete();
        }
    }
}
