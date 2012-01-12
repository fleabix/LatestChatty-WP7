using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LatestChatty.Classes
{
	public class Story
	{
		public string preview { get; set; }
		public int comment_count { get; set; }
		public string name { get; set; }
		public string dateText { get; set; }
		public int id { get; set; }
		public string body { get; set; }

		public Story(XElement x)
		{
			comment_count = (int)x.Element("comment-count");
			if (x.Element("date") != null)
			{
				this.dateText = (string)x.Element("date");
			}
			name = ((string)x.Element("name"));
			id = (int)x.Element("id");
			body = StripHTML(((string)x.Element("body")).Trim());
			preview = ((string)x.Element("preview")).Trim();
		}

		private string StripHTML(string s)
		{
			return Regex.Replace(s, " target=\"_blank\"", string.Empty);
		}
	}
}
