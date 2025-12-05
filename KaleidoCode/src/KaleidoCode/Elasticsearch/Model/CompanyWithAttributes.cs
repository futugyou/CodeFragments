using OpenSearch.Client;

namespace KaleidoCode.Elasticsearch;

[OpenSearchType(RelationName = "company_wWith_attr")]
public class CompanyWithAttributes
{
    [Keyword(NullValue = "null", Similarity = "BM25")]
    public string Name { get; set; }

    [Text(Name = "office_hours")]
    public TimeSpan? HeadOfficeHours { get; set; }

    [Object(Store = false)]
    public List<EmployeeWithAttributes> Employees { get; set; }
}

[OpenSearchType(RelationName = "employee")]
public class EmployeeWithAttributes
{
    [Text(Name = "first_name")]
    public string FirstName { get; set; }

    [Text(Name = "last_name")]
    public string LastName { get; set; }

    [Number(DocValues = false, IgnoreMalformed = true, Coerce = true)]
    public int Salary { get; set; }

    [Date(Format = "MMddyyyy")]
    public DateTime Birthday { get; set; }

    [Boolean(NullValue = false, Store = true)]
    public bool IsManager { get; set; }

    [Nested]
    [PropertyName("empl")]
    public List<Employee> Employees { get; set; }
}