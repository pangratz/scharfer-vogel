using System;
using System.Windows;
using System.Xml;
using System.Net;
using Twitterizer;
using System.Configuration;

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

        public void Tweet(string message) {
            twitterComm.Tweet(message);
        }

        public TwitterStatusCollection GetHomeTimeline()
        {
            return twitterComm.GetHomeTimeline();
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

        public void Tweet(string message)
        {
            Console.WriteLine("Sending Tweet: {0}", message);
            TwitterResponse<TwitterStatus> statusResponse = TwitterStatus.Update(tokens, message);
            if (statusResponse.Result == RequestResult.Success)
            {
                Console.WriteLine("Successfull Tweet");
            }
            else
            {
                Console.WriteLine("error: {0}", statusResponse.ErrorMessage);
            }
        }

        public void DirectMessage(string user, string message)
        {
        }

        public TwitterStatusCollection GetHomeTimeline()
        {
            TwitterResponse<TwitterStatusCollection> timelineResponse = TwitterTimeline.HomeTimeline(tokens);
            if (timelineResponse.Result == RequestResult.Success)
            {
                // show the timeline
                return timelineResponse.ResponseObject as TwitterStatusCollection;
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
    }

}
