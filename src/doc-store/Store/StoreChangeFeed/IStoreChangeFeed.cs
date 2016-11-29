using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.Driver.Net;

namespace doc_store.Store.StoreChangeFeed
{
    public interface IStoreChangeFeed
    {
        /// <summary>
        /// subscribe to the desired change watchers
        /// </summary>
        /// <param name="watchers">the list of watchers</param>
        void Subscribe(List<ChangeWatcher> watchers);
    }

    public class StoreChangeFeed : IStoreChangeFeed
    {
        private readonly Connection conn;

        public StoreChangeFeed(Connection conn)
        {
            this.conn = conn;
        }

        public void Subscribe(List<ChangeWatcher> watchers)
        {
            watchers.ForEach(async w => await SubscribeToWatcher(w));
        }

        /// <summary>
        /// subscribes to a single watcher
        /// </summary>
        /// <param name="watcher"></param>
        /// <returns></returns>
        private async Task SubscribeToWatcher(ChangeWatcher watcher)
        {
            //TODO: implement filtering by client if specified in watcher 
            var feed = await watcher.ExpressionToWatch.Changes().RunChangesAsync<object>(conn);
            while (await feed.MoveNextAsync())
            {
                watcher.RunOnEvent(feed.Current);
            }
        }
    }
}
