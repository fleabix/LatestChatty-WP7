using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using LatestChatty.ViewModels;
using LatestChatty.Classes;

namespace LatestChatty.Pages
{
	public partial class HeadlinesPage : PhoneApplicationPage
	{
		public HeadlinesPage()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			bool create = false;
			StoryList headlines = CoreServices.Instance.GetHeadlines(ref create);
			Headlines.DataContext = headlines;
		}

		private void Refresh_Click(object sender, EventArgs e)
		{
			StoryList headlines = Headlines.DataContext as StoryList;
			headlines.Refresh();
		}
	}
}