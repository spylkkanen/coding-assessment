using OrderTransformer.Models;

namespace OrderTransformer.Services;

public interface IXmlParserService
{
    OrderBatch Parse(string xml);
}
