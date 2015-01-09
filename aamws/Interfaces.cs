using System.Collections.Generic;
using aamcommon;
using tail;

namespace aamws
{
    interface IJob
    {
        string Id { get; }
        JobStatus Status { get; }
        string Error { get; }

        void Update(Dictionary<Field, string> fieldsToBeUpdated);
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
