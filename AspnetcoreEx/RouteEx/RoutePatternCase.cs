using Microsoft.AspNetCore.Routing.Patterns;
using System.Text;

namespace AspnetcoreEx.RouteEx;

public class RoutePatternCase
{
    public static string Format(RoutePattern pattern)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"RawText:{pattern.RawText}");
        builder.AppendLine($"InboundPrecedence:{pattern.InboundPrecedence}");
        builder.AppendLine($"OutboundPrecedence:{pattern.OutboundPrecedence}");

        var segments = pattern.PathSegments;
        builder.AppendLine("Segments");
        foreach (var segment in segments)
        {
            foreach (var part in segment.Parts)
            {
                builder.AppendLine($"\t{ToString(part)}");
            }
        }
        builder.AppendLine("Defaults");
        foreach (var @default in pattern.Defaults)
        {
            builder.AppendLine($"\t{@default.Key} = {@default.Value}");
        }
        builder.AppendLine("ParameterPolicies ");
        foreach (var policy in pattern.ParameterPolicies)
        {
            builder.AppendLine($"\t{policy.Key} = {string.Join(',', policy.Value.Select(it => it.Content))}");
        }

        builder.AppendLine("RequiredValues ");
        foreach (var required in pattern.RequiredValues)
        {
            builder.AppendLine($"\t{required.Key} = {required.Value}");
        }

        return builder.ToString();

        string ToString(RoutePatternPart part)
        {
            return part switch
            {
                RoutePatternLiteralPart literal => $"Literal: {literal.Content}",
                RoutePatternSeparatorPart separator => $"Separator: {separator.Content}",
                RoutePatternParameterPart parameter => $"Parameter: Name={parameter.Name}",
                _ => throw new ArgumentException("Invalid RoutePatternPart.")
            };
        }
    }


}