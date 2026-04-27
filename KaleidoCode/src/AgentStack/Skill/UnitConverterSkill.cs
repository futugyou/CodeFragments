
namespace AgentStack.Skill;

public sealed class UnitConverterSkill : AgentClassSkill<UnitConverterSkill>
{
    /// <inheritdoc/>
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "unit-converter",
        "Convert between common units using a multiplication factor. Use when asked to convert miles, kilometers, pounds, or kilograms.");

    /// <inheritdoc/>
    protected override string Instructions => """
        Use this skill when the user asks to convert between units.

        1. Review the conversion-table resource to find the factor for the requested conversion.
        2. Use the convert script, passing the value and factor from the table.
        3. Present the result clearly with both units.
        """;

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> used to marshal parameters and return values
    /// for scripts and resources.
    /// </summary>
    /// <remarks>
    /// This override is not necessary for this sample, but can be used to provide custom
    /// serialization options, for example a source-generated <c>JsonTypeInfoResolver</c>
    /// for Native AOT compatibility.
    /// </remarks>
    protected override JsonSerializerOptions? SerializerOptions => null;

    /// <summary>
    /// A conversion table resource providing multiplication factors.
    /// </summary>
    [AgentSkillResource("conversion-table")]
    [Description("Lookup table of multiplication factors for common unit conversions.")]
    public string ConversionTable => """
        # Conversion Tables

        Formula: **result = value × factor**

        | From        | To          | Factor   |
        |-------------|-------------|----------|
        | miles       | kilometers  | 1.60934  |
        | kilometers  | miles       | 0.621371 |
        | pounds      | kilograms   | 0.453592 |
        | kilograms   | pounds      | 2.20462  |
        """;

    /// <summary>
    /// Converts a value by the given factor.
    /// </summary>
    [AgentSkillScript("convert")]
    [Description("Multiplies a value by a conversion factor and returns the result as JSON.")]
    private static string ConvertUnits(double value, double factor, IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetRequiredService<ConversionService>();
        return service.Convert(value, factor);
    }
}

public sealed class ConversionService
{
    /// <summary>
    /// Returns a markdown table of supported distance conversions.
    /// </summary>
    public string GetDistanceTable() =>
        """
        # Conversion Tables

        Formula: **result = value × factor**

        | From        | To          | Factor   |
        |-------------|-------------|----------|
        | miles       | kilometers  | 1.60933  |
        | kilometers  | miles       | 0.621371 |
        | pounds      | kilograms   | 0.453592 |
        | kilograms   | pounds      | 2.20463  |
        """;

    /// <summary>
    /// Returns a markdown table of supported weight conversions.
    /// </summary>
    public string GetWeightTable() =>
        """
        # Weight Conversions

        Formula: **result = value × factor**

        | From        | To          | Factor   |
        |-------------|-------------|----------|
        | pounds      | kilograms   | 0.453592 |
        | kilograms   | pounds      | 2.20462  |
        """;

    /// <summary>
    /// Converts a value by the given factor and returns a JSON result.
    /// </summary>
    public string Convert(double value, double factor)
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    }
}