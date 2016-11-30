using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RethinkDb.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using doc_store.Store.StoreChangeFeed;

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

    public class RethinkDbStore
    {
        public RethinkDB R = RethinkDB.R;
        private ILogger logger;
        private string ip;
        private RethinkDb.Driver.Ast.Table documentTable;

        public RethinkDbStore(IConfiguration config, ILogger logger)
        {
            this.logger = logger;
            string rethinkHost = config["RethinkDb:Host"];
            string dbName = config["RethinkDb:DatabaseName"];
            string tableName = config["RethinkDb:TableName"];

            //indexes
            const string name_client_user_index = "client_name_user";

            logger.LogInformation($"Initializing RethinkDb on host '{rethinkHost}'");

            this.ip = GetIp(rethinkHost);

            RethinkDb.Driver.Net.Connection c = NewConnection();

            List<string> dbs = R.DbList().Run<List<string>>(c);
            if (!dbs.Contains(dbName))
            {
                logger.LogInformation("Db does not exist. Creating...");
                R.DbCreate(dbName).Run(c);
                logger.LogInformation("Db created");
            }
            var db = R.Db(dbName);

            List<string> tables = db.TableList().Run<List<string>>(c);
            if (!tables.Contains(tableName))
            {
                logger.LogInformation("Documents table does not exist. Creating...");
                db.TableCreate(tableName).Run(c);
                logger.LogInformation("Table created");
            }
            documentTable = db.Table(tableName);

            List<string> indexes = documentTable.IndexList().Run<List<string>>(c);
            if (!indexes.Contains(name_client_user_index))
            {
                logger.LogInformation("Creating indexes...");
                RethinkDb.Driver.Ast.ReqlFunction1 pathIx = row => { return R.Array(row["client"], row["name"], row["user"]); };
                documentTable.IndexCreate(name_client_user_index, pathIx).Run(c);
                documentTable.IndexWait(name_client_user_index).Run(c);
                logger.LogInformation("Created all indexes");
            }

            var notificationClient = new NotificationApiClient(config, logger);
            SubscribeToChanges(c, notificationClient);
        }

        private void SubscribeToChanges(RethinkDb.Driver.Net.Connection c, NotificationApiClient notificationClient)
        {
            var changeFeed = new StoreChangeFeed.StoreChangeFeed(c);
            changeFeed.Subscribe(new List<ChangeWatcher>()
            {
                new ChangeWatcher()
                {
                    ExpressionToWatch = documentTable,
                    RunOnEvent = (t) => notificationClient.PushNotification(t)
                }
            });
        }

        private RethinkDb.Driver.Net.Connection NewConnection()
        {
            return R.Connection()
             .Hostname(ip)
             .Port(RethinkDBConstants.DefaultPort)
             .Timeout(60)
             .Connect();
        }

        public void InsertDocument(object document)
        {
            var c = NewConnection();

            documentTable.Insert(document).Run(c);
        }

        public StoreDocument AddExtractedText(Guid id, string extractedText)
        {
            var c = NewConnection();

            const string textExtractedState = "text-extracted";

            StoreDocument document = this.documentTable.Get(id).Pluck("state").Run<StoreDocument>(c);

            string[] states = new string[] { };

            if (!document.State.Contains(textExtractedState))
            {
                var state = document.State.ToList();
                state.Add(textExtractedState);
                states = state.ToArray();
            }

            var update = new {
                extractedText = extractedText,
                updated = DateTime.UtcNow,
                state = states
            };



            var result = this.documentTable.Get(id).Update(update).Run(c);

            return GetDocument(id);
        }

        public StoreDocument GetDocument(Guid id)
        {
            //todo: is it smart to open the connection every time??
            var c = NewConnection();

            StoreDocument doc = this.documentTable.Get(id).Run<StoreDocument>(c);
            return doc;
        }

        public StoreDocument GetDocument(string index, params object[] exp)
        {
            //todo: is it smart to open the connection every time??
            var c = NewConnection();

            RethinkDb.Driver.Net.Cursor<StoreDocument> docs = this.documentTable.GetAll(exp)[new { index = index }].Run<StoreDocument>(c);

            //TODO: only get the first plus do it all in the db, thats what it is for..
            foreach (var doc in docs.OrderByDescending(x => x.Version))
            {
                this.logger.LogInformation("found matching doc");
                return doc;
            }

            return null;
        }

        private static string GetIp(string hostname)
            => Dns.GetHostEntryAsync(hostname)
                .Result
                .AddressList
                .First(a => a.AddressFamily == AddressFamily.InterNetwork)
                .ToString();
    }
}