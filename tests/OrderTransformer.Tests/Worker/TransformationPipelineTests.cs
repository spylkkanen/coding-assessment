using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderTransformer.Models;
using OrderTransformer.Services;
using OrderTransformer.Worker;
using Xunit;

namespace OrderTransformer.Tests.Worker;

public class TransformationPipelineTests
{
    private readonly Mock<IXmlParserService> _parserMock = new();
    private readonly Mock<IOrderValidatorService> _validatorMock = new();
    private readonly Mock<IFieldMappingService> _mapperMock = new();
    private readonly Mock<IJsonTransformerService> _transformerMock = new();
    private readonly TransformationPipeline _sut;

    private static readonly OrderBatch TestBatch = new()
    {
        TenantId = "test-tenant",
        Orders = new List<Order>
        {
            new()
            {
                Header = new OrderHeader { OrderId = "ORD-2024-001234", OrderDate = "2024-01-15", Status = "confirmed" },
                Customer = new Customer { CustomerId = "CUST-001", Name = "Test", Email = "test@test.com", Address = new Address() },
                Items = new List<OrderItem>(),
                Totals = new OrderTotals()
            }
        }
    };

    public TransformationPipelineTests()
    {
        _sut = new TransformationPipeline(
            _parserMock.Object,
            _validatorMock.Object,
            _mapperMock.Object,
            _transformerMock.Object,
            NullLogger<TransformationPipeline>.Instance);
    }

    [Fact]
    public void Process_ValidXml_ReturnsSuccessResult()
    {
        _parserMock.Setup(p => p.Parse(It.IsAny<string>())).Returns(TestBatch);
        _validatorMock.Setup(v => v.Validate(It.IsAny<OrderBatch>())).Returns(new List<ValidationError>());
        _mapperMock.Setup(m => m.MapFields(It.IsAny<OrderBatch>())).Returns(TestBatch);
        _transformerMock.Setup(t => t.Transform(It.IsAny<OrderBatch>(), It.IsAny<List<ValidationError>>())).Returns("{}");

        var result = _sut.Process("<xml/>", "test-blob.xml");

        Assert.True(result.Success);
        Assert.Equal("{}", result.Json);
        Assert.Empty(result.ValidationErrors);
    }

    [Fact]
    public void Process_WithValidationErrors_StillReturnsSuccess()
    {
        var errors = new List<ValidationError>
        {
            new() { OrderId = "ORD-2024-001234", Field = "Email", Message = "Invalid", ErrorCode = "FORMAT" }
        };

        _parserMock.Setup(p => p.Parse(It.IsAny<string>())).Returns(TestBatch);
        _validatorMock.Setup(v => v.Validate(It.IsAny<OrderBatch>())).Returns(errors);
        _mapperMock.Setup(m => m.MapFields(It.IsAny<OrderBatch>())).Returns(TestBatch);
        _transformerMock.Setup(t => t.Transform(It.IsAny<OrderBatch>(), errors)).Returns("{\"errors\":1}");

        var result = _sut.Process("<xml/>", "test-blob.xml");

        Assert.True(result.Success);
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public void Process_CallsServicesInCorrectOrder()
    {
        var callOrder = new List<string>();

        _parserMock.Setup(p => p.Parse(It.IsAny<string>()))
            .Callback(() => callOrder.Add("parse"))
            .Returns(TestBatch);
        _validatorMock.Setup(v => v.Validate(It.IsAny<OrderBatch>()))
            .Callback(() => callOrder.Add("validate"))
            .Returns(new List<ValidationError>());
        _mapperMock.Setup(m => m.MapFields(It.IsAny<OrderBatch>()))
            .Callback(() => callOrder.Add("map"))
            .Returns(TestBatch);
        _transformerMock.Setup(t => t.Transform(It.IsAny<OrderBatch>(), It.IsAny<List<ValidationError>>()))
            .Callback(() => callOrder.Add("transform"))
            .Returns("{}");

        _sut.Process("<xml/>", "test-blob.xml");

        Assert.Equal(new[] { "parse", "validate", "map", "transform" }, callOrder);
    }

    [Fact]
    public void Process_ParseFailure_ReturnsFailureResult()
    {
        _parserMock.Setup(p => p.Parse(It.IsAny<string>())).Throws(new Exception("Parse error"));

        var result = _sut.Process("bad xml", "test-blob.xml");

        Assert.False(result.Success);
        Assert.Equal("Parse error", result.ErrorMessage);
    }
}
