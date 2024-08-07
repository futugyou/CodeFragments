﻿using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using System.Xml;
using System.Xml.XPath;

namespace AspnetcoreEx.SemanticKernel;

public static class XmlMarkupPlanParser
{
    private static readonly Dictionary<string, KeyValuePair<string, string>> s_skillMapping = new()
    {
        { "lookup", new KeyValuePair<string, string>("google", "SearchAsync") },
    };

    public static Plan FromMarkup(this string markup, string goal, SKContext context)
    {
        Console.WriteLine("Markup:");
        Console.WriteLine(markup);
        Console.WriteLine();

        var doc = new XmlMarkup(markup);
        var nodes = doc.SelectElements();
        return nodes.Count == 0 ? new Plan(goal) : NodeListToPlan(nodes, context, goal);
    }

    private static Plan NodeListToPlan(XmlNodeList nodes, SKContext context, string description)
    {
        Plan plan = new(description);
        for (var i = 0; i < nodes.Count; ++i)
        {
            var node = nodes[i];
            var functionName = node!.LocalName;
            var skillName = string.Empty;

            if (s_skillMapping.TryGetValue(node!.LocalName, out KeyValuePair<string, string> value))
            {
                functionName = value.Value;
                skillName = value.Key;
            }

            var hasChildElements = node.HasChildElements();

            if (hasChildElements)
            {
                plan.AddSteps(NodeListToPlan(node.ChildNodes, context, functionName));
            }
            else
            {
                if (string.IsNullOrEmpty(skillName)
                        ? !context.Skills!.TryGetFunction(functionName, out var _)
                        : !context.Skills!.TryGetFunction(skillName, functionName, out var _))
                {
                    var planStep = new Plan(node.InnerText);
                    planStep.Parameters.Update(node.InnerText);
                    planStep.Outputs.Add($"markup.{functionName}.result");
                    plan.Outputs.Add($"markup.{functionName}.result");
                    plan.AddSteps(planStep);
                }
                else
                {
                    var command = string.IsNullOrEmpty(skillName)
                        ? context.Skills.GetFunction(functionName)
                        : context.Skills.GetFunction(skillName, functionName);
                    var planStep = new Plan(command);
                    planStep.Parameters.Update(node.InnerText);
                    planStep.Outputs.Add($"markup.{functionName}.result");
                    plan.Outputs.Add($"markup.{functionName}.result");
                    plan.AddSteps(planStep);
                }
            }
        }

        return plan;
    }
}


public class XmlMarkup
{
    public XmlMarkup(string response, string? wrapperTag = null)
    {
        if (!string.IsNullOrEmpty(wrapperTag))
        {
            response = $"<{wrapperTag}>{response}</{wrapperTag}>";
        }

        this.Document = new XmlDocument();
        this.Document.LoadXml(response);
    }

    public XmlDocument Document { get; }

    public XmlNodeList SelectAllElements()
    {
        return this.Document.SelectNodes("//*")!;
    }

    public XmlNodeList SelectElements()
    {
        return this.Document.SelectNodes("/*")!;
    }
}

#pragma warning disable CA1815 // Override equals and operator equals on value types
public struct XmlNodeInfo
{
    public int StackDepth { get; set; }
    public XmlNode Parent { get; set; }
    public XmlNode Node { get; set; }

    public static implicit operator XmlNode(XmlNodeInfo info)
    {
        return info.Node;
    }
}
#pragma warning restore CA1815

#pragma warning disable CA1711
public static class XmlEx
{
    public static bool HasChildElements(this XmlNode elt)
    {
        if (!elt.HasChildNodes)
        {
            return false;
        }

        var childNodes = elt.ChildNodes;
        for (int i = 0, count = childNodes.Count; i < count; ++i)
        {
            if (childNodes[i]?.NodeType == XmlNodeType.Element)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Walks the Markup DOM using an XPathNavigator, allowing recursive descent WITHOUT requiring a Stack Hit
    ///     This is safe for very large and highly nested documents.
    /// </summary>
    public static IEnumerable<XmlNodeInfo> EnumerateNodes(this XmlNode node, int maxStackDepth = 32)
    {
        var nav = node.CreateNavigator();
        return EnumerateNodes(nav!, maxStackDepth);
    }

    public static IEnumerable<XmlNodeInfo> EnumerateNodes(this XmlDocument doc, int maxStackDepth = 32)
    {
        var nav = doc.CreateNavigator();
        nav!.MoveToRoot();
        return EnumerateNodes(nav, maxStackDepth);
    }

    public static IEnumerable<XmlNodeInfo> EnumerateNodes(this XPathNavigator nav, int maxStackDepth = 32)
    {
        var info = new XmlNodeInfo
        {
            StackDepth = 0
        };
        var hasChildren = nav.HasChildren;
        while (true)
        {
            info.Parent = (XmlNode)nav.UnderlyingObject!;
            if (hasChildren && info.StackDepth < maxStackDepth)
            {
                nav.MoveToFirstChild();
                info.StackDepth++;
            }
            else
            {
                var hasParent = false;
                while (hasParent = nav.MoveToParent())
                {
                    info.StackDepth--;
                    if (info.StackDepth == 0)
                    {
                        hasParent = false;
                        break;
                    }

                    if (nav.MoveToNext())
                    {
                        break;
                    }
                }

                if (!hasParent)
                {
                    break;
                }
            }

            do
            {
                info.Node = (XmlNode)nav.UnderlyingObject!;
                yield return info;
                if (hasChildren = nav.HasChildren)
                {
                    break;
                }
            } while (nav.MoveToNext());
        }
    }
}