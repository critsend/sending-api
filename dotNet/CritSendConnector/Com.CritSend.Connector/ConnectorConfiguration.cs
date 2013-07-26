using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Com.CritSend.Connector
{

    /// <summary>
    /// Contains configuration for the <see cref="Com.CritSend.Connector.CritSendConnect"/> class.
    /// </summary>
    internal class ConnectorConfiguration
    {

        /// <summary>
        /// Initializes a new instance of <see cref="ConnectorConfiguration"/>.
        /// </summary>
        internal ConnectorConfiguration()
        {

        }

        /// <summary>
        /// Unique instance of <see cref="ConnectorConfiguration"/>.
        /// </summary>
        private static ConnectorConfiguration _instance = null;
        /// <summary>
        /// Object used for thread-safety in the <see cref="Instance"/> property.
        /// </summary>
        private static readonly object padlock = new object();

        /// <summary>
        /// Gets the unique instance of <see cref="ConnectorConfiguration"/>.
        /// </summary>
        public static ConnectorConfiguration Instance 
        { 
            get
            {
                lock (padlock)
                {
                    if (_instance == null)
                        _instance = ConfigurationManager.GetSection("critSend") as ConnectorConfiguration;
                }
                return _instance;
            }
         }

        /// <summary>
        /// Gets or sets the list of the urls hosting the web service.
        /// </summary>
        internal string MainHostsRaw { get; set; }

        /// <summary>
        /// Gets or sets the list of the urls hosting the web service.
        /// </summary>
        internal string HostsRaw { get; set; }

        /// <summary>
        /// Get or sets the fast host url.
        /// </summary>
        internal string FastHostsRaw { get; set; }

        /// <summary>
        /// User to use to authenticate to the web service.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Key to use to authenticate to the web service.
        /// </summary>
        public string Key { get; set; }

        #region Tools

        /// <summary>
        /// HostsRaw property split by ';'.
        /// </summary>
        private List<string> _hosts;

        /// <summary>
        /// FastHostsRaw property split by ';'.
        /// </summary>
        private List<string> _fastHostsTab;

        /// <summary>
        /// MainHostsRaw property split by ';'.
        /// </summary>
        private List<string> _mainHostsTab;

        /// <summary>
        /// Access to the split host string.
        /// </summary>
        /// <returns></returns>
        public List<string> Hosts
        {
            get
            {
                if (_hosts == null)
                    _hosts = new List<string>(HostsRaw.Split(';'));
                return _hosts;
            }
        }

        /// <summary>
        /// Access to the split host string.
        /// </summary>
        /// <returns></returns>
        public List<string> FastHosts
        {
            get
            {
                if (_fastHostsTab == null)
                    _fastHostsTab = new List<string>(FastHostsRaw.Split(';'));
                return _fastHostsTab;
            }
        }

        public List<string> MainHosts
        {
            get
            {
                if (_mainHostsTab == null)
                    _mainHostsTab = new List<string>(MainHostsRaw.Split(';'));
                return _mainHostsTab;
            }
        }

        #endregion Tools

    }
}
