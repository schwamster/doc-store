using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace doc_store.Store.StoreChangeFeed
{
    public interface IActionOnEventRunner
    {
        /// <summary>
        /// define an action to run when the event triggers
        /// </summary>
        /// <param name="target">the object that cotnains details about the action is executed for</param>
        /// <returns></returns>
        void ExecuteOnEvent(object target);
    }

    public class PushNotificationEventRunner : IActionOnEventRunner 
    {
        private readonly IConfiguration config;
        private readonly ILogger logger;

        public PushNotificationEventRunner(IConfiguration config, ILogger logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public async void ExecuteOnEvent(object obj)
        {
            var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                var uri = $"http://{this.config["Endpoints:NotificationApi"]}/notification";
                try
                {
                    using (var message = await client.PostAsync(uri, content))
                    {
                        logger.LogDebug($"success post to: {uri}, received: {message}");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"failed post to: {uri}, reason: {ex}");
                }
            }
        }
    }
}
