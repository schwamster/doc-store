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
    public class NotificationApiClient
    {
        private readonly IConfiguration config;
        private readonly ILogger logger;

        public NotificationApiClient(IConfiguration config, ILogger logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public async void PushNotification(object obj)
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
