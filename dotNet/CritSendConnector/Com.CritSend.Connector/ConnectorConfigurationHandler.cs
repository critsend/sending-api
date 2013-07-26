using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Xml.XPath;

namespace Com.CritSend.Connector
{
    /// <summary>
    /// Handles the critSend configuration section.
    /// </summary>
    public class ConnectorConfigurationSectionHandler : IConfigurationSectionHandler
    {
        /// <summary>
        /// Creates a new instance of <see cref="ConnectorConfiguration"/> based on the configuration.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            var conf = new ConnectorConfiguration();
            XPathNavigator nav = section.CreateNavigator();
            conf.User = GetValue(nav, "user");
            conf.Key = GetValue(nav, "key");
            conf.HostsRaw = GetValue(nav, "hosts");
            conf.MainHostsRaw = GetValue(nav, "mainHosts");
            conf.FastHostsRaw = GetValue(nav, "fastHosts");
            return conf;
        }

        private string GetValue(XPathNavigator nav, string key)
        {
            XPathNavigator tmpNav = nav.SelectSingleNode(key);
            return tmpNav != null ? tmpNav.Value : null;
            //throw new CritSendException(string.Format("Configuration : could not find mandatory key '{0}' in 'critSend' section", key));
        }
    }
}
