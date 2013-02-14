using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections.Generic; // List<T>
using Mono.Data.Sqlite;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

// Own
using Tools;

namespace SQLiteServer
{
    public class QueryCache
    {
        public struct TQueryCacheItem
        {
            public string Query;
            public string AccessRight;
        }
        
    
        private static List<TQueryCacheItem> QueryQueue;
        private Thread threadHandleQueryQueue;

        // Constructor
        public QueryCache()
        {
            QueryQueue = new List<TQueryCacheItem>();
            
            // Init Query Queue Handler Thread
            threadHandleQueryQueue = new Thread(HandleQueryQueue);
            threadHandleQueryQueue.Priority = ThreadPriority.Lowest;
            threadHandleQueryQueue.Start ();
        }

        // Destructor
        ~ QueryCache ()
        {
            // Stop Query Queue Handler Thread
            threadHandleQueryQueue.Interrupt();
            threadHandleQueryQueue = null;

            // Clear Query Queue
            QueryQueue.Clear();
        }

        public void AddQuery(TQueryCacheItem AQuery)
        {
            QueryQueue.Add( AQuery );
        }

        // Handle Query Queue
        private void HandleQueryQueue ()
        {
            try
            {
                while (true) {

                    // Execute Query
                    lock (Sync.signal)
                    {
                        // TODO: Run p.e. 10 querys a time and group by AccessRight
                        if (QueryQueue.Count > 0) {
                            TQueryCacheItem QueryCacheItem;
                            QueryCacheItem = QueryQueue[0];
                            QueryQueue.RemoveAt(0);
                            Console.WriteLine ( QueryCacheItem.AccessRight + " " + "C" + " "  + QueryCacheItem.Query);  
                            string QueryResult = String.Empty;
                            MainClass.SQLite.ExecuteSQL(QueryCacheItem.Query, QueryCacheItem.AccessRight, ref QueryResult, true);
                        }
                    }

                    Thread.Sleep(1);
                }
            } catch (Exception e) {
                Console.WriteLine ( "QueryCache Exception: " + e.Message);  
            }
        }   


    }
}

