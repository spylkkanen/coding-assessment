using OrderTransformer.Models;

namespace OrderTransformer.Services;

public interface IFieldMappingService
{
    OrderBatch MapFields(OrderBatch batch);
}
