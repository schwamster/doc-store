using System;
using Xunit;
using doc_store.Store;
using Moq;
using FluentAssertions; 

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void Test1() 
        {
            var store = new Mock<IDocumentStore>();

            store.Setup(x => x.AddDocument(It.IsAny<Document>()))
            .Returns(new DocumentAddResult());

            var result = store.Object.AddDocument(new Document());

            result.Should().NotBeNull();            
        }
    }
}
