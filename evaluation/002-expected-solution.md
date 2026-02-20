### What We're Looking For

| Area | What We Observe |
|---|---|
| **Code reading** | Can you trace execution flow? Do you read existing code before changing it? |
| **Architecture understanding** | Can you explain why the code is structured this way? |
| **Design communication** | Can you draw and explain your approach before coding? |
| **Implementation** | Does your code follow existing patterns? Is it clean and correct? |
| **Testing** | Do you write tests? Do you cover edge cases? |
| **AI tool usage** | Do you use AI effectively? Do you review and understand its output? |
| **Problem solving** | How do you handle errors or unexpected behavior? |

---


## Architecture

```
Azurite Blob Storage
  input/  ──────────────────────────┐
                                    │
                         BlobPollingWorker
                         (polls every 5s)
                                    │
                        TransformationPipeline
                                    │
                    ┌───────────────┼───────────────┐
                    │               │               │
              XmlParserService  OrderValidator  FieldMapping
              (XML → Model)    Service          Service
                    │          (validate)       (map values)
                    │               │               │
                    └───────────────┼───────────────┘
                                    │
                        JsonTransformerService
                        (Model → JSON)
                                    │
  output/ ◄─────────────────────────┘
  processed/ ◄── (input file moved here after success)
  failed/    ◄── (input file moved here on error)
```

## Data Flow

1. XML order files are placed in the `input/` folder of the blob storage container
2. `BlobPollingWorker` detects new files every 5 seconds
3. `XmlParserService` parses the XML into `OrderBatch` domain model
4. `OrderValidatorService` validates all fields and collects errors
5. `FieldMappingService` maps field values (codes → human-readable names)
6. `JsonTransformerService` serializes the model to JSON (including any validation errors)
7. JSON output is written to the `output/` folder
8. Processed XML files are moved to `processed/`

## Domain Model

```
OrderBatch
├── TenantId
└── Orders[]
    ├── Header (OrderId, OrderDate, Status)
    ├── Customer (CustomerId, Name, Email, Address)
    ├── Items[] (LineNumber, ProductCode, Description, Quantity, UnitPrice, Currency)
    └── Totals (Subtotal, TaxRate, TaxAmount, Total, Currency)
```

## Project Structure

```
src/OrderTransformer/
├── Program.cs                          # Entry point, DI registration
├── Models/
│   ├── OrderModels.cs                  # Domain model records
│   ├── ValidationError.cs             # Validation error record
│   └── TransformationResult.cs        # Pipeline result record
├── Services/
│   ├── IBlobStorageService.cs         # Blob read/write/move interface
│   ├── BlobStorageService.cs          # Azure.Storage.Blobs implementation
│   ├── IXmlParserService.cs           # XML parsing interface
│   ├── XmlParserService.cs            # System.Xml.Linq implementation
│   ├── IJsonTransformerService.cs     # JSON output interface
│   ├── JsonTransformerService.cs      # System.Text.Json implementation
│   ├── IOrderValidatorService.cs      # Validation interface
│   ├── OrderValidatorService.cs       # Validation implementation
│   ├── IFieldMappingService.cs        # Field mapping interface
│   └── FieldMappingService.cs         # Mapping implementation
└── Worker/
    ├── BlobPollingWorker.cs           # BackgroundService polling blob storage
    └── TransformationPipeline.cs      # Orchestrates the processing steps

tests/OrderTransformer.Tests/
├── Services/
│   ├── XmlParserServiceTests.cs       # XML parsing tests
│   ├── JsonTransformerServiceTests.cs # JSON output tests
│   ├── OrderValidatorServiceTests.cs  # Validation tests
│   └── FieldMappingServiceTests.cs    # Mapping tests
├── Worker/
│   └── TransformationPipelineTests.cs # Pipeline orchestration tests
└── TestData/
    ├── valid-order.xml                # Valid test data
    └── invalid-order.xml              # Invalid test data
```



# Expected Solution: 002 — Order Date Validation

**For interviewer use only. Do not share with candidates.**

## Overview

The candidate must add two validation checks to `OrderValidatorService.cs`:
1. **FORMAT check** — `OrderDate` must be a parseable ISO 8601 date/time
2. **RANGE check** — `OrderDate` must not be in the future

They also need to add unit tests to `OrderValidatorServiceTests.cs`.

---

## What the Candidate Should Discover (Code Reading)

Before coding, the candidate should:

1. **Find the validator** — `src/OrderTransformer/Services/OrderValidatorService.cs`
2. **Observe the pattern** — Format validations only run when the field is non-empty (using `!string.IsNullOrWhiteSpace(...)` guard). This means empty `OrderDate` produces only a `REQUIRED` error, not a duplicate `FORMAT` error.
3. **Understand ValidationError** — Uses `ErrorCode` values: `"REQUIRED"`, `"FORMAT"`, `"RANGE"`
4. **Find existing tests** — `tests/OrderTransformer.Tests/Services/OrderValidatorServiceTests.cs` — uses `CreateValidBatch()` helper, `[Theory]`/`[InlineData]` for parameterized tests

---

## Correct Implementation

### Changes to `OrderValidatorService.cs`

Add the following block **after** the existing format validations (after the Country code check, before the Item validations loop), following the same guard pattern:

```csharp
// OrderDate format and range validation
if (!string.IsNullOrWhiteSpace(order.Header.OrderDate))
{
    if (!DateTimeOffset.TryParse(order.Header.OrderDate, out var parsedDate))
    {
        errors.Add(new ValidationError
        {
            OrderId = orderId,
            Field = "Header.OrderDate",
            Message = "OrderDate must be a valid ISO 8601 date/time",
            ErrorCode = "FORMAT"
        });
    }
    else if (parsedDate > DateTimeOffset.UtcNow)
    {
        errors.Add(new ValidationError
        {
            OrderId = orderId,
            Field = "Header.OrderDate",
            Message = "OrderDate must not be in the future",
            ErrorCode = "RANGE"
        });
    }
}
```

**Key decisions the candidate should make:**

| Decision | Correct Choice | Why |
|----------|---------------|-----|
| Guard against empty | `!string.IsNullOrWhiteSpace` check | Follows existing pattern; avoids duplicate error with REQUIRED |
| Parse method | `DateTimeOffset.TryParse` | Safe parsing, no exception on bad input. `DateTime.TryParse` is also acceptable |
| Check order | Parse first, then range | Can't check if future-dated if it doesn't parse |
| `else if` vs. separate `if` | `else if` for range | Only check range if format is valid — avoids meaningless range error on garbage |
| Error codes | `"FORMAT"` and `"RANGE"` | Matches existing error code conventions |
| Future check | `parsedDate > DateTimeOffset.UtcNow` | UTC comparison avoids timezone issues |

### What the Full Validate Method Should Look Like (relevant section)

```csharp
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

// --- NEW: OrderDate format and range validation ---
if (!string.IsNullOrWhiteSpace(order.Header.OrderDate))
{
    if (!DateTimeOffset.TryParse(order.Header.OrderDate, out var parsedDate))
    {
        errors.Add(new ValidationError
        {
            OrderId = orderId,
            Field = "Header.OrderDate",
            Message = "OrderDate must be a valid ISO 8601 date/time",
            ErrorCode = "FORMAT"
        });
    }
    else if (parsedDate > DateTimeOffset.UtcNow)
    {
        errors.Add(new ValidationError
        {
            OrderId = orderId,
            Field = "Header.OrderDate",
            Message = "OrderDate must not be in the future",
            ErrorCode = "RANGE"
        });
    }
}
// --- END NEW ---

// Item validations
for (var i = 0; i < order.Items.Count; i++)
```

---

## Expected Tests

### Changes to `OrderValidatorServiceTests.cs`

Add the following test methods. The candidate should follow the existing pattern of using `CreateValidBatch()` and `with` expressions.

```csharp
// --- Format validations: OrderDate ---

[Theory]
[InlineData("yesterday")]
[InlineData("not-a-date")]
[InlineData("2024-13-45T00:00:00Z")]
[InlineData("32/01/2024")]
public void Validate_InvalidOrderDateFormat_ReturnsFormatError(string orderDate)
{
    var batch = CreateValidBatch();
    var order = batch.Orders[0];
    batch = batch with
    {
        Orders = new List<Order>
        {
            order with { Header = order.Header with { OrderDate = orderDate } }
        }
    };

    var errors = _sut.Validate(batch);

    Assert.Contains(errors, e => e.Field == "Header.OrderDate" && e.ErrorCode == "FORMAT");
}

[Fact]
public void Validate_ValidOrderDate_ReturnsNoDateErrors()
{
    var batch = CreateValidBatch();
    // CreateValidBatch already has "2024-01-15T10:30:00Z" which is valid

    var errors = _sut.Validate(batch);

    Assert.DoesNotContain(errors, e => e.Field == "Header.OrderDate");
}

// --- Range validations: OrderDate ---

[Fact]
public void Validate_FutureOrderDate_ReturnsRangeError()
{
    var batch = CreateValidBatch();
    var order = batch.Orders[0];
    var futureDate = DateTimeOffset.UtcNow.AddYears(5).ToString("O");
    batch = batch with
    {
        Orders = new List<Order>
        {
            order with { Header = order.Header with { OrderDate = futureDate } }
        }
    };

    var errors = _sut.Validate(batch);

    Assert.Contains(errors, e => e.Field == "Header.OrderDate" && e.ErrorCode == "RANGE");
}

[Fact]
public void Validate_EmptyOrderDate_ReturnsOnlyRequiredError()
{
    var batch = CreateValidBatch();
    var order = batch.Orders[0];
    batch = batch with
    {
        Orders = new List<Order>
        {
            order with { Header = order.Header with { OrderDate = "" } }
        }
    };

    var errors = _sut.Validate(batch);

    Assert.Contains(errors, e => e.Field == "Header.OrderDate" && e.ErrorCode == "REQUIRED");
    Assert.DoesNotContain(errors, e => e.Field == "Header.OrderDate" && e.ErrorCode == "FORMAT");
}
```

**Minimum test count: 4 tests** (format theory with multiple cases, valid date, future date, empty date only-required)

---

## Scoring Guide for Feature 002

### What to Look For

| Area | Excellent | Acceptable | Red Flag |
|------|-----------|------------|----------|
| **Code reading** | Finds `OrderValidatorService.cs` quickly, reads existing validation pattern before writing | Takes some time but finds it | Asks where to put it, or modifies the wrong file |
| **Guard pattern** | Uses `!string.IsNullOrWhiteSpace` guard like existing validations | Implements guard but slightly differently | No guard — FORMAT error fires on empty string alongside REQUIRED |
| **Parse approach** | `DateTimeOffset.TryParse` or `DateTime.TryParse` — no exceptions | Uses try/catch around `DateTime.Parse` | Regex-only date validation (fragile, incomplete) |
| **else if for range** | Only checks future if parse succeeded (else if) | Separate if with parsed flag variable | Checks future date even when parse failed |
| **Future date test** | Uses `DateTimeOffset.UtcNow.AddYears(...)` or similar dynamic date | Hard-codes a far-future date like "2099-..." | No future date test |
| **Empty date test** | Explicitly asserts no FORMAT error when field is empty | Tests empty produces REQUIRED but doesn't check for absence of FORMAT | No test for the empty case |

### Common Mistakes to Watch For

1. **Duplicate error on empty** — Not guarding with `IsNullOrWhiteSpace`, so empty `OrderDate` gets both `REQUIRED` and `FORMAT` errors
2. **Using regex for date validation** — A regex like `^\d{4}-\d{2}-\d{2}` will match `2024-13-45` (invalid month/day). `TryParse` correctly rejects this.
3. **Not using `else if`** — Checking range independently of format means a garbage string could produce both FORMAT and RANGE errors
4. **Hard-coded future date in tests** — Using `"2099-01-01T00:00:00Z"` works today but is fragile. Using `UtcNow.AddYears(5)` is more robust. (Accept either approach, but note which they chose.)
5. **Using `DateTime` instead of `DateTimeOffset`** — Acceptable, but `DateTimeOffset` is better for UTC comparison. Don't penalize for `DateTime.TryParse`.
6. **Not running existing tests** — Candidate should run `dotnet test` before and after to verify no regressions

### Test Run Expectations

After correct implementation:

```
dotnet test tests/OrderTransformer.Tests --verbosity normal
```

- All 16 existing tests should still pass
- 4+ new tests should pass
- Total: **20+ tests passing**, 0 failures

---

## Acceptance Criteria Checklist

- [ ] Unparseable `OrderDate` produces a `FORMAT` validation error
- [ ] Future-dated `OrderDate` produces a `RANGE` validation error
- [ ] Valid past/present dates pass validation without errors
- [ ] Empty `OrderDate` still produces only a `REQUIRED` error (no duplicate `FORMAT` error)
- [ ] Existing validation rules are unchanged
- [ ] Unit tests cover valid dates, invalid formats, and future dates
- [ ] All existing tests still pass
- [ ] Application builds and runs successfully
