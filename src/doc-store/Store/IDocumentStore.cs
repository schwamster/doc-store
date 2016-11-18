using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace doc_store.Store
{
    public interface IDocumentStore
    {
        DocumentAddResult AddDocument(Document document);
        StoreDocument GetDocument(Guid id);

        StoreDocument AddExtractedText(Guid id, string extractedText);
    }

    /// <summary>
    /// Result you will receive after storing a document. In the happy case you get Result.Success (1) and the Id of the 
    /// document that has been created. In error Cases you will Receive a Result > 1 and an Error Message that describes the
    /// error as good as possible
    /// </summary>
    public class DocumentAddResult
    {
        public Guid DocumentId { get; set; }
        public Result Result { get; set; }
        public string Message { get; set; }
    }

    public enum Result
    {
        Unknown = 0,
        Success = 1,
        Duplicate,
        Failed
    }



}
