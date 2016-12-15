using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using doc_store.Store.StoreChangeFeed;
using RethinkDb.Driver;
using System.Net;
using System.Net.Sockets;
using RethinkDb.Driver.Net;

namespace doc_store.Store
{
    public interface IRethinkStore
    {
        void InsertDocument(object document);
        IStoreChangeFeed changeFeed { get; set; }
    }

    public class RethinkDbStore : IRethinkStore
    {
        public RethinkDB R = RethinkDB.R;
        public IStoreChangeFeed changeFeed { get; set; }

        private ILogger logger;
        private string ip;
        private RethinkDb.Driver.Ast.Table documentTable;
        private IConfiguration configuration;

        public RethinkDbStore(IConfiguration config, ILogger log)
        {
            this.logger = log;
            
            string rethinkHost = config["RethinkDb:Host"];
            string dbName = config["RethinkDb:DatabaseName"];
            string tableName = config["RethinkDb:TableName"];

            logger.LogInformation($"Initializing RethinkDb on host '{rethinkHost}'");

            var conn = NewConnection();
            ip = GetIp(rethinkHost);

            RethinkDb.Driver.Ast.Db db = EnsureDatabaseExists(dbName, conn);
            EnsureTableExists(tableName, conn, db);

            documentTable = db.Table(tableName);

            EnsureIndexesExist(conn);

            SubscribeToChanges(conn, config, logger);
        }

        private void EnsureIndexesExist(Connection c)
        {
            const string name_client_user_index = "client_name_user";
            List<string> indexes = documentTable.IndexList().Run<List<string>>(c);
            if (!indexes.Contains(name_client_user_index))
            {
                logger.LogInformation("Creating indexes...");
                RethinkDb.Driver.Ast.ReqlFunction1 pathIx = row => { return R.Array(row["client"], row["name"], row["user"]); };
                documentTable.IndexCreate(name_client_user_index, pathIx).Run(c);
                documentTable.IndexWait(name_client_user_index).Run(c);
                logger.LogInformation("Created all indexes");
            }
        }

        private void EnsureTableExists(string tableName, Connection c, RethinkDb.Driver.Ast.Db db)
        {
            List<string> tables = db.TableList().Run<List<string>>(c);
            if (!tables.Contains(tableName))
            {
                logger.LogInformation("Documents table does not exist. Creating...");
                db.TableCreate(tableName).Run(c);
                logger.LogInformation("Table created");
            }
        }

        private RethinkDb.Driver.Ast.Db EnsureDatabaseExists(string dbName, Connection c)
        {
            List<string> dbs = R.DbList().Run<List<string>>(c);
            if (!dbs.Contains(dbName))
            {
                logger.LogInformation("Db does not exist. Creating...");
                R.DbCreate(dbName).Run(c);
                logger.LogInformation("Db created");
            }
            var db = R.Db(dbName);
            return db;
        }


        private void SubscribeToChanges(Connection conn, IConfiguration config, ILogger logger)
        {
            changeFeed = new RethinkChangeFeed(conn, new PushNotificationEventRunner(config, logger))
            {
                // this will come later from the auth 
                LimitWatchToClient = "1337" 
            };

            changeFeed.Subscribe(new List<IRethinkChangeWatcher>()
            {
                new RethinkChangeWatcher() { ExpressionToWatch = documentTable }
            });
        }

        private Connection NewConnection()
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

            var update = new
            {
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
