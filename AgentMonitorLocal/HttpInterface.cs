using System.IO;
using Nancy;
using Nancy.Extensions;
using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace AgentMonitorLocal_prototype
{
    public class HttpInterface
    {
        internal const int PORT = 7789;

        internal const string StatusRequest = "subscribe";
        internal const string StatusReport = "status";


        protected static byte[] PrepareStatusForSend(string status)
        {
            var data = Encoding.ASCII.GetBytes(status.ToString(CultureInfo.InvariantCulture));
            return data;
        }

        protected static string GetResponseString(WebResponse response)
        {
            var responseString = string.Empty;
            using (var stream = response.GetResponseStream())
            {
                if (null == stream) return responseString;
                var reader = new StreamReader(stream, Encoding.UTF8);
                responseString = reader.ReadToEnd();
            }
            return responseString;
        }

        protected static Uri ConnectionUrl(string client, string request)
        {
            return new UriBuilder("http", client, PORT, request).Uri;
        }

    }

}
