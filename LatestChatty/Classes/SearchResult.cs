﻿using System;
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
    public class SearchResult
    {
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

        public SearchResult(XElement x)
        {
            date = ((string)x.Attribute("date")).Trim();
            if (date == "")
            {
                date = "No Date";
            }

            id = (int)x.Attribute("id");
            author = (string)x.Attribute("author");
            body = StripHTML(((string)x.Element("body")).Trim());
            storyid = (int)x.Attribute("story_id");
        }

        private string StripHTML(string s)
        {
            return Regex.Replace(s, " target=\"_blank\"", string.Empty);
        }
    }
}
