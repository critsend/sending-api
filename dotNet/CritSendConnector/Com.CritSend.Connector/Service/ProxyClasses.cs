using System;
using System.Collections.Generic;
using System.Text;
using WS = Com.CritSend.Connector.com.messaging_master.mail;

namespace Com.CritSend.Connector
{

    public class CampaignParameters
    {
        private readonly List<string> _tags = new List<string>();

        public string MailFrom { get; set; }
        public string MailFromFriendly { get; set; }
        public string ReplyTo { get; set; }
        public bool ReplyToFiltered { get; set; }

        public List<string> Tags
        {
            get
            {
                return _tags;
            }
        }

        public CampaignParameters()
        {

        }

        public CampaignParameters(List<string> tags, string mailFrom, string mailFromFriendly, string replyTo, bool replyToFiltered)
        {
            _tags.AddRange(tags);
            MailFrom = mailFrom;
            MailFromFriendly = mailFromFriendly;
            ReplyTo = replyTo;
            ReplyToFiltered = replyToFiltered;
        }

        internal WS.CampaignParameters GetProxy()
        {
            WS.CampaignParameters proxy = new WS.CampaignParameters();
            proxy.mailfrom = MailFrom;
            proxy.mailfrom_friendly = MailFromFriendly;
            proxy.replyto = ReplyTo;
            proxy.replyto_filtered = ReplyToFiltered;
            proxy.tag = Tags.ToArray();
            return proxy;
        }
    }

    public class EmailContent
    {
        public string Html { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }

        public EmailContent()
        {

        }

        public EmailContent(string subject, string html, string text)
        {
            Subject = subject;
            Html = html;
            Text = text;
        }

        internal WS.Content GetProxy()
        {
            WS.Content proxy = new WS.Content();
            proxy.html = Html;
            proxy.subject = Subject;
            proxy.text = Text;
            return proxy;
        }
    }

    public class Email
    {
        public string Address { get; set; }
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public string Field6 { get; set; }
        public string Field7 { get; set; }
        public string Field8 { get; set; }
        public string Field9 { get; set; }
        public string Field10 { get; set; }
        public string Field11 { get; set; }
        public string Field12 { get; set; }
        public string Field13 { get; set; }
        public string Field14 { get; set; }
        public string Field15 { get; set; }

        public Email()
        {

        }

        public Email(string address)
        {
            Address = address;
        }

        internal com.messaging_master.mail.Email GetProxy()
        {
            com.messaging_master.mail.Email proxy = new com.messaging_master.mail.Email();
            proxy.email = Address;
            proxy.field1 = Field1;
            proxy.field2 = Field2;
            proxy.field3 = Field3;
            proxy.field4 = Field4;
            proxy.field5 = Field5;
            proxy.field6 = Field6;
            proxy.field7 = Field7;
            proxy.field8 = Field8;
            proxy.field9 = Field9;
            proxy.field10 = Field10;
            proxy.field11 = Field11;
            proxy.field12 = Field12;
            proxy.field13 = Field13;
            proxy.field14 = Field14;
            proxy.field15 = Field15;
            return proxy;
        }

        internal static com.messaging_master.mail.Email[] Convert(List<Email> subscribers)
        {
            Email[] proxies = subscribers.ToArray();
            return Array.ConvertAll<Email, com.messaging_master.mail.Email>(proxies, e => e.GetProxy());
        }
    }
}
