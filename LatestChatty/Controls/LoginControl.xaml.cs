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
using Microsoft.Phone.Tasks;

namespace LatestChatty.Controls
{
    public partial class LoginControl : UserControl
    {
        CoreServices.LoginCallback _delegate;
        public LoginControl()
        {
            InitializeComponent();
        }

        public LoginControl(CoreServices.LoginCallback callback) : this()
        {
            _delegate = callback;
        }

        public void LoginVerification(bool verified)
        {
            if (verified)
            {
                ((Panel)Parent).Children.Remove(this);
                if (_delegate != null)
                {
                    _delegate(verified);
                }
            }
            else
            {
							//TODO: Bind Progress bar
                VerificationFailed.Visibility = Visibility.Visible;
								ProgressBar.IsIndeterminate = true;
            }
            ProgressBar.Visibility = Visibility.Collapsed;
						ProgressBar.IsIndeterminate = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VerificationFailed.Visibility = Visibility.Collapsed;
            ProgressBar.Visibility = Visibility.Visible;
						ProgressBar.IsIndeterminate = false;

            CoreServices.Instance.TryLogin(usernameTB.Text, passwordTB.Password, LoginVerification);
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();

            task.Uri = new Uri("http://www.shacknews.com/create_account.x");
            task.Show();
        }
    }
}
