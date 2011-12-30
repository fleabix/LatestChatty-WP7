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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace LatestChatty.Classes
{
    [DataContract]
    public class Comment
    {
        [DataMember]
        public string preview { get; set; }
        [DataMember]
        public int reply_count { get; set; }
        [DataMember]
        public PostCategory category { get; set; }
        [DataMember]
        public string date { get; set; }
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string author { get; set; }
        [DataMember]
        public string body { get; set; }
        [DataMember]
        public int storyid { get; set; }
        [DataMember]
        public ObservableCollection<Comment> Comments { get; set; }

        public Comment(XElement x, int thisstoryid)
        {
            reply_count = (int)x.Attribute("reply_count");
            date = ((string)x.Attribute("date")).Substring(0,19);
            id = (int)x.Attribute("id");
            author = (string)x.Attribute("author");
            body = StripHTML(((string)x.Element("body")).Trim());
            category = (PostCategory)Enum.Parse(typeof(PostCategory), ((string)x.Attribute("category")).Trim(), true);
            preview = ((string)x.Attribute("preview")).Trim();
            storyid = thisstoryid;

            List<XElement> comments = x.Element("comments").Elements("comment").ToList();
            Comments = new ObservableCollection<Comment>();
            if (comments.Count() > 0)
            {
                foreach (XElement xchild in comments)
                {
                    Comment child = new Comment(xchild, thisstoryid);
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
