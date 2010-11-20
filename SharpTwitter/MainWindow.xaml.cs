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

            App tweetApp = (App) App.Current;
            TwitterStatusCollection tweets = tweetApp.GetHomeTimeline();
            foreach (TwitterStatus status in tweets)
            {
                AddTweet(status);
            }
        }

        private void AddTweet(TwitterStatus status)
        {
            Grid g = new Grid();
            ColumnDefinition imageColumnDef = new ColumnDefinition();
            imageColumnDef.Width = new GridLength(32);
            g.ColumnDefinitions.Add(imageColumnDef);

            ColumnDefinition tweetColumnDef = new ColumnDefinition();
            tweetColumnDef.Width = new GridLength(200);
            g.ColumnDefinitions.Add(tweetColumnDef);

            g.RowDefinitions.Add(new RowDefinition());
            g.RowDefinitions.Add(new RowDefinition());

            // add the image of the tweeter
            string userImageUrl = status.User.ProfileImageLocation;
            Label userImageLabel = new Label();
            Image userImage = new Image();
            userImage.ToolTip = test;
            BitmapImage bitmap = new BitmapImage(new Uri(userImageUrl));
            userImage.Source = bitmap;
            g.Children.Add(userImage);
            Grid.SetRow(userImage, 0);
            Grid.SetColumn(userImage, 0);
            Grid.SetRowSpan(userImage, 2);

            // add tweeted message
            Label tweetLabel = new Label();
            TextBlock tweetLabelTextBlock = new TextBlock();
            tweetLabelTextBlock.TextWrapping = TextWrapping.Wrap;
            tweetLabelTextBlock.Text = status.Text;
            tweetLabel.Content = tweetLabelTextBlock;
            g.Children.Add(tweetLabel);
            Grid.SetRow(tweetLabel, 1);
            Grid.SetColumn(tweetLabel, 1);

            // add username
            Label userLabel = new Label();
            userLabel.Content = status.User.ScreenName;
            g.Children.Add(userLabel);
            Grid.SetRow(userLabel, 0);
            Grid.SetColumn(userLabel, 1);

            tweetListView.Items.Add(g);
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
            charactersLeftLabel.Content = charsLeft;

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
                App tweetApp = App.Current as App;
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
