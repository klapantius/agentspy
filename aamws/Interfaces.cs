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

        void Update(IDictionary<Field, string> fieldsToBeUpdated);
    }

    interface IJobFactory
    {
        IJob Create(string id);
    }

    public delegate void LogParserEventHandler(string jobid, IDictionary<Field, string> fieldsToBeUpdated, bool publish);

    public interface ILogParser : IDisposable
    {
        event LogParserEventHandler Changed;

        void TailUpdateHandler(object o, TailEventArgs e);
        void Remove(string jobid);
    }

    public interface ILogParserRule
    {
        string Rule { get; }

        bool IsMatching(string line);
        IDictionary<Field, string> Parse(string line);
    }


}
