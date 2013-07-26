using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using Com.CritSend.Connector;
using System.Text;
using System.Configuration;

namespace TestWebSite
{
    public partial class _Default : System.Web.UI.Page
    {

        private void Tests(string user, string key)
        {
            StringBuilder output = new StringBuilder();
            // I create an instance of CritSendConnect. This is the bridge to MxM system.
            CritSendConnect mxm = new CritSendConnect(user, key);

            // A tag as described in the doc is where to put the reporting information, you tag your 
            // delivery. e.g. invoice, alerting, emergencies
            output.Append(mxm.CreateTag("test"));
            output.AppendLine();

            // You have to create them once.
            output.Append(mxm.CreateTag(".net"));
            output.AppendLine();

            // To list all existing tags.
            List<string> gotTags = mxm.ListAllTags();
            foreach (var item in gotTags)
            {
                output.AppendLine(item);
            }
            output.AppendLine();

            // this will output the content of the tags: open, bounced addresses,... See documentation to process them.
            output.Append(mxm.GetTag(".net"));
            output.AppendLine();

            // You can empty it too.
            output.Append(mxm.DeleteTag(".net"));
            output.AppendLine();

            string testEmail = ConfigurationManager.AppSettings["testEmail"];

            // You need just the part below to send an email.
            // to send an email you first need to create a list of recipients
            List<Email> subscribers = new List<Email>();
            subscribers.Add(new Email(testEmail));

            // I set my content. I can leave html or text empty.
            EmailContent content = new EmailContent("test subject", "<b>some html content</b>", "plain text");

            // I set my tags
            List<string> tags = new List<string>();
            tags.Add("test");

            // and the parameters of my campaign.
            CampaignParameters campaignParameters = new CampaignParameters(tags, "mail@messaging-master.com", user ?? "CritSend_Test_user", testEmail, true);

            // And then send the campaign
            output.Append(mxm.SendCampaign(content, campaignParameters, subscribers));
            output.AppendLine();
            Response.Write(output.ToString());
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "text/plain";
            //Tests(null, null);
            Tests("test163@mxmtech.net", "AckaalGLjUQyRbHMGuX");
            Tests("testdude@mxmtech.net", "MKJjeSCXFtj");
        }
    }
}