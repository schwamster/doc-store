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
        /// change watchers will run only on specific clients if set; leave empty string to watch all clients.
        /// </summary>
        string LimitWatchToClient { get; set; }

        /// <summary>
        /// subscribe to the desired change watchers
        /// </summary>
        /// <param name="watchers">the list of watchers</param>
        Task Subscribe(List<IRethinkChangeWatcher> watchers);

        IActionOnEventRunner eventRunner { get; set; }
    }

    public class RethinkChangeFeed : IStoreChangeFeed
    {
        private readonly Connection conn;
        public IActionOnEventRunner eventRunner { get; set; }
        public string LimitWatchToClient { get; set; }

        public RethinkChangeFeed(Connection conn, IActionOnEventRunner onEventRunner)
        {
            this.conn = conn;
            this.eventRunner = onEventRunner;
        }

        /// <summary>
        /// Subscribes to changes for all watchers sent in param
        /// </summary>
        /// <param name="watchers"></param>
        /// <returns></returns>
        public async Task Subscribe(List<IRethinkChangeWatcher> watchers)
        {
            var tasks = new List<Task>();
            watchers.ForEach(t => tasks.Add(SubscribeToWatcher(t)));

            await Task.WhenAll(tasks);

            //watchers.ForEach(async w => await SubscribeToWatcher(w));
        }

        /// <summary>
        /// subscribes to a single watcher
        /// </summary>
        /// <param name="watcher"></param>
        /// <returns></returns>
        private async Task SubscribeToWatcher(IRethinkChangeWatcher watcher)
        {
            var waitFor = !string.IsNullOrEmpty(LimitWatchToClient) ?
                watcher.ExpressionToWatch.Filter(t => t["client"].Eq(LimitWatchToClient)) :
                watcher.ExpressionToWatch;

            var feed = await waitFor.Changes().RunChangesAsync<object>(conn);
            while (await feed.MoveNextAsync())
            {
                await eventRunner.ExecuteOnEvent(feed.Current);
            }
        }
    }
}
