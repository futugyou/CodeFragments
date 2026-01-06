
namespace OpenSearchStack.Index;

public class IndexService
{
    public IndexService(ILogger<IndexService> log, OpenSearchClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly OpenSearchClient client;
    private readonly ILogger<IndexService> log;

    public async Task<bool> OrderIndex()
    {
        var createIndexResponse = await client.Indices.CreateAsync("order", c => c.Map<OrderInfo>(m => m.AutoMap()));
        return createIndexResponse.Acknowledged;
    }

    public async Task<bool> OrderPropertyVisitorIndex()
    {
        var createIndexResponse = await client.Indices.CreateAsync("order_propert_visitor", c => c.
             Map<OrderInfo>(m => m.AutoMap(new DisableDocValuesPropertyVisitor()))
        );

        return createIndexResponse.Acknowledged;
    }

    // company
    //   ├── Name
    //   ├── Employees.FirstName
    //   ├── Employees.LastName
    //   ├── Employees.Salary
    // var response = await client.SearchAsync<Company>(s => s
    //     .Index("company")
    //     .Query(q => q
    //         .Match(m => m
    //             .Field(f => f.Employees.FirstName)
    //             .Query("John")
    //         )
    //     )
    // );
    public async Task<bool> CompanyIndex()
    {
        var createIndexResponse = await client.Indices.CreateAsync("company", c => c
             .Map<Company>(m => m
                 .Properties(ps => ps
                     .Text(s => s
                         .Name(n => n.Name)
                     )
                     .Object<Employee>(o => o
                         .Name(n => n.Employees)
                         .Properties(eps => eps
                             .Text(s => s
                                 .Name(e => e.FirstName)
                             )
                             .Text(s => s
                                 .Name(e => e.LastName)
                             )
                             .Number(n => n
                                 .Name(e => e.Salary)
                                 .Type(NumberType.Integer)
                             )
                         )
                     )
                 )
             )
         );

        return createIndexResponse.Acknowledged;
    }

    // company-nested
    //   ├── Name
    //   ├── Employees (nested type)
    //     ├── Employees.FirstName
    //     ├── Employees.LastName
    //     ├── Employees.Salary
    // var response = await client.SearchAsync<Company>(s => s
    //     .Index("company-nested")
    //     .Query(q => q
    //         .Nested(n => n
    //             .Path(p => p.Employees)
    //             .Query(nq => nq
    //                 .Match(m => m
    //                     .Field(f => f.Employees.FirstName)
    //                     .Query("John")
    //                 )
    //             )
    //         )
    //     )
    // );
    public async Task<bool> CompanyNestedIndex()
    {
        var createIndexResponse = await client.Indices.CreateAsync("company-nested", c => c
               .Map<CompanyWithAttributes>(m => m
                   .AutoMap()
                   .Properties(ps => ps
                       .Nested<EmployeeWithAttributes>(n => n
                           .Name(nn => nn.Employees)
                           .AutoMap()
                           .Properties(pps => pps
                               .Text(s => s
                                   .Name(e => e.FirstName)
                                   .Fields(fs => fs
                                       .Keyword(ss => ss
                                           .Name("firstNameRaw")
                                       )
                                       .TokenCount(t => t
                                           .Name("length")
                                           .Analyzer("standard")
                                       )
                                   )
                               )
                               .Number(nu => nu
                                   .Name(e => e.Salary)
                                   .Type(NumberType.Double)
                                   .IgnoreMalformed(false)
                               )
                               .Date(d => d
                                   .Name(e => e.Birthday)
                                   .Format("MM-dd-yy")
                               )
                           )
                       )
                   )
               )
           );

        return createIndexResponse.Acknowledged;
    }

    public async Task<bool> PeopleIndex()
    {
        var createIndexResponse = await client.Indices.CreateAsync("people", c => c
            .Map<Person>(p => p
                .AutoMap() // automatically create the mapping from the type
                .Properties(props => props
                    .Keyword(t => t.Name("initials")) // create an additional field to store the initials
                    .Ip(t => t.Name(dv => dv.IpAddress)) // map field as IP Address type
                    .Object<GeoIp>(t => t.Name(dv => dv.GeoIp)) // map GeoIp as object
                )
            )
        );
        
        return createIndexResponse.Acknowledged;
    }

}