using System;
using Xunit;
using doc_store.Store;
using Moq;
using FluentAssertions;
using doc_store.Store.StoreChangeFeed;
using RethinkDb.Driver.Ast;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

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

        //discuss with Bastian whats meaningful to test 
    }
}
