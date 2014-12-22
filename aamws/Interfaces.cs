using tail;

namespace aamws
{
    interface IJob
    {
        void Update(string status, string build, string assembly, string tc);
    }

    interface IJobFactory
    {
        IJob Create(string id);
    }

    interface IChangeAnalyzer
    {
        void TailUpdateHandler(object o, TailEventArgs e);
        void Remove(string jobid);
    }
}
