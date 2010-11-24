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
using Twitterizer;
using System.Collections;

namespace SharpTwitter
{
    /// <summary>
    /// Interaktionslogik für TwitterWindow.xaml
    /// </summary>
    public partial class TwitterWindow : Window
    {

        private App tweetApp;
        private Brush defaultForeground;
        private ICollection<TwitterStatus> tweetsList;
        private TwitterStatus replyTo = null;

        public TwitterWindow()
        {
            InitializeComponent();
            defaultForeground = charactersLeftLabel.Foreground;           

            tweetApp = (App) App.Current;

            tweetsList = new SortedSet<TwitterStatus>(new TwitterStatusComparer());
            tweetListView.ItemsSource = tweetsList;

            TwitterStatusCollection tweets = tweetApp.GetHomeTimeline();
            SetTweets(tweets);
        }

        private void SetTweets(TwitterStatusCollection tweetsCollection)
        {
            replyTo = null;
            tweetTextBox.Clear();
            tweetsList.Clear();
            foreach (TwitterStatus status in tweetsCollection)
            {
                AddTweet(status);
            }
        }

        private void AddTweet(TwitterStatus status)
        {
            tweetsList.Add(status);
            tweetListView.Items.Refresh();
        }

        void HandleUserLableDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        private void tweetMessageChanged(object sender, TextChangedEventArgs e)
        {
            int length = tweetTextBox.Text.Length;            
            charactersLeftLabel.Visibility = (length > 0) ? Visibility.Visible : Visibility.Hidden;

            int charsLeft = (140 - length);
            charactersLeftLabel.Foreground = (charsLeft < 0) ? Brushes.Red : defaultForeground;
            charactersLeftLabel.Content = charsLeft;
        }

        private void keyPressed(object sender, KeyEventArgs e)
        {
            // check if the user wants to submit the tweet
            if (e.Key == Key.Enter) {
                // set this enter press to handled
                e.Handled = true;

                string tweetMessage = tweetTextBox.Text;
                if (tweetMessage.Length == 0 || tweetMessage.Length > 140)
                {
                    // invalid tweet length
                    return;
                }

                // tweet the entered message
                TwitterStatus status;
                if (replyTo != null)
                    status = tweetApp.ReplyToTweet(replyTo.Id, tweetMessage);
                else
                    status = tweetApp.Tweet(tweetMessage);

                if (status != null)
                {
                    AddTweet(status);
                    tweetTextBox.Clear();
                    replyTo = null;
                }
            }
            else if (e.Key == Key.Escape) {
                e.Handled = true;
                tweetTextBox.Clear();
                replyTo = null;
            }
        }

        private void ProfileImageClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                Image img = sender as Image;
                string username = img.Tag as string;
                Console.WriteLine("show all tweets of user {0}", username);
                TwitterStatusCollection tweets = tweetApp.GetHomeTimeline(username);
                SetTweets(tweets);
            }
        }

        private void TweetMessageClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                DockPanel panel = sender as DockPanel;
                replyTo = panel.Tag as TwitterStatus;

                string tweetText = String.Format("@{0} ", replyTo.User.ScreenName);
                tweetTextBox.Text = tweetText;
            }
        }
    }

    public class TwitterStatusComparer : IComparer<TwitterStatus>
    {

        public int Compare(TwitterStatus s1, TwitterStatus s2)
        {
            return s2.CreatedDate.CompareTo(s1.CreatedDate);
        }

    }
}
