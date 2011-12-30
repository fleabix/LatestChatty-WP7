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
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace LatestChatty.Classes
{
    public class Story
    {
        public string preview { get; set; }
        public int comment_count { get; set; }
        public string name { get; set; }
        public string date { get; set; }
        public int id { get; set; }
        public string body { get; set; }

        public Story(XElement x)
        {
            comment_count = (int)x.Element("comment-count");
            if (x.Element("date") != null)
            {
                date = ((string)x.Element("date")).Substring(0, 19).Replace("T", " ");
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
