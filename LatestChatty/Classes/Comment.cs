using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LatestChatty.Classes
{
	[DataContract]
	public class Comment : NotifyPropertyChangedBase
	{
		[DataMember]
		public string preview { get; set; }
		[DataMember]
		public int reply_count { get; set; }
		[DataMember]
		public PostCategory category { get; set; }
		[DataMember]
		public string dateText { get; set; }
		[DataMember]
		public int id { get; set; }
		[DataMember]
		public string author { get; set; }
		[DataMember]
		public int storyid { get; set; }
		[DataMember]
		public ObservableCollection<Comment> Comments { get; set; }
		[DataMember]
		public string body { get; set; }
		//Set to true if you have a reply below this comment
		[DataMember]
		public bool selfReply { get; set; }
		//Set to true if you are the author of this comment
		[DataMember]
		public bool myPost { get; set; }
		//True if there are new replies since the last time we loaded this comment
		[DataMember]
		public bool HasNewReplies { get; set; }
		//True if this is the first time we've seen this comment
		[DataMember]
		public bool New { get; set; }
		//Contains the number of new posts since we last loaded the comment.
		[DataMember]
		public int NewPostCount { get; set; }
		//If this is set, when we get the reply count we'll update the stored count if it's different.
		[DataMember]
		public bool SavePostCounts { get; set; }

		[DataMember]
		public int Depth { get; set; }

		[DataMember]
		public bool IsSelected { get; set; }

		public Comment(XElement x, int thisstoryid, bool saveCounts, int depth)
		{
			this.SavePostCounts = saveCounts;
			this.reply_count = (int)x.Attribute("reply_count");
			this.dateText = (string)x.Attribute("date");
			this.id = (int)x.Attribute("id");
			this.author = (string)x.Attribute("author");
			this.body = StripHTML(((string)x.Element("body")).Trim());
			this.category = (PostCategory)Enum.Parse(typeof(PostCategory), ((string)x.Attribute("category")).Trim(), true);
			this.preview = ((string)x.Attribute("preview")).Trim();
			this.storyid = thisstoryid;
			var element = (from e in x.Elements()
										 where e.Name == "participants"
										 from p in e.Elements()
										 where p.Name == "participant" && p.Value == CoreServices.Instance.Credentials.UserName
										 select p).FirstOrDefault();
			selfReply = element != null;
			this.myPost = author == CoreServices.Instance.Credentials.UserName;
			this.New = !CoreServices.Instance.PostSeenBefore(this.id);
			this.NewPostCount = CoreServices.Instance.NewReplyCount(this.id, reply_count, this.SavePostCounts);
			this.HasNewReplies = (this.NewPostCount > 0 || this.New);
			this.Depth = depth;

			List<XElement> comments = x.Element("comments").Elements("comment").ToList();
			Comments = new ObservableCollection<Comment>();
			if (comments.Count > 0)
			{
				foreach (XElement xchild in comments)
				{
					Comment child = new Comment(xchild, thisstoryid, this.SavePostCounts, depth + 1);
					Comments.Add(child);
				}
			}
		}

		private string StripHTML(string s)
		{
			return Regex.Replace(s, " target=\"_blank\"", string.Empty);
		}

		public Comment GetChild(int searchid)
		{
			if (id == searchid)
			{
				return this;
			}
			Comment found;
			foreach (Comment c in Comments)
			{
				found = c.GetChild(searchid);
				if (found != null)
				{
					return found;
				}
			}
			return null;
		}
	}
}
