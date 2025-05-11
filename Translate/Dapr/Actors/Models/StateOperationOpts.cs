
namespace Actors.Models;

public class StateOperationOpts
{
    public Dictionary<string, string> Metadata { get; set; }
    public string ContentType { get; set; }
    public bool StateTTLEnabled { get; set; }
}
