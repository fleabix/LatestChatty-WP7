using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using LatestChatty.Classes;
using System.Xml.Linq;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections.Generic;

namespace LatestChatty.ViewModels
{
	public class WatchList : NotifyPropertyChangedBase
	{
		public ObservableCollection<Comment> Comments { get; private set; }

		private List<int> subscribedComments = new List<int>();

		public WatchList()
		{
			this.Comments = new ObservableCollection<Comment>();
			LoadWatchList();
		}

		~WatchList()
		{
			SaveWatchList();
		}

		public void Add(Comment c)
		{
			if (!this.IsOnWatchList(c)) { this.subscribedComments.Add(c.id); this.DownloadComment(c.id); }
		}

		public void Remove(Comment c)
		{
			if (this.IsOnWatchList(c))
			{
				this.subscribedComments.Remove(c.id);
				var commentToRemove = this.Comments.First(c1 => c1.id == c.id);
				this.Comments.Remove(commentToRemove);
			}
		}

		public bool IsOnWatchList(Comment c)
		{
			return this.subscribedComments.Any(i => i == c.id);
		}

		public bool AddOrRemove(Comment c)
		{
			if (this.IsOnWatchList(c))
			{
				this.Remove(c);
				return true;
			}
			else
			{
				this.Add(c);
				return false;
			}
		}

		public void SaveWatchList()
		{
			DataContractSerializer ser = new DataContractSerializer(typeof(List<int>));

			using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
			{
				using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("watchlist.txt", FileMode.Create, isf))
				{
					ser.WriteObject(stream, this.subscribedComments);
				}
			}
		}

		public void LoadWatchList()
		{
			try
			{
				DataContractSerializer ser = new DataContractSerializer(typeof(List<int>));

				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("watchlist.txt", FileMode.OpenOrCreate, isf))
					{
						this.subscribedComments = ser.ReadObject(stream) as List<int>;
					}
				}
			}

			catch { }/*(Exception e)
			{
				MessageBox.Show(string.Format("problem loading pinned threads. {0}", e.ToString()));
			}*/
		}

		//This may not be the optimal way to do this, but it works...
		public void RefreshWatchList()
		{
			this.Comments.Clear();
			foreach (var commentId in this.subscribedComments)
			{
				this.DownloadComment(commentId);
			}
		}

		private void DownloadComment(int commentId)
		{
			string request = CoreServices.Instance.ServiceHost + "thread/" + commentId + ".xml";
			CoreServices.Instance.QueueDownload(request, GetCommentsCallback);
		}

		private void GetCommentsCallback(XDocument response)
		{
			try
			{
				XElement x = response.Elements("comments").Elements("comment").First();
				var storyId = int.Parse(response.Element("comments").Attribute("story_id").Value);
				//Don't save the counts when we load these posts.
				var comment = new Comment(x, storyId, false, 0);
				var insertAt = 0;
				//Sort them the same all the time.
				for (insertAt = 0; insertAt < this.Comments.Count; insertAt++)
				{
					//Keep looking
					if (comment.id > this.Comments[insertAt].id)
					{
						continue;
					}
					//Already exists... don't add it twice.  (This could happen if they click refresh fast)
					if (comment.id == this.Comments[insertAt].id)
					{
						return;
					}
					//We belong before this one.
					if (comment.id < this.Comments[insertAt].id)
					{
						break;
					}
				}
				this.Comments.Insert(insertAt, comment);
			}
			catch (Exception)
			{
				MessageBox.Show("Problem refreshing pinned comments.");
			}
		}
	}
}
