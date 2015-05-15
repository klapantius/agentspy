using Nancy;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace AgentMonitorLocal_prototype
{
    class HttpInterface: NancyModule, IDisposable
    {
        internal const int PORT = 7789;
        //internal List<string> myConnections = new List<string>() { };
        internal List<string> myConnections = new List<string>() { "EH5DE01G0020PC.ww005.siemens.net" };

        internal const string StatusRequest = "subscribe";
        internal const string StatusReport = "status";

        public HttpInterface() : base("/")
        {
            Post[StatusRequest] = parameters =>
            {
                if (!myConnections.Contains(Request.UserHostAddress))
                {
                    myConnections.Add(Request.UserHostAddress);
                    Console.WriteLine("status request accepted from {0}", Request.UserHostAddress);
                }
                return "ok";
            };
            Post[StatusReport] = parameters =>
            {
                Console.WriteLine("Got status report: {0}", Request.Body);
                // raise an event
                return "ok";
            };
            Get["/"] = parameters => "Hello World";
        }

        public void SendStatus(string status)
        {
            myConnections.AsParallel().ForAll(connection =>
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(ConnectionUrl(connection));
                    var data = Encoding.ASCII.GetBytes(status.ToString(CultureInfo.InvariantCulture));
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.ContentLength = data.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var response = request.GetResponse();
                    response.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error caught while trying to send status to {0}:", ConnectionUrl(connection));
                    Console.WriteLine(e.Message);
                }
            });
        }

        public void SendStatusRequests()
        {
            myConnections.AsParallel().ForAll(connection =>
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(ConnectionUrl(connection));
                    var data = Encoding.ASCII.GetBytes(StatusRequest);
                    request.Method = "POST";
                    request.ContentType = "text";
                    request.ContentLength = data.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var response = request.GetResponse();
                    response.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception caught while trying to send status request to {0}:", ConnectionUrl(connection));
                    Console.WriteLine(e.Message);
                }
            });
        }

        private static Uri ConnectionUrl(string client)
        {
            return new UriBuilder("http", client, PORT).Uri;
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Close();
        }

    }
}
