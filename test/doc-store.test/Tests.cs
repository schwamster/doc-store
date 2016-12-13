using System;
using Xunit;
using doc_store.Store;
using Moq;
using FluentAssertions;
using doc_store.Store.StoreChangeFeed;

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void GivenDocStore_AddDocument() 
        {
            var store = new Mock<IDocumentStore>();
    
            store.Setup(x => x.AddDocument(It.IsAny<Document>()))
            .Returns(new DocumentAddResult());

            var result = store.Object.AddDocument(new Document());

            result.Should().NotBeNull();            
        }

        public void GivenRethinkDbStore_AndChangeFeed_RaiseEventOnInsert()
        {
            var store = new Mock<IRethinkStore>();
            var changeFeed = new Mock<IStoreChangeFeed>();
            var eventRunner = new Mock<IActionOnEventRunner>();
            store.Setup(t => t.changeFeed).Returns(changeFeed.Object);
            store.Setup(t => t.eventRunner).Returns(eventRunner.Object);

            // complete tests


        }
    }
}
