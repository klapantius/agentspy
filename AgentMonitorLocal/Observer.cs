using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using aamcommon;
using Nancy;
using Nancy.Extensions;
using Nancy.Hosting.Self;
using HttpStatusCode = Nancy.HttpStatusCode;

namespace AgentMonitorLocal_prototype
{
    public class Observer : HttpInterface
    {

        private List<Connection> myConnections = new List<Connection>()
        {
            new Connection("EH5DE01T0159PC", ""),
            new Connection("EH5DE01T0126PC", ""),
            new Connection("EH5DE01T0162PC", ""),
        };

        public Observer()
        {
            ObserverModule.Observer = this;
            var myNancyModule = new ObserverModule();

            myNancyModule.Start();

            SendStatusRequests(myConnections.Select(c => c.Name).ToArray());
        }

        public void SendStatusRequests(params string[] agents)
        {
            agents/*.ToList().ForEach*/.AsParallel().ForAll(connection =>
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(ConnectionUrl(connection, StatusRequest));
                    var data = Encoding.ASCII.GetBytes(StatusRequest);
                    request.Method = "POST";
                    request.ContentType = "text";
                    request.ContentLength = data.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    using (var response = (HttpWebResponse) request.GetResponse())
                    {
                        var responseString = GetResponseString(response);
                        Console.WriteLine(
                            response.StatusCode == System.Net.HttpStatusCode.Accepted
                                ? "connection to {0} established"
                                : "{0} rejected the connection request", connection);
                        //Console.WriteLine("response from {0}: {1}", connection, responseString);
                        response.Close();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception caught while trying to send status request to {0}:", ConnectionUrl(connection, StatusRequest));
                    Console.WriteLine(e.Message);
                }
            });
        }

        public bool StatusUpdateHandler(string json)
        {
            var received = new TestAgentStatusRecord { AsJson = json };
            var agent = myConnections.SingleOrDefault(c => c.Name == received[Field.AgentName]);
            if (agent == null)
            {
                Console.WriteLine("Status update from an unknown agent: {0}", received[Field.AgentName]);
                return false;
            }
            if (agent.Record == null)
            {
                Console.WriteLine("first message arrived from {0}", received[Field.AgentName]);
            }
            else
            {
                Console.WriteLine("status update from {0}", received[Field.AgentName]);
            }
            agent.Record = received;
            Console.WriteLine(agent.Record);
            return true;
        }

    }

    public class ObserverModule : NancyModule, IDisposable
    {
        private NancyHost myHost;
        public static Observer Observer { get; set; }

        public ObserverModule()
        {
            Post[HttpInterface.StatusReport] = parameters =>
            {
                //Console.WriteLine("Got status report from: {0}", Request.UserHostAddress);
                //var msg = Request.Body.AsString();
                //if (!Observer.StatusUpdateHandler(msg))
                //{
                //    if (string.IsNullOrEmpty(msg))
                //        Console.WriteLine("Empty status message from {0}", Request.UserHostAddress);
                //    else
                //        Console.WriteLine("Status update from an unknown agent: \"{0}\" ({1})",
                //            new TestAgentStatusRecord { AsJson = msg }[Field.AgentName], Request.UserHostAddress);
                //    return new Response() { StatusCode = HttpStatusCode.BadRequest };
                //}
                return "ok";
            };
            Post["/"] = parameters =>
            {
                Console.WriteLine("got a post");
                return "Hello World";
            };
            Get["/"] = parameters =>
            {
                Console.WriteLine("got a get");
                return "Hello World";
            };
        }

        public void Start()
        {
            myHost = new NancyHost(new Uri("http://localhost:" + HttpInterface.PORT));
            myHost.Start();
            Console.WriteLine("Listening on port {0}", HttpInterface.PORT);
        }

        public void Close()
        {
            if (null != myHost)
                myHost.Stop();
        }

        public void Dispose()
        {
            Close();
        }

    }

}
