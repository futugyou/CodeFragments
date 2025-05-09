
using Config;
using Messaging;

namespace AppChannel;

// AppChannel is an abstraction over communications with user code.
public interface IAppChannel
{
	Task<ApplicationConfig> GetAppConfigAsync(string appID, CancellationToken token);
	Task<InvokeMethodResponse> InvokeMethodAsync(InvokeMethodRequest req, string appID, CancellationToken token);
	Task<InvokeMethodResponse> TriggerJobAsync(string name, object data);
}
