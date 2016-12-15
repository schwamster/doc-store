using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace doc_store.Store
{
    public class Document
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("client")]
        public string Client { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Base64 Encoded Byte Array representation of the file
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("extractedText")]
        public string ExtractedText { get; set; }

        public string[] State { get; set; }
        [JsonProperty("version")]
        public int Version { get; set; }

        /// <summary>
        /// If a document has multiple versions each document has a different id, they do share the documentSequenceId
        /// The documentSequenceId equals the Id of the first document in the sequence
        /// </summary>
        [JsonProperty("documentSequenceId")]
        public Guid DocumentSequenceId { get; set; }
    }

    public static class DocumentMapper
    {
        public static Document Convert(this StoreDocument input)
        {
            return new Store.Document()
            {
                Client = input.Client,
                Content = input.Content,
                DocumentSequenceId = input.DocumentSequenceId,
                ExtractedText = input.ExtractedText,
                Id = input.Id,
                Name = input.Name,
                State = input.State,
                User = input.User,
                Version = input.Version
            };
        }
    }

    public class StoreDocument : Document
    {
        public StoreDocument()
        {

        }
        public StoreDocument(Document document)
        {
            //change client later to come from auth token instead of request body.
            this.Client =  document.Client ?? "1337";
            this.Content = document.Content;
            this.Id = document.Id;
            this.Name = document.Name;
            this.User = document.User;
        }
        [JsonProperty("inserted")]
        public DateTime Inserted { get; set; }
        [JsonProperty("updated")]
        public DateTime Updated { get; set; }
        [JsonProperty("state")]
        public string[] State { get; set; }
        [JsonProperty("version")]
        public int Version { get; set; }

        /// <summary>
        /// If a document has multiple versions each document has a different id, they do share the documentSequenceId
        /// The documentSequenceId equals the Id of the first document in the sequence
        /// </summary>
        [JsonProperty("documentSequenceId")]
        public Guid DocumentSequenceId { get; set; }
    }
}
