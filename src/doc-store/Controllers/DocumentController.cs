using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using doc_store.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace doc_store.Controllers
{

    [Route("api/[controller]")]
    public class DocumentController : Controller
    {
        private IConfiguration configuration;
        private ILogger logger;
        private IDocumentStore store;

        public DocumentController(IConfiguration configuration, ILogger<DocumentController> logger, IDocumentStore store)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.store = store;
        }

        // GET api/values
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        /// <summary>
        /// Getting a specific document by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public StoreDocument Get(Guid id)
        {
            var result = this.store.GetDocument(id);
            return result;
        }

        /// <summary>
        /// Add a document
        /// </summary>
        /// <param name="name">name of the document</param>
        /// <param name="user">name of the user adding the document</param>
        /// <param name="client">name of the client adding the document</param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public DocumentAddResult Post(string user, string client, id string, ICollection<IFormFile> file)
        {
            this.logger.LogInformation("document received");
            var document = new Document()
            {
                Client = client,
                User = user,
                Id = id
            };

            var f = file.First();
            if (f.Length > 0)
            {
                document.Name = f.FileName;
                var bytes = ConvertToBytes(f);
                document.Content = System.Convert.ToBase64String(bytes);
                this.logger.LogInformation("Adding document to store");
            }

            var result = this.store.AddDocument(document);

            return result;
        }

        internal static byte[] ConvertToBytes(IFormFile image)
        {
            byte[] bytes = null;
            BinaryReader reader = new BinaryReader(image.OpenReadStream());
            bytes = reader.ReadBytes((int)image.Length);
            return bytes;
        }

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
