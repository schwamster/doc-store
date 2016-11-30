using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace doc_store.Store.StoreChangeFeed
{
    /// <summary>
    /// a watcher describes a Rethink expression to watch for changes, and an action to execute on event
    /// </summary>
    public class ChangeWatcher
    {
        /// <summary>
        /// The rethinkDB expression you want to watch for changes. This can be either a table or some whatever expression like insert or smth
        /// </summary>
        public RethinkDb.Driver.Ast.ReqlExpr ExpressionToWatch { get; set; }

        /// <summary>
        /// the action to run when an event occurs, based on the expression that you watched
        /// </summary>
        public Action<object> RunOnEvent { get; set; }

        /// <summary>
        /// set this if you want that your watched expression only for specific clients.  
        /// </summary>
        public long? LimitWatchToSingleClient { get; set; }
    }
}
