using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
        None
    }

    public class QueryResultEventArgs : EventArgs
    {
        public IQueryResult QueryResult { get; }

        public QueryResultEventArgs(IQueryResult queryResult)
        {
            QueryResult = queryResult;
        }
    }

    //public class QueryExceptionEventArgs : EventArgs
    //{
    //    public QueryException QueryException { get; }

    //    public QueryExceptionEventArgs(QueryException queryException)
    //    {
    //        QueryException = queryException;
    //    }
    //}
}
