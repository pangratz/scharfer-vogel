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

            // read the account tokens & secret from properties
            string token = SharpTwitter.Properties.Settings.Default.token;
            string tokenSecret = SharpTwitter.Properties.Settings.Default.tokenSecret;

            twitterComm = new TwitterCommunicator(token, tokenSecret);
        }

        public bool IsConnected()
        {
            return twitterComm.TestCredentials();
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


        public string GetTwitterLoginUrl()
        {
            return twitterComm.GetTwitterLoginUrl();
        }

        public void StartNewAuthorization()
        {
            twitterComm.StartNewAuthorization();
        }

        public void FinishAuthorization(string pin)
        {
            string accessToken, accessTokenSecret;
            twitterComm.FinishAuthorization(pin, out accessToken, out accessTokenSecret);

            // save default values
            SharpTwitter.Properties.Settings.Default.token = accessToken;
            SharpTwitter.Properties.Settings.Default.tokenSecret = accessTokenSecret;
            SharpTwitter.Properties.Settings.Default.Save();
        }

        internal void CancelAuthorization()
        {
            twitterComm.CancelAuthorization();
        }
    }

    public class TwitterCommunicator
    {

        private OAuthTokenResponse currentRequestToken;
        private OAuthTokens tokens;

        public TwitterCommunicator(string token, string tokenSecret)
        {
            tokens = new OAuthTokens();
            tokens.ConsumerKey = TwitterAccountConstants.oAuthConsumerKey;
            tokens.ConsumerSecret = TwitterAccountConstants.oAuthConsumerSecret;

            if (token != null && tokenSecret != null)
            {
                tokens.AccessToken = token;
                tokens.AccessTokenSecret = tokenSecret;
            }
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
                // exception
            }

            return null;
        }

        internal bool TestCredentials()
        {
            try
            {
                TwitterResponse<TwitterUser> verifyCredentialsRes = TwitterAccount.VerifyCredentials(tokens);
                if (verifyCredentialsRes.Result == RequestResult.Success)
                {
                    return true;
                }
            } catch (Exception) {
            }

            return false;
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

        internal string GetTwitterLoginUrl()
        {
            if (currentRequestToken != null)
            {
                Uri authorizationUri = OAuthUtility.BuildAuthorizationUri(currentRequestToken.Token);
                return authorizationUri.ToString();
            }

            throw new InvalidOperationException("Call StartNewAuthorization first!");
        }

        internal void StartNewAuthorization()
        {
            currentRequestToken = OAuthUtility.GetRequestToken(TwitterAccountConstants.oAuthConsumerKey, TwitterAccountConstants.oAuthConsumerSecret, "oob");
        }

        internal void FinishAuthorization(string pin, out string AccessToken, out string AccessTokenSecret)
        {
            OAuthTokenResponse response = OAuthUtility.GetAccessToken(TwitterAccountConstants.oAuthConsumerKey, TwitterAccountConstants.oAuthConsumerSecret, currentRequestToken.Token, pin);
            
            AccessToken = response.Token;
            AccessTokenSecret = response.TokenSecret;

            tokens.AccessToken = AccessToken;
            tokens.AccessTokenSecret = AccessTokenSecret;

            currentRequestToken = null;
        }

        internal void CancelAuthorization()
        {
            currentRequestToken = null;
        }
    }

}
