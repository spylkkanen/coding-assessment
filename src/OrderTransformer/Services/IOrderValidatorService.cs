using OrderTransformer.Models;

namespace OrderTransformer.Services;

public interface IOrderValidatorService
{
    List<ValidationError> Validate(OrderBatch batch);
}
