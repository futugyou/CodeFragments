using HashidsNet;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KaleidoCode.Extensions;

public class HashIdModelBinder : IModelBinder
{
    Hashids hashids = new Hashids("thisisdefaultsalt", 10);// 加盐, 加最小长度

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var modelName = bindingContext.ModelName;

        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        var str = valueProviderResult.FirstValue;

        bindingContext.Result = ModelBindingResult.Success(hashids.Decode(str)[0]);

        return Task.CompletedTask;
    }
}