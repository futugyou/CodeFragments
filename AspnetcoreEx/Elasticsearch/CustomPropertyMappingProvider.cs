using System.Reflection;
using Nest;

namespace AspnetcoreEx.Elasticsearch;

public class CustomPropertyMappingProvider : PropertyMappingProvider
{
    public override IPropertyMapping CreatePropertyMapping(MemberInfo memberInfo)
    {
        return memberInfo.Name == nameof(Person.LastName)
            ? new PropertyMapping { Name = "lname" }
            : base.CreatePropertyMapping(memberInfo);
    }
}