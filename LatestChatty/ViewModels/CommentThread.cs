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
using System.Xml.Linq;
using LatestChatty.Classes;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace LatestChatty.ViewModels
{
	[DataContract]
	public class CommentThread : NotifyPropertyChangedBase
	{
		[DataMember]
		public int _id = 0;
		[DataMember]
		public int _story = 0;

		[DataMember]
		public ObservableCollection<Comment> RootComment { get; set; }

		//private Comment rootComment;
		//[DataMember]
		//public Comment RootComment {
		//  get { return this.rootComment; }
		//  private set { this.SetProperty("RootComment", ref this.rootComment, value); }
		//}

		public IEnumerable<Comment> ChildComments { get { return RootComment.First().Comments; } }

		private Comment selectedComment;
		public Comment SelectedComment
		{
			get { return this.selectedComment; }
			set { this.SetProperty("SelectedComment", ref this.selectedComment, value); }
		}

		public bool isWatched;
		[DataMember]
		public bool IsWatched
		{
			get { return this.isWatched; }
			set
			{
				this.SetProperty("IsWatched", ref this.isWatched, value);
				//This whole thing sort of seems like a bad idea. Instead, just use The TogglePinned method
				//if (this.SetProperty("IsWatched", ref this.isWatched, value))
				//{
				//  //CoreServices.Instance.AddOrRemoveWatch(this.RootComment);
				//  CoreServices.Instance.AddOrRemoveWatch(this.RootComment.First());
				//}
			}
		}

		public bool isLoading;
		[DataMember]
		public bool IsLoading
		{
			get { return this.isLoading; }
			set { this.SetProperty("IsLoading", ref this.isLoading, value); }
		}

		public CommentThread(int id, int story)
		{
			this.LoadThread(id, story);
		}

		public void LoadThread(int id, int story)
		{
			_id = id;
			_story = story;
			RootComment = new ObservableCollection<Comment>();
			this.Refresh();
		}

		public void TogglePinned()
		{
			this.IsWatched = !CoreServices.Instance.AddOrRemoveWatch(this.RootComment.First());
		}

		public void SelectComment(Comment c)
		{
			this.SelectedComment = c;
		}

		public void SelectComment(int commentId)
		{
			this.SelectedComment = this.GetComment(commentId);
		}
	
		void GetCommentsCallback(XDocument response)
		{
			try
			{
				XElement x = response.Elements("comments").Elements("comment").First();

				//this.RootComment = new Comment(x, _story);
				//this.IsWatched = CoreServices.Instance.IsOnWatchedList(this.RootComment);
				//this.SelectedComment = this.RootComment;

				this.RootComment.Clear();
				var rootComment = new Comment(x, _story, true);
				this.RootComment.Add(rootComment);
				this.SelectComment(rootComment);
				this.IsWatched = CoreServices.Instance.IsOnWatchedList(rootComment);
				CoreServices.Instance.SaveReplyCounts(); //Force reply counts to be saved when we load a comment thread.
				this.IsLoading = false;
			}
			catch (Exception)
			{
				MessageBox.Show("Cannot load thread of story " + _story + ", comment " + _id + ".");
			}
		}

		public void Refresh()
		{
			string request = CoreServices.Instance.ServiceHost + "thread/" + _id + ".xml";
			this.IsLoading = true;
			CoreServices.Instance.QueueDownload(request, GetCommentsCallback);
		}

		private Comment GetComment(int id)
		{
			var rootComment = this.RootComment.First();
			return rootComment.id == id ? rootComment : rootComment.GetChild(id);
		}

	}
}
