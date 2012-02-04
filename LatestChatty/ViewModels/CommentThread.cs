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
		public int SeedCommentId = 0;
		[DataMember]
		public int StoryId = 0;

		[DataMember]
		public ObservableCollection<Comment> FlatComments { get; set; }

		private Comment selectedComment;
		[DataMember]
		public Comment SelectedComment
		{
			get { return this.selectedComment; }
			set
			{
				System.Diagnostics.Debug.WriteLine("Setting selected comment.");
				if (this.SetProperty("SelectedComment", ref this.selectedComment, value))
				{
					//If we successfully switched to a new comment, save it as the currently highlighted comment.
					if ((value != null) && (CoreServices.Instance.GetSelectedComment() != value.id))
					{
						CoreServices.Instance.SetCurrentSelectedComment(this.selectedComment);
					}
				}
			}
		}

		public bool isWatched;
		[DataMember]
		public bool IsWatched
		{
			get { return this.isWatched; }
			set
			{
				this.SetProperty("IsWatched", ref this.isWatched, value);
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
			SeedCommentId = id;
			StoryId = story;
			this.FlatComments = new ObservableCollection<Comment>();
			this.Refresh();
		}

		public void TogglePinned()
		{
			this.IsWatched = !CoreServices.Instance.AddOrRemoveWatch(this.FlatComments.First());
		}

		public void SelectComment(Comment c)
		{
			this.SelectedComment = c;
		}

		public void SelectComment(int commentId)
		{
			var c = this.GetCommentById(commentId);
			if (c != null) this.SelectedComment = c;
		}

		void GetCommentsCallback(XDocument response)
		{
			try
			{
				XElement x = response.Elements("comments").Elements("comment").First();
				
				var rootComment = new Comment(x, StoryId, true, 0);
				this.IsWatched = CoreServices.Instance.IsOnWatchedList(rootComment);
				
				this.FlatComments.Clear();
				foreach (var comment in this.GetFlattenedComments(rootComment))
				{
					this.FlatComments.Add(comment);
				}

				this.SelectComment(SeedCommentId);
				CoreServices.Instance.SaveReplyCounts(); //Force reply counts to be saved when we load a comment thread.
				this.IsLoading = false;
			}
			catch (Exception)
			{
				MessageBox.Show("Cannot load thread of story " + StoryId + ", comment " + SeedCommentId + ".");
			}
		}

		public void Refresh()
		{
			string request = CoreServices.Instance.ServiceHost + "thread/" + SeedCommentId + ".xml";
			this.IsLoading = true;
			CoreServices.Instance.QueueDownload(request, GetCommentsCallback);
		}

		private Comment GetCommentById(int id)
		{
			return this.FlatComments.SingleOrDefault(c => c.id == id);
		}
		
		private IEnumerable<Comment> GetFlattenedComments(Comment c)
		{
			yield return c;
			foreach (var comment in c.Comments)
				foreach (var com in GetFlattenedComments(comment))
					yield return com;
		}
	}
}
