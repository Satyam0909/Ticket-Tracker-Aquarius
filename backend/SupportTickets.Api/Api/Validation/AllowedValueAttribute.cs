using System.ComponentModel.DataAnnotations;

namespace SupportTickets.Api.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class AllowedValueAttribute(params string[] values) : ValidationAttribute
{
    private readonly HashSet<string> allowedValues = new(values, StringComparer.Ordinal);

    public override bool IsValid(object? value) =>
        value is null || value is string text && allowedValues.Contains(text);

    public override string FormatErrorMessage(string name) =>
        $"{name} must be one of: {string.Join(", ", allowedValues)}.";
}
