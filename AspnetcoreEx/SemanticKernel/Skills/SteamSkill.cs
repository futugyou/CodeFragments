using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace AspnetcoreEx.SemanticKernel.Skills;

public class SteamSkill
{
    [SKFunction("Return the first row of a qwerty keyboard")]
    public string Qwerty(string input)
    {
        return "qwertyuiop";
    }

    [SKFunction("Return a string that's duplicated")]
    public string DupDup(string text)
    {
        return text + text;
    }

    [SKFunction("Joins a first and last name together")]
    [SKFunctionContextParameter(Name = "firstname", Description = "Informal name you use")]
    [SKFunctionContextParameter(Name = "lastname", Description = "More formal name you use")]
    public string FullNamer(SKContext context)
    {
        return context["firstname"] + " " + context["lastname"];
    }
}
