using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using doc_store.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNet.JsonPatch;

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
            //this parsing could be a dotnetcore Middleware, that takes the encyrpted token from the httprequest, sends it to an Identity-API/verifytoken endpoint, then the identity API will check the JWT
            //according to some secret that all APIs have access to (shared path, docker --volumes-from ) and if its verified ok, responds with OK then 
            //the middleware in this API lets the request pass through to this controller.
            var parsedAuthorizationBearerToken = new
            {
                username = "Jenny",
                role= "admin",
                client = "1337"
            };
            document.Client = parsedAuthorizationBearerToken.client;

            this.logger.LogInformation("document received");
            this.logger.LogInformation("Adding document to store");
            var result = this.store.AddDocument(document);

            return result;
        }

        /// <summary>
        /// Right now you can only update the property extractedText
        /// </summary>
        /// <remarks>
        /// Note that the key is a GUID and not an integer.
        ///  
        ///     PATCH /api/document/{id}
        ///     [
        ///         {
        ///             "value": "das ist der text",
        ///             "path": "/extractedText",
        ///             "op": "add",
        ///             "from": "string"
        ///         }
        ///     ]
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="patch"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        public IActionResult Patch(Guid id, [FromBody]JsonPatchDocument<Document> patch)
        {
            var document = new Document(); 
            patch.ApplyTo(document);

            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            var model = new
            {
                patch
            };

            this.store.AddExtractedText(id, document.ExtractedText);

            return Ok(model);
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
