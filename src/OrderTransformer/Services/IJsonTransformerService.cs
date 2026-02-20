using OrderTransformer.Models;

namespace OrderTransformer.Services;

public interface IJsonTransformerService
{
    string Transform(OrderBatch batch, List<ValidationError> validationErrors);
}
