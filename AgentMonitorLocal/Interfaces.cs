using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AgentMonitorLocal_prototype
{
    public interface ISimpleResponse
    {
        HttpStatusCode StatusCode { get; }
        string Content { get; }
    }
    
    public delegate ISimpleResponse SimpleResponder(HttpListenerRequest request);

    public interface IExecutorModule
    {
        SimpleResponder HttpRequestProcessor();

        ISimpleResponse HttpRequestProcessor(HttpListenerRequest request);
    }
}
