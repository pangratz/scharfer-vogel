using System;
using System.Windows;
using System.Xml;
using System.Net;
using Twitterizer;
using System.Configuration;
using System.Diagnostics;

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
            return twitterComm.GetHomeTimeline(null);
        }

        internal TwitterStatusCollection GetHomeTimeline(string username)
        {
            return twitterComm.GetHomeTimeline(username);
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

            TwitterResponse<TwitterUser> resp = TwitterUser.Show(tokens, "pangratz");
            TwitterUser twitterUser = resp.ResponseObject as TwitterUser;
            string desc = twitterUser.Description;
            Console.WriteLine("description: {0}", desc);
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

        public void DirectMessage(string user, string message)
        {
        }

        public TwitterStatusCollection GetHomeTimeline(string username)
        {
            TwitterResponse<TwitterStatusCollection> response;
            if (username != null)
            {
                UserTimelineOptions opts = new UserTimelineOptions();
                opts.ScreenName = username;
                response = TwitterTimeline.UserTimeline(tokens, opts);
            } else
            {
                response = TwitterTimeline.HomeTimeline(tokens);
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

        internal TwitterStatus ReplyToTweet(decimal replyTweetId, string message)
        {
            Console.WriteLine("Replying to Tweet {0} with message {1}", replyTweetId, message);
            StatusUpdateOptions opts = new StatusUpdateOptions();
            opts.InReplyToStatusId = replyTweetId;
            TwitterResponse<TwitterStatus> response = TwitterStatus.Update(tokens, message, opts);
            return HandleTwitterResponse(response);
        }
    }

}
