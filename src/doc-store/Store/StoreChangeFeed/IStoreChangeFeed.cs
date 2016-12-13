using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.Driver.Net;

namespace doc_store.Store.StoreChangeFeed
{
    /// <summary>
    /// this contract is about defining something that happens on certain events. E.g. an insert in your store, triggers an event that can be used to e.g. notify someone of that insert.
    /// </summary>
    public interface IStoreChangeFeed
    {
        /// <summary>
        /// subscribe to the desired change watchers
        /// </summary>
        /// <param name="watchers">the list of watchers</param>
        void Subscribe(List<IRethinkChangeWatcher> watchers);
    }

    public class RethinkChangeFeed : IStoreChangeFeed
    {
        private readonly Connection conn;
        private readonly IActionOnEventRunner eventRunner;

        public RethinkChangeFeed(Connection conn, IActionOnEventRunner onEventRunner)
        {
            this.conn = conn;
            this.eventRunner = onEventRunner;
        }

        public void Subscribe(List<IRethinkChangeWatcher> watchers)
        {
            watchers.ForEach(async w => await SubscribeToWatcher(w));
        }

        /// <summary>
        /// subscribes to a single watcher
        /// </summary>
        /// <param name="watcher"></param>
        /// <returns></returns>
        private async Task SubscribeToWatcher(IRethinkChangeWatcher watcher)
        {
            //TODO: implement filtering by client if specified in watcher 
            var feed = await watcher.ExpressionToWatch.Changes().RunChangesAsync<object>(conn);
            while (await feed.MoveNextAsync())
            {
                this.eventRunner.ExecuteOnEvent(feed.Current);
            }
        }
    }
}
