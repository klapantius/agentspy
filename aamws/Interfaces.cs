using System;
using System.Collections.Generic;
using aamcommon;
using tail;

namespace aamws
{
    public interface IJob
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

    public interface ILogParser : IDisposable
    {
        void TailUpdateHandler(object o, TailEventArgs e);
        void Remove(string jobid);
    }

    public interface ILogParserRule
    {
        string Rule { get; }

        bool IsMatching(string line);
        Dictionary<Field, string> Parse(string line);
    }


}
