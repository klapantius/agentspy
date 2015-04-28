using System.IO;
using System.Net;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Url = System.Security.Policy.Url;

namespace AgentMonitorLocal_prototype
{
    class HttpInterface: NancyModule, IDisposable
    {
        internal const int PORT = 7789;
        private List<string> myClients = new List<string>() { "EH5DE01G0020PC.ww005.siemens.net" };

        public HttpInterface() : base("/")
        {
            Post["subscribe"] = parameters =>
            {
                if (!myClients.Contains(Request.UserHostAddress)) 
                    myClients.Add(Request.UserHostAddress);
                return "ok";
            };
            Get["/"] = parameters => "Hello World";
        }

        public void SendStatus(string status)
        {
            myClients.AsParallel().ForAll(client =>
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(ClientUrl(client));
                    request.Method = "POST";
                    var dataStream = request.GetRequestStream();
                    var byteArray = new byte[status.Length * sizeof(char)];
                    Buffer.BlockCopy(status.ToCharArray(), 0, byteArray, 0, byteArray.Length);
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    var response = request.GetResponse();
                    response.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error caught while trying to send status to {0}:", ClientUrl(client));
                    Console.WriteLine(e.Message);
                }
            });
        }

        private static Uri ClientUrl(string client)
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
