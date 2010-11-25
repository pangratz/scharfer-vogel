using System;
using System.Windows;
using System.Xml;
using System.Net;
using Twitterizer;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Data;
using System.Globalization;

namespace SharpTwitter
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {

        private TwitterCommunicator twitterComm;

        public App() : base() {
            twitterComm = new TwitterCommunicator();
            twitterComm.testCredentials();
        }

        public TwitterStatus Tweet(string message) {
            return twitterComm.Tweet(message);
        }

        public TwitterStatus ReTweet(decimal retweetId)
        {
            return twitterComm.ReTweet(retweetId);
        }

        public TwitterStatus ReplyToTweet(decimal replyTweetId, string message)
        {
            return twitterComm.ReplyToTweet(replyTweetId, message);
        }

        public TwitterStatusCollection GetHomeTimeline()
        {
            return GetHomeTimeline(null);
        }

        public TwitterStatusCollection GetHomeTimeline(decimal lastStatusId)
        {
            return GetHomeTimeline(null, lastStatusId);
        }

        public TwitterStatusCollection GetHomeTimeline(string username)
        {
            return GetHomeTimeline(username, Decimal.MinusOne);
        }

        public TwitterStatusCollection GetHomeTimeline(string username, decimal lastStatusId)
        {
            return twitterComm.GetHomeTimeline(username, lastStatusId);
        }

        public void UpdateTweetFavouriteStatus(decimal tweetId, bool isFavorite)
        {
            twitterComm.UpdateTweetFavouriteStatus(tweetId, isFavorite);
        }

    }

    public class TwitterCommunicator
    {
        private OAuthTokens tokens;

        public TwitterCommunicator()
        {
            tokens = new OAuthTokens();
            tokens.AccessToken = TwitterAccountConstants.oAuthToken;
            tokens.AccessTokenSecret = TwitterAccountConstants.oAuthTokenSecret;
            tokens.ConsumerKey = TwitterAccountConstants.oAuthConsumerKey;
            tokens.ConsumerSecret = TwitterAccountConstants.oAuthConsumerSecret;

            OAuthTokenResponse requestToken = OAuthUtility.GetRequestToken(TwitterAccountConstants.oAuthConsumerKey, TwitterAccountConstants.oAuthConsumerSecret, "oob");
            // Direct or instruct the user to the following address:
            Uri authorizationUri = OAuthUtility.BuildAuthorizationUri(requestToken.Token);
            Console.WriteLine("goto url {0}", authorizationUri);
            string pin = "1234567";


            // TODO create login prompt where the user shall enter the PIN
            // TODO store the pin in a file

            // OAuthTokenResponse response = OAuthUtility.GetAccessToken(TwitterAccountConstants.oAuthConsumerKey, TwitterAccountConstants.oAuthConsumerSecret, requestToken.Token, pin);

            // tokens.AccessToken = response.Token;
            // tokens.AccessTokenSecret = response.TokenSecret;
        }

        public TwitterStatus Tweet(string message)
        {
            Console.WriteLine("Sending Tweet: {0}", message);
            TwitterResponse<TwitterStatus> statusResponse = TwitterStatus.Update(tokens, message);
            return HandleTwitterResponse(statusResponse);
        }

        private TwitterStatus HandleTwitterResponse(TwitterResponse<TwitterStatus> statusResponse)
        {
            if (statusResponse.Result == RequestResult.Success)
            {
                Console.WriteLine("Successfull");
                return statusResponse.ResponseObject as TwitterStatus;
            }
            else
            {
                Console.WriteLine("error: {0}", statusResponse.ErrorMessage);
                return null;
            }
        }

        public TwitterStatusCollection GetHomeTimeline(string username, decimal lastStatusId)
        {
            TwitterResponse<TwitterStatusCollection> response;
            if (username != null)
            {
                UserTimelineOptions opts = new UserTimelineOptions();
                if (lastStatusId > 0)
                    opts.SinceStatusId = lastStatusId;
                opts.ScreenName = username;
                response = TwitterTimeline.UserTimeline(tokens, opts);
            } else
            {
                TimelineOptions opts = new TimelineOptions();
                if (lastStatusId > 0)
                    opts.SinceStatusId = lastStatusId;

                response = TwitterTimeline.HomeTimeline(tokens, opts);
            }

            if (response.Result == RequestResult.Success)
            {
                // show the timeline
                return response.ResponseObject as TwitterStatusCollection;
            }
            else
            {
                // excption
            }

            return null;
        }

        internal void testCredentials()
        {
            TwitterResponse<TwitterUser> verifyCredentialsRes = TwitterAccount.VerifyCredentials(tokens);
            if (verifyCredentialsRes.Result != RequestResult.Success)
            {
                throw new ApplicationException(verifyCredentialsRes.ErrorMessage);
            }
        }

        internal TwitterStatus ReTweet(decimal replyTweetId)
        {
            Console.WriteLine("ReTweeting Tweet: {0}", replyTweetId);
            TwitterResponse<TwitterStatus> statusResponse = TwitterStatus.Retweet(tokens, replyTweetId);
            return HandleTwitterResponse(statusResponse);
        }

        internal void UpdateTweetFavouriteStatus(decimal tweetId, bool isFavorite)
        {
            if (isFavorite)
                TwitterFavorite.Create(tokens, tweetId);
            else
                TwitterFavorite.Delete(tokens, tweetId);
        }

        internal TwitterStatus ReplyToTweet(decimal replyTweetId, string message)
        {
            Console.WriteLine("Replying to Tweet {0} with message {1}", replyTweetId, message);
            StatusUpdateOptions opts = new StatusUpdateOptions();
            opts.InReplyToStatusId = replyTweetId;
            TwitterResponse<TwitterStatus> response = TwitterStatus.Update(tokens, message, opts);
            return HandleTwitterResponse(response);
        }
    }

    class TweetDateCreatedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Console.WriteLine("Converting {0} to type {1}", value, targetType.GetType());

            DateTime dateTime = (DateTime)value;
            TimeSpan t = (DateTime.UtcNow - dateTime.ToUniversalTime());

            if (t.TotalSeconds < 60)
                return string.Format("{0:0} seconds ago", t.TotalSeconds);
            else if (t.TotalMinutes < 60)
                return string.Format("{0:0} minutes ago", t.TotalMinutes);
            else if (t.TotalHours < 24)
                return string.Format("{0:0} hours ago", t.TotalHours);
            else if (t.TotalDays < 30)
                return string.Format("{0:0} days ago", t.TotalDays);

            return dateTime.ToString("d.M.yyyy, H:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Console.WriteLine("Converting back {0} to type {1}", value, targetType.GetType());
            return value;
        }
    }

}
