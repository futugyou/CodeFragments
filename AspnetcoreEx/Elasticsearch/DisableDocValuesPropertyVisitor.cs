using System.Reflection;
using Nest;

namespace AspnetcoreEx.Elasticsearch;

public class DisableDocValuesPropertyVisitor : NoopPropertyVisitor
{
    public override void Visit(INumberProperty type, PropertyInfo propertyInfo, ElasticsearchPropertyAttributeBase attribute)
    {
        type.DocValues = false;
    }

    public override void Visit(IBooleanProperty type, PropertyInfo propertyInfo, ElasticsearchPropertyAttributeBase attribute)
    {
        type.DocValues = false;
    }

    public override IProperty Visit(PropertyInfo propertyInfo, ElasticsearchPropertyAttributeBase attribute)
    {
        // all field tpye is text
        return new TextProperty();
    }
    public override bool SkipProperty(PropertyInfo propertyInfo, ElasticsearchPropertyAttributeBase attribute)
    {
        return propertyInfo?.DeclaringType != typeof(DictionaryDocument);
    }
}

public class DictionaryDocument : SortedDictionary<string, dynamic>
{
    public int Id { get; set; }
}
