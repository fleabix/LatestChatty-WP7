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
using Microsoft.Phone.Controls;

namespace LatestChatty.Classes
{
	public static class WebBrowserBinding
	{
		public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached("Html", typeof(string), typeof(WebBrowserBinding), new PropertyMetadata(HtmlChanged));

		public static string GetHtml(DependencyObject obj)
		{
			return (string)obj.GetValue(HtmlProperty);
		}

		public static void SetHtml(DependencyObject obj, string value)
		{
			obj.SetValue(HtmlProperty, value);
		}

		private static void HtmlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var browser = obj as WebBrowser;
			if (browser == null)
			{
				return;
			}

			if (e.NewValue == null)
			{
				return;
			}

			//We're gonna trick it by starting with opacity of transparent.  When we have something to load, then we'll show the thing.
			browser.InvokeScript("setContent", e.NewValue.ToString());
			if (browser.Opacity != 1) browser.Opacity = 1;
		}
	}
}
