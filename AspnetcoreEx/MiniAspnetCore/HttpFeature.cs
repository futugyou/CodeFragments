using System.Collections.Specialized;

namespace AspnetcoreEx.MiniAspnetCore;

public interface IFeatureCollection : IDictionary<Type, object?>
{

}

public class FeatureCollection : Dictionary<Type, object?>, IFeatureCollection
{

}

public static partial class MiniExtensions
{
    public static T? Get<T>(this IFeatureCollection feature) where T:class
    {
        return feature.TryGetValue(typeof(T), out var value) ? (T?)value : default;
    }

    public static IFeatureCollection Set<T>(this IFeatureCollection features, T? feature) where T:class
    {
        features[typeof(T)] = feature;
        return features;
    }
}

public interface IHttpRequestFeature
{
    Uri? Url { get; }
    NameValueCollection Headers { get; }
    Stream Body { get; }
}

public interface IHttpResponseFeature
{
    int StatusCode { get; set; }
    NameValueCollection Headers { get; }
    Stream Body { get; }
}