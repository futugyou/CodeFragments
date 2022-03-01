using Nest;

namespace AspnetcoreEx.Elasticsearch;

[ElasticsearchType(IdProperty = nameof(Name))]
public class Company
{
    public string Name { get; set; }
    public DateTime CreateAt { get; set; }
    public double Assets { get; set; }
    public GeoLocation LocationPoint { get; set; }
    public List<Employee> Employees { get; set; }
}

public class Employee
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Salary { get; set; }
    public DateTime Birthday { get; set; }
    public bool IsManager { get; set; }
    public List<Employee> Employees { get; set; }
    public TimeSpan Hours { get; set; }
}