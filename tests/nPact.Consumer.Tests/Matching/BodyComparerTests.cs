using System;
using FluentAssertions;
using Moq;
using nPact.Common.Contracts;
using nPact.Consumer.Contracts;
using nPact.Consumer.Matching;
using nPact.Consumer.Rendering;
using Xunit;

namespace nPact.Consumer.Tests.Matching
{
    public class BodyComparerTests
    {
        private readonly Mock<IConsumerConfiguration> _configurationMock;

        public BodyComparerTests()
        {
            _configurationMock = new Mock<IConsumerConfiguration>();
            _configurationMock.Setup(x => x.BodyKeyStringComparison)
                .Returns(StringComparison.CurrentCultureIgnoreCase);
        }

        [Fact]
        public void GivingTwoEqualsANestedObjects_WhenMatching_ShouldReturnTrue()
        {
            var expectedRequest = GetRequestDefinition(new {object1 = new {prop1 = "value1"}});
            var actualRequest = GetRequestDefinition(new {object1 = new {prop1 = "value1"}});

            var bodyComparer = new BodyComparer(expectedRequest, _configurationMock.Object);

            bodyComparer.Matches(actualRequest).Should().BeTrue();
        }

        [Fact]
        public void GivingTwoEqualsANestedObjects_WhenDiffing_ShouldReturnNull()
        {
            var expectedRequest = GetRequestDefinition(new {object1 = new {prop1 = "value1"}});
            var actualRequest = GetRequestDefinition(new {object1 = new {prop1 = "value1"}});

            var bodyComparer = new BodyComparer(expectedRequest, _configurationMock.Object);

            bodyComparer.DiffGram(actualRequest).Should().BeNull();
        }
        
        [Fact]
        public void GivinAMissingNestedPropertyObject_WhenMatching_ShouldReturnFalse()
        {
            var expectedRequest = GetRequestDefinition(new {object1 = new {prop1 = "value1"}});
            var actualRequest = GetRequestDefinition(new {object1 = new {prop2 = "value2"}});

            var bodyComparer = new BodyComparer(expectedRequest, _configurationMock.Object);

            bodyComparer.Matches(actualRequest).Should().BeFalse();
        }
        
        [Fact]
        public void GivinAMissingNestedPropertyObject_WhenDiffing_ShouldReturnDifferenceDescription()
        {
            var expectedRequest = GetRequestDefinition(new {object1 = new {prop1 = "value1"}});
            var actualRequest = GetRequestDefinition(new {object1 = new {otherStuff = "otherValue"}});

            var bodyComparer = new BodyComparer(expectedRequest, _configurationMock.Object);

            bodyComparer.DiffGram(actualRequest)
                .Should().ContainKey("object1.prop1")
                .WhichValue?.ToString()
                .Should().BeEquivalentTo(@"{
  ""expected"": ""value1"",
  ""actual"": ""<Member missing>""
}");
        }

        [Fact]
        public void GivinAMissingPropertyObject_WhenDiffing_ShouldReturnDifferenceDescription()
        {
            var expectedRequest = GetRequestDefinition(new {object1 = new {prop1 = "value1", prop2 = "value2"}});
            var actualRequest = GetRequestDefinition(new {object2 = new {otherStuff = "otherValue"}});

            var bodyComparer = new BodyComparer(expectedRequest, _configurationMock.Object);

            bodyComparer.DiffGram(actualRequest)
                .Should().ContainKey("object1")
                .WhichValue?.ToString()
                .Should().BeEquivalentTo(@"{
  ""expected"": ""{\n  \""prop1\"": \""value1\"",\n  \""prop2\"": \""value2\""\n}"",
  ""actual"": ""<Member missing>""
}");
        }

        [Fact]
        public void GivingObjectsWithDifferentValues_WhenMatching_ShouldReturnFalse()
        {
            var expectedRequest = GetRequestDefinition(new {object1 = new {prop1 = "value-expected"}});
            var actualRequest = GetRequestDefinition(new {object1 = new {prop1 = "value-actual"}});

            var bodyComparer = new BodyComparer(expectedRequest, _configurationMock.Object);

            bodyComparer.Matches(actualRequest).Should().BeFalse();
        }

        [Fact]
        public void GivingObjectsWithDifferentValues_WhenDiffing_ShouldReturnDifferenceDescription()
        {
            var expectedRequest = GetRequestDefinition(new {object1 = new {prop1 = "value-expected"}});
            var actualRequest = GetRequestDefinition(new {object1 = new {prop1 = "value-actual"}});

            var bodyComparer = new BodyComparer(expectedRequest, _configurationMock.Object);

            bodyComparer.DiffGram(actualRequest)
                .Should().ContainKey("object1.prop1")
                .WhichValue?.ToString()
                .Should().BeEquivalentTo(@"{
  ""expected"": ""value-expected"",
  ""actual"": ""value-actual""
}");
        }

        private static IPactRequestDefinition GetRequestDefinition(object requestBody)
        {
            var requestMock = new Mock<IPactRequestDefinition>();
            requestMock.SetupGet(x => x.RequestBody)
                .Returns(new JsonBody(requestBody));

            return requestMock.Object;
        }
    }
}