using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.Driver.Ast;

namespace doc_store.Store.StoreChangeFeed
{
    public interface IChangeWatcher
    {

    }

    public interface IRethinkChangeWatcher: IChangeWatcher
    {
        /// <summary>
        /// The rethinkDB expression you want to watch for changes. This can be either a table or some whatever expression like insert or smth
        /// </summary>
        ReqlExpr ExpressionToWatch { get; set; }

        /// <summary>
        /// set this if you want that your watched expression only for specific clients.  
        /// </summary>
        long? LimitWatchToSingleClient { get; set; }
    }

    /// <summary>
    /// a watcher describes a Rethink expression to watch for changes, and an action to execute on event
    /// </summary>
    /// TODO: Refactor me later to be valid for other stores than Rethink as well
    public class RethinkChangeWatcher : IRethinkChangeWatcher
    {
        public ReqlExpr ExpressionToWatch { get; set; }

        public long? LimitWatchToSingleClient { get; set; }
    }
}
