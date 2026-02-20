using OrderTransformer.Services;
using OrderTransformer.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Register services
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
builder.Services.AddSingleton<IXmlParserService, XmlParserService>();
builder.Services.AddSingleton<IJsonTransformerService, JsonTransformerService>();
builder.Services.AddSingleton<IOrderValidatorService, OrderValidatorService>();
builder.Services.AddSingleton<IFieldMappingService, FieldMappingService>();
builder.Services.AddSingleton<TransformationPipeline>();

// Register background worker
builder.Services.AddHostedService<BlobPollingWorker>();

var host = builder.Build();
host.Run();
