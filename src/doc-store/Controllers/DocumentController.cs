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
        /// <param name="document">the document to add</param>
        /// <returns></returns>
        [HttpPost]
        public DocumentAddResult Post([FromBody]Document document)
        {
            this.logger.LogInformation("document received");
            this.logger.LogInformation("Adding document to store");
            var result = this.store.AddDocument(document);

            return result;
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
