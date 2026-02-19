using System.Text.RegularExpressions;
using OrderTransformer.Models;

namespace OrderTransformer.Services;

public class OrderValidatorService : IOrderValidatorService
{
    private static readonly Regex OrderIdPattern = new(@"^ORD-\d{4}-\d{6}$", RegexOptions.Compiled);
    private static readonly Regex EmailPattern = new(@"^[^@]+@[^@]+\.[^@]+$", RegexOptions.Compiled);
    private static readonly Regex CountryCodePattern = new(@"^[A-Z]{2}$", RegexOptions.Compiled);
    private static readonly Regex CurrencyCodePattern = new(@"^[A-Z]{3}$", RegexOptions.Compiled);

    public List<ValidationError> Validate(OrderBatch batch)
    {
        var errors = new List<ValidationError>();

        foreach (var order in batch.Orders)
        {
            var orderId = string.IsNullOrWhiteSpace(order.Header.OrderId) ? "unknown" : order.Header.OrderId;

            // Required fields
            ValidateRequired(errors, orderId, order.Header.OrderId, "Header.OrderId");
            ValidateRequired(errors, orderId, order.Header.OrderDate, "Header.OrderDate");
            ValidateRequired(errors, orderId, order.Customer.CustomerId, "Customer.CustomerId");
            ValidateRequired(errors, orderId, order.Customer.Name, "Customer.Name");
            ValidateRequired(errors, orderId, order.Customer.Email, "Customer.Email");

            // Format validations
            if (!string.IsNullOrWhiteSpace(order.Header.OrderId) && !OrderIdPattern.IsMatch(order.Header.OrderId))
            {
                errors.Add(new ValidationError
                {
                    OrderId = orderId,
                    Field = "Header.OrderId",
                    Message = "OrderId must match pattern ORD-YYYY-NNNNNN",
                    ErrorCode = "FORMAT"
                });
            }

            if (!string.IsNullOrWhiteSpace(order.Customer.Email) && !EmailPattern.IsMatch(order.Customer.Email))
            {
                errors.Add(new ValidationError
                {
                    OrderId = orderId,
                    Field = "Customer.Email",
                    Message = "Email must match pattern [^@]+@[^@]+\\.[^@]+",
                    ErrorCode = "FORMAT"
                });
            }

            if (!string.IsNullOrWhiteSpace(order.Customer.Address.Country) && !CountryCodePattern.IsMatch(order.Customer.Address.Country))
            {
                errors.Add(new ValidationError
                {
                    OrderId = orderId,
                    Field = "Customer.Address.Country",
                    Message = "Country code must be exactly 2 uppercase letters",
                    ErrorCode = "FORMAT"
                });
            }

            // Item validations
            for (var i = 0; i < order.Items.Count; i++)
            {
                var item = order.Items[i];

                if (!string.IsNullOrWhiteSpace(item.Currency) && !CurrencyCodePattern.IsMatch(item.Currency))
                {
                    errors.Add(new ValidationError
                    {
                        OrderId = orderId,
                        Field = $"Items[{i}].Currency",
                        Message = "Currency code must be exactly 3 uppercase letters",
                        ErrorCode = "FORMAT"
                    });
                }

                if (item.Quantity <= 0)
                {
                    errors.Add(new ValidationError
                    {
                        OrderId = orderId,
                        Field = $"Items[{i}].Quantity",
                        Message = "Quantity must be greater than 0",
                        ErrorCode = "RANGE"
                    });
                }

                if (item.UnitPrice < 0)
                {
                    errors.Add(new ValidationError
                    {
                        OrderId = orderId,
                        Field = $"Items[{i}].UnitPrice",
                        Message = "UnitPrice must be greater than or equal to 0",
                        ErrorCode = "RANGE"
                    });
                }
            }

            // Totals range validations
            if (order.Totals.Subtotal < 0)
            {
                errors.Add(new ValidationError
                {
                    OrderId = orderId,
                    Field = "Totals.Subtotal",
                    Message = "Subtotal must be greater than or equal to 0",
                    ErrorCode = "RANGE"
                });
            }

            if (order.Totals.TaxAmount < 0)
            {
                errors.Add(new ValidationError
                {
                    OrderId = orderId,
                    Field = "Totals.TaxAmount",
                    Message = "TaxAmount must be greater than or equal to 0",
                    ErrorCode = "RANGE"
                });
            }

            if (order.Totals.Total < 0)
            {
                errors.Add(new ValidationError
                {
                    OrderId = orderId,
                    Field = "Totals.Total",
                    Message = "Total must be greater than or equal to 0",
                    ErrorCode = "RANGE"
                });
            }
        }

        return errors;
    }

    private static void ValidateRequired(List<ValidationError> errors, string orderId, string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new ValidationError
            {
                OrderId = orderId,
                Field = field,
                Message = $"{field} is required",
                ErrorCode = "REQUIRED"
            });
        }
    }
}
