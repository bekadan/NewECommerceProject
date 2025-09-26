using Core.Shared.Primitives;

namespace Product.Domain.ValueObjects;

public sealed class Price : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Price(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Price amount cannot be negative.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency must be provided.", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    #region Operations

    public Price Add(Price other)
    {
        EnsureSameCurrency(other);
        return new Price(Amount + other.Amount, Currency);
    }

    public Price Subtract(Price other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0) throw new InvalidOperationException("Resulting price cannot be negative.");
        return new Price(result, Currency);
    }

    public Price Multiply(decimal factor)
    {
        if (factor < 0) throw new ArgumentException("Factor cannot be negative.", nameof(factor));
        return new Price(Amount * factor, Currency);
    }

    public Price Divide(decimal divisor)
    {
        if (divisor <= 0) throw new ArgumentException("Divisor must be positive.", nameof(divisor));
        return new Price(Amount / divisor, Currency);
    }

    private void EnsureSameCurrency(Price other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Currency mismatch: {Currency} vs {other.Currency}");
    }

    #endregion

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
