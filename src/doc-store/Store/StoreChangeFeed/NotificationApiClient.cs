using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace doc_store.Store.StoreChangeFeed
{
    public static class NotificationApiClient
    {
        //prettify later - read from config env vars, err logging, etc 
        public static async void PushNotification(object obj)
        {
            var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                try
                {
                    using (var message = await client.PostAsync($"http://localhost:8099/", content))
                    {
                        //log OK status if ever needed, or something
                    }
                }
                catch (Exception ex)
                {
                    //leave it for the moment
                }
            }
        }
    }
}
