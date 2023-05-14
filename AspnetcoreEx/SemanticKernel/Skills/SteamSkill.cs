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
}
