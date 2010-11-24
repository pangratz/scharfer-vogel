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

    public enum CurrentView
    {
        HOME_TIMELINE,
        USER_TIMELINE
    }

    /// <summary>
    /// Interaktionslogik für TwitterWindow.xaml
    /// </summary>
    public partial class TwitterWindow : Window
    {

        private App tweetApp;
        private Brush defaultForeground;
        private ICollection<TwitterStatus> tweetsList;
        private TwitterStatus replyTo = null;
        private CurrentView currentView;
        private string currentViewUsername;

        public TwitterWindow()
        {
            InitializeComponent();
            defaultForeground = charactersLeftLabel.Foreground;           

            tweetApp = (App) App.Current;

            tweetsList = new SortedSet<TwitterStatus>(new TwitterStatusComparer());
            tweetListView.ItemsSource = tweetsList;

            currentView = CurrentView.HOME_TIMELINE;
            SetTitle("Home timeline");
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
            tweetListView.ScrollIntoView(tweetsList.First());
        }

        private void AddTweet(TwitterStatus status)
        {
            tweetsList.Add(status);
            tweetListView.Items.Refresh();
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
                    ReplyToLabel.Content = "";
                }
            }
            else if (e.Key == Key.Escape) {
                e.Handled = true;
                tweetTextBox.Clear();
                replyTo = null;
                ReplyToLabel.Content = "";
            }
        }

        private void ProfileImageClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                Image img = sender as Image;
                string username = img.Tag as string;
                Console.WriteLine("show all tweets of user {0}", username);
                SetTitle(username + "'s timeline");
                currentView = CurrentView.USER_TIMELINE;
                currentViewUsername = username;
                TwitterStatusCollection tweets = tweetApp.GetHomeTimeline(username);
                SetTweets(tweets);
            }
        }

        private void TweetMessageClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                FrameworkElement panel = sender as FrameworkElement;
                replyTo = panel.Tag as TwitterStatus;

                string tweetText = String.Format("@{0} ", replyTo.User.ScreenName);
                tweetTextBox.Text = tweetText;
                string replyToStr = String.Format("In reply to: {0} - {1}", replyTo.User.ScreenName, replyTo.Text);
                ReplyToLabel.Content = replyToStr;
            }
            else if (e.ChangedButton == MouseButton.Right && e.ClickCount == 1)
            {
                // show popup?
            }
        }

        private void TweetDockPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            // set opacity of the star to 1.0
            DockPanel dockPanel = sender as DockPanel;
            
            Image FavouriteStarImage = dockPanel.FindName("FavouriteStarImage") as Image;
            FavouriteStarImage.Opacity = 1.0;

            Image RetweetImage = dockPanel.FindName("RetweetImage") as Image;
            RetweetImage.Opacity = 1.0;
        }

        private void TweetDockPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            DockPanel dockPanel = sender as DockPanel;
            
            // update favorite star
            Image FavouriteStarImage = dockPanel.FindName("FavouriteStarImage") as Image;
            TwitterStatus status = (TwitterStatus) FavouriteStarImage.Tag;
            bool isFavourite = (bool) status.IsFavorited;
            FavouriteStarImage.Opacity = (isFavourite) ? 1.0 : 0.0;

            // update rewteet star
            Image RetweetImage = dockPanel.FindName("RetweetImage") as Image;
            status = (TwitterStatus) RetweetImage.Tag;
            bool isRetweeted = (bool)status.Retweeted;
            RetweetImage.Opacity = (isRetweeted) ? 1.0 : 0.0;
        }

        private void FavouriteStar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // invert favourite status of this tweet
            Image FavouriteStarImage = sender as Image;
            TwitterStatus status = (TwitterStatus) FavouriteStarImage.Tag;

            bool isFavorite = (bool) status.IsFavorited;
            isFavorite = !isFavorite;

            status.IsFavorited = isFavorite;
            FavouriteStarImage.Tag = status;
            tweetApp.UpdateTweetFavouriteStatus(status.Id, isFavorite);

            if (isFavorite)
                FavouriteStarImage.Opacity = 1.0;
        }

        private void RetweetIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            TwitterStatus status = image.Tag as TwitterStatus;
            status.Retweeted = true;
            image.Tag = status;
            image.Opacity = 1.0;

            TwitterStatus retweetedStatus = tweetApp.ReTweet(status.Id);
            AddTweet(retweetedStatus);
        }

        private void Icon_MouseEnter(object sender, MouseEventArgs e)
        {
            Image image = sender as Image;
            image.Cursor = Cursors.Hand;
        }

        private void Icon_MouseLeave(object sender, MouseEventArgs e)
        {
            Image image = sender as Image;
            image.Cursor = Cursors.Arrow;
        }

        private void tweetListView_KeyUp(object sender, KeyEventArgs e)
        {
            // check if the pressed key is the ESC key
            if ((e.Key == Key.Escape) && (currentView == CurrentView.USER_TIMELINE))
            {
                TwitterStatusCollection tweets = tweetApp.GetHomeTimeline();
                SetTweets(tweets);
                currentView = CurrentView.HOME_TIMELINE;
                SetTitle("Home timeline");
            }
        }

        private void SetTitle(string newTitle)
        {
            string title = "SharpTwitter";
            if (newTitle != null && newTitle.Length > 0)
            {
                title += " - " + newTitle;
            }
            this.Title = title;
        }
    }

    public class TwitterStatusComparer : IComparer<TwitterStatus>
    {

        public int Compare(TwitterStatus status1, TwitterStatus status2)
        {
            return status2.CreatedDate.CompareTo(status1.CreatedDate);
        }

    }
}
