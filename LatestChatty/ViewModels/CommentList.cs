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
using System.Collections.Generic;
using Microsoft.Expression.Interactivity.Core;

namespace LatestChatty.ViewModels
{
	[DataContract]
	public class CommentList : NotifyPropertyChangedBase
	{
		[DataMember]
		public int _story = 0;
		[DataMember]
		public int _page = 1;
		[DataMember]
		public ObservableCollection<Comment> Comments { get; set; }
		
		public ICommand LoadMoreComments { get; set; }

		public bool isLoading;
		public bool IsLoading
		{
			get { return this.isLoading; }
			set { this.SetProperty("IsLoading", ref this.isLoading, value); }
		}

		public CommentList(int story, int startPage)
		{
			_story = story;
			_page = startPage;
			this.LoadMoreComments = new ActionCommand(() => this.LoadMore());
			this.Comments = new ObservableCollection<Comment>();
			Refresh();
		}

		public void LoadMore()
		{
			if (this.IsLoading) return;
			//System.Diagnostics.Debug.WriteLine("Load more comments.");
			_page++;
			this.GetComments();
		}

		void GetCommentsCallback(XDocument response)
		{
			try
			{
				_story = (int)response.Root.Attribute("story_id");

				var ObjChatty = from x in response.Descendants("comment")
												select new Comment(x, _story, true, 0);

				foreach (Comment singleComment in ObjChatty)
				{
					if (!this.Comments.Any(c => c.id == singleComment.id))
					{
						//Avoid adding duplicates
						this.Comments.Add(singleComment);
					}
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Cannot load comments of story " + _story + ", page " + _page + ".");
			}
			finally
			{
				this.IsLoading = false;
				CoreServices.Instance.SaveReplyCounts(); //Force saving reply count
			}
		}

		public void Refresh()
		{
			if (this.IsLoading) return;
			this.Comments.Clear();
			this._page = 1;
			this.GetComments();
		}

		private void GetComments()
		{
			string request;
			this.IsLoading = true;

			if (_story == 0)
			{
				request = CoreServices.Instance.ServiceHost + "index.xml";
			}
			else
			{
				if (_page == 0)
				{
					request = CoreServices.Instance.ServiceHost + _story + ".xml";
				}
				else
				{
					request = CoreServices.Instance.ServiceHost + _story + "." + _page + ".xml";
				}
			}
			CoreServices.Instance.QueueDownload(request, GetCommentsCallback);
		}
	}
}
