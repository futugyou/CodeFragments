using System.Reflection;
using OpenSearch.Client;

namespace KaleidoCode.Elasticsearch;

public class DisableDocValuesPropertyVisitor : NoopPropertyVisitor
{
    public override void Visit(INumberProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute)
    {
        type.DocValues = false;
    }

    public override void Visit(IBooleanProperty type, PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute)
    {
        type.DocValues = false;
    }

    public override IProperty Visit(PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute)
    {
        // all field tpye is text
        return new TextProperty();
    }
    public override bool SkipProperty(PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute)
    {
        return propertyInfo?.DeclaringType != typeof(DictionaryDocument);
    }
}

public class DictionaryDocument : SortedDictionary<string, dynamic>
{
    public int Id { get; set; }
}
