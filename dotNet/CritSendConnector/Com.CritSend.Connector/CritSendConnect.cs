using System;
using System.Collections.Generic;
using System.Text;
using Com.CritSend.Connector.com.messaging_master.mail;
using System.Security.Cryptography;
using System.Web.Services.Protocols;
using WS = Com.CritSend.Connector.com.messaging_master.mail;
using log4net;

namespace Com.CritSend.Connector
{
    ///<summary>
    /// Connector for CritSend servers.
    ///</summary>
    public class CritSendConnect
    {

        /// <summary>
        /// Web service client to use for transactions.
        /// </summary>
        private readonly WSDL01forMxMaster _client;

        private readonly Random _random = new Random();

        private readonly ILog _logger = LogManager.GetLogger(typeof (CritSendConnect));

        private string _host;

        /// <summary>
        /// If set to <c>true</c>, use the fast web service.
        /// </summary>
        private readonly bool _fast;

        /// <summary>
        /// Path to the WSDL definition of the web service.
        /// </summary>
        private const string Wsdl = "/api_2.php";

        /// <summary>
        /// Namespace of critsend.
        /// </summary>
        private const string Namespace = "http://mxmaster.net/campaign/0.1#doCampaign";

        /// <summary>
        /// Format used for the timestamp during authentications.
        /// ex:2010-12-15T22:29:09.0005781+01:00
        ///    2010-12-15T21:46:28.6386718+01:00
        /// </summary>
        private const string TimeStampFormat = "yyyy-MM-ddTHH:mm:ssZ";

        private readonly ConnectorConfiguration _configuration;
        private Authentication _authentication;
        private string _key;
        private string _user;

        /// <summary>
        /// Initializes a new instance of <see cref="CritSendConnect"/>
        /// </summary>
        public CritSendConnect()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CritSendConnect"/>
        /// </summary>
        /// <param name="user">The Critsent user to use instead of the configuration file.</param>
        /// <param name="key">The password to use instead of the configuration file.</param>
        public CritSendConnect(string user, string key)
            : this(false, user, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CritSendConnect"/>
        /// </summary>
        /// <param name="fast">If set to <c>true</c> use the fast web service.</param>
        public CritSendConnect(bool fast)
            : this(fast, null, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="CritSendConnect"/>
        /// </summary>
        /// <param name="fast">If set to <c>true</c> use the fast web service.</param>
        /// <param name="user">The Critsent user to use instead of the configuration file.</param>
        /// <param name="key">The password to use instead of the configuration file.</param>
        public CritSendConnect(bool fast, string user, string key)
        {
            _fast = fast;
            _configuration = ConnectorConfiguration.Instance;
            _user = user ?? _configuration.User;
            _key = key ?? _configuration.Key;

            // This line avoids the HTTP 417 errors.
            // Found at http://social.msdn.microsoft.com/Forums/en-US/csharpgeneral/thread/5023d48b-476a-47db-b108-1a4eca6a5316
            System.Net.ServicePointManager.Expect100Continue = false;

            WSDL01forMxMaster client = CreateClient(!_fast ? _configuration.Hosts : _configuration.FastHosts) ??
                                       GetFallbackHost(_configuration.Hosts);


            _client = client;
        }

        /// <summary>
        /// This method creates a tag. It is idempotent
        /// </summary>
        /// <param name="tag">String of less than 40 characters and without spaces or non-ASCII characters</param>
        /// <returns><c>true</c> if the tag was created; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="tag"/> is badly formatted.</exception>
        /// <exception cref="CritSendException">In case of soap error, contains the message.</exception>
        public bool CreateTag(string tag)
        {
            var result = false;
            CallWebMethod(delegate(Authentication auth)
            {
                result = _client.createTag(auth, tag);
            }, true);
            return result;
        }

        /// <summary>
        /// This method deletes a tag.
        /// </summary>
        /// <param name="tag">String of less than 40 characters and without spaces or non-ASCII characters</param>
        /// <returns><c>true</c> if the tag was deleted; false otherwise (especially if it does not exist).</returns>
        /// <exception cref="ArgumentException">If the <paramref name="tag"/> is badly formatted.</exception>
        /// <exception cref="CritSendException">In case of soap error, contains the message.</exception>
        public bool DeleteTag(string tag)
        {
            bool result = false;
            CallWebMethod(delegate(Authentication auth)
            {
                result = _client.deleteTag(auth, tag);
            }, true);
            return result;
        }

        /// <summary>
        /// This method checks whether a tag exists or not. It is idempotent
        /// </summary>
        /// <param name="tag">string of less than 40 characters and without spaces or non-ASCII characters</param>
        /// <returns><c>true</c> if the tag exists; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="tag"/> is badly formatted.</exception>
        /// <exception cref="CritSendException">In case of soap error, contains the message.</exception>
        public bool IsTag(string tag)
        {
            bool result = false;
            CallWebMethod(delegate(Authentication auth)
            {
                result = _client.isTag(auth, tag);
            }, true);
            return result;
        }

        /// <summary>
        /// This method sends back the content of the tag (see reporting doc)
        /// </summary>
        /// <param name="tag">string of less than 40 characters and without spaces or non-ASCII characters</param>
        /// <returns>A string containing the file; false otherwise</returns>
        /// <exception cref="ArgumentException">If the <paramref name="tag"/> is badly formatted.</exception>
        /// <exception cref="CritSendException">In case of soap error, contains the message.</exception>
        public string GetTag(string tag)
        {
            string result = null;
            CallWebMethod(delegate(Authentication auth)
            {
                result = _client.getTag(auth, tag);
            }, true);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="start"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If the <paramref name="tag"/> is badly formatted.</exception>
        /// <exception cref="CritSendException">In case of soap error, contains the message.</exception>
        public string GetTagPaginated(string tag, int start, int offset)
        {
            string result = null;
            CallWebMethod(delegate(Authentication auth)
            {
                result = _client.getTagPaginated(auth, tag, start, offset);
            }, true);
            return result;
        }

        /// <summary>
        /// This method lists all existing tags.
        /// </summary>
        /// <returns>An list containing the tags.</returns>
        /// <exception cref="CritSendException">In case of soap error, contains the message.</exception>
        public List<string> ListAllTags()
        {
            string [] result = null;
            CallWebMethod(delegate(Authentication auth)
            {
                result = _client.listAllTags(auth);
            }, true);
            return new List<string>(result);
        }

        /// <summary>
        /// Resets the given tag.
        /// </summary>
        /// <param name="tag">The tag to reset.</param>
        /// <returns><c>true</c> if the tag has been successfully reset; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="tag"/> is badly formatted.</exception>
        /// <exception cref="CritSendException">In case of soap error, contains the message.</exception>
        public bool ResetTag(string tag)
        {
            bool result = false;
            CallWebMethod(delegate(Authentication auth)
            {
                result = _client.resetTag(auth, tag);
            }, true);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="campaignParameters"></param>
        /// <param name="subscribers"></param>
        /// <returns></returns>
        public bool SendCampaign(EmailContent content, CampaignParameters campaignParameters, List<Email> subscribers)
        {
            bool result = false;
            CallWebMethod(delegate(Authentication auth)
            {
                result = _client.sendCampaign(auth, Email.Convert(subscribers), campaignParameters.GetProxy(), content.GetProxy());
            });
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="campaignParameters"></param>
        /// <param name="subscribers"></param>
        /// <returns></returns>
        public bool SendEmail(EmailContent content, CampaignParameters campaignParameters, List<Email> subscribers)
        {
            bool result = false;
            CallWebMethod(delegate(Authentication auth)
            {
                result = _client.sendEmail(auth, Email.Convert(subscribers), campaignParameters.GetProxy(), content.GetProxy());
            }, true);
            return result;
        }

        /// <summary>
        /// Sets the usage of the hosts of the FastHosts configuration section.
        /// </summary>
        /// <param name="isFast"></param>
        public void SetFastDelivery(bool isFast)
        {
            if (!_fast && isFast)
            {
                int index = _random.Next(_configuration.FastHosts.Count);
                string host = _configuration.FastHosts[index];
                _client.Url = host + Wsdl;
            }
            else if (_fast && !isFast)
            {
                _client.Url = _host;
            }
        }

        /// <summary>
        /// Encapsulates the call to a web method.
        /// </summary>
        /// <param name="action">The delegate to the method executing a web method.</param>
        /// <param name="useMainHosts">Set to true to use the main hosts to call the web service.</param>
        private void CallWebMethod(Action<Authentication> action, bool useMainHosts)
        {
            string currentHost = _client.Url;
            try
            {
                if (useMainHosts)
                    _client.Url = GetRandomHost(_configuration.MainHosts);
                action(GetAuthentication());
            }
            catch (SoapException ex)
            {
                _logger.Error("Exception caught while calling method '" + 
                    action.Method.Name + "'", ex);
                _logger.Info("Reconnecting and retrying...");
                Connect();
                action(GetAuthentication());
            }
            finally
            {
                _client.Url = currentHost;
            }
        }

        private void CallWebMethod(Action<Authentication> action)
        {
            CallWebMethod(action, false);
        }

        /// <summary>
        /// Gets a working host from the passed array of urls.
        /// </summary>
        /// <param name="hosts">Array containing the hosts to test.</param>
        /// <returns></returns>
        private WSDL01forMxMaster GetFallbackHost(IList<string> hosts)
        {
            WSDL01forMxMaster client = null;
            string host = null;
            //We try all the hosts.
            while (client == null)
            {
                host = hosts[_random.Next(hosts.Count)];
                client = CreateClient(host);
            }
            _host = host;

            //If we did not find anything suitable
            if (client == null)
            {
                throw new CritSendException("MxmConnect has no more available host.");
            }
            return client;
        }

        private void Connect(IList<string> hosts)
        {
            var size = hosts.Count;
            if (size > 0)
            {
                string host = GetRandomHost(hosts);
                _client.Url = host;
            }
            else
            {
                throw new CritSendException("MxmConnect has no more available host.");
            }
        }

        private string GetRandomHost(IList<string> hosts)
        {
            int size = hosts.Count;
            int index = 0;
            if (size > 1)
                index = _random.Next(size);
            string host = hosts[index] + Wsdl;
            return host;
        }

        private void Connect()
        {
            Connect(_configuration.Hosts);
        }

        /// <summary>
        /// Creates a random web service client instance.
        /// </summary>
        /// <param name="hosts"></param>
        /// <returns></returns>
        private WSDL01forMxMaster CreateClient(IList<string> hosts)
        {
            return CreateClient(GetRandomHost(hosts));
        }

        /// <summary>
        /// Creates a web service client instance.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private WSDL01forMxMaster CreateClient(string host)
        {
            WSDL01forMxMaster client;
            try
            {
                client = new WSDL01forMxMaster { EnableDecompression = true, Url = host + Wsdl };

                // If the url does not work an exception is thrown.
                var auth = GetAuthentication();
                client.isTag(auth, "test");
            }
            catch (Exception)
            {
                return null;
            }
            return client;
        }

        /// <summary>
        /// Generates an <see cref="Authentication"/> instance based on the current credentials and time.
        /// </summary>
        /// <returns></returns>
        private Authentication GetAuthentication()
        {
            DateTime timestamp = System.DateTime.Now;
            // Milliseconds seem to be a problem...
            timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                timestamp.Hour, timestamp.Minute, timestamp.Second, 0).ToUniversalTime();

            WS.Authentication auth = new WS.Authentication();
            auth.user = _user;
            auth.timestamp = timestamp;

            HMACSHA256 hmac = new HMACSHA256(System.Text.Encoding.UTF8.GetBytes(_key));
            string password = Namespace + _user + timestamp.ToString(TimeStampFormat);

            auth.signature = ConvertToHex(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));

            return auth;
        }

        /// <summary>
        /// Converts the passed bytes array to a hexadecimal string.
        /// </summary>
        /// <param name="asciiString"></param>
        /// <returns></returns>
        private static string ConvertToHex(byte[] asciiString)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < asciiString.Length; i++)
            {
                string s = asciiString[i].ToString("x2");
                if (s.Length == 1)
                    sb.Append(0);
                sb.Append(s);
            }

            return sb.ToString();
        }
    }
}
