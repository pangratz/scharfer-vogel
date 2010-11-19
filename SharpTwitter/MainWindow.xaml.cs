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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharpTwitter
{
    /// <summary>
    /// Interaktionslogik für TwitterWindow.xaml
    /// </summary>
    public partial class TwitterWindow : Window
    {

        private Brush defaultForeground;

        public TwitterWindow()
        {
            InitializeComponent();
            defaultForeground = charactersLeftLabel.Foreground;
        }

        private void tweetMessageChanged(object sender, TextChangedEventArgs e)
        {
            int length = tweetTextBox.Text.Length;
            int charsLeft = (140 - length);
            if (charsLeft < 0) {
                charactersLeftLabel.Foreground = Brushes.Red;
            } else {
                charactersLeftLabel.Foreground = defaultForeground;
            }
            charactersLeftLabel.Content = (140 - length);

        }

        private void keyPressed(object sender, KeyEventArgs e)
        {
            // check if the user wants to submit the tweet
            if (e.Key == Key.Enter) {
                // set this enter press to handled
                e.Handled = true;

                string tweetMessage = tweetTextBox.Text;
                if (tweetMessage.Length == 0 || tweetMessage.Length > 140) {
                    // invalid tweet length
                    return;
                }

                // tweet the entered message
                App tweetApp = (App) App.Current;
                tweetApp.Tweet(tweetMessage);

                tweetTextBox.Clear();
            }
            else if (e.Key == Key.Escape) {
                e.Handled = true;
                tweetTextBox.Clear();
            }
        }
    }
}
