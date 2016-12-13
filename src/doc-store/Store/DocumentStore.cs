using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RethinkDb.Driver;
using System;


namespace doc_store.Store
{
    public class DocumentStore : IDocumentStore
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private Lazy<RethinkDbStore> store;

        public DocumentStore(IConfiguration configuration, ILogger<DocumentStore> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.store = new Lazy<RethinkDbStore>(() => new RethinkDbStore(configuration, logger));
        }

        public DocumentAddResult AddDocument(Document document)
        {

            var storeInstance = this.store.Value;

            var toSave = new StoreDocument(document) {
                Inserted = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                State = new[] { "persisted" },
                Version = 1,
                DocumentSequenceId = document.Id
            };

            //first check if the document maybe already exists - right now we will jusBt override
            var existingDoc = store.Value.GetDocument("client_name_user", store.Value.R.Array(toSave.Client, toSave.Name, toSave.User));

            if(existingDoc != null)
            {
                toSave.Version = existingDoc.Version + 1;
                toSave.DocumentSequenceId = existingDoc.DocumentSequenceId;
            }


            this.store.Value.InsertDocument(toSave);
            this.logger.LogInformation($"saved document '{document.Id}/{document.Name}' in db");

            return new DocumentAddResult() { Result = Result.Success, DocumentId = document.Id };
        }

        public StoreDocument AddExtractedText(Guid id, string extractedText)
        {
            return this.store.Value.AddExtractedText(id, extractedText);
        }

        public StoreDocument GetDocument(Guid id)
        {
            return this.store.Value.GetDocument(id);
        }
    }
}