using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SharpTwitter
{
    /// <summary>
    /// Interaktionslogik für ConnectDialog.xaml
    /// </summary>
    public partial class ConnectDialog : Window
    {

        public ConnectDialog(string uri)
        {
            InitializeComponent();
            twitterLoginUrl.NavigateUri = new Uri(uri);
        }

        private void connectClicked(object sender, RoutedEventArgs e)
        {
            // get the entered pin
            // get the token and token secret
            // save token and secret to properties
            // re-connect to twitter
            Close();
        }

        public string GetPinCode()
        {
            return pinTextBox.Text;
        }

        private void twitterLoginUrlClicked(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            // go to the url
            Hyperlink source = sender as Hyperlink;
            if (source != null)
            {
                System.Diagnostics.Process.Start(source.NavigateUri.ToString());
            }
        }

    }
}
