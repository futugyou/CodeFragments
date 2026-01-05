
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

    public async Task CreteElasticIndex()
    {
        await client.Indices.CreateAsync("order", c => c.Map<OrderInfo>(m => m.AutoMap()));
        await client.Indices.CreateAsync("order_propert_visitor", c => c.
             Map<OrderInfo>(m => m.AutoMap(new DisableDocValuesPropertyVisitor()))
         );

        await client.Indices.CreateAsync("company", c => c
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
        await client.Indices.CreateAsync("company1", c => c
             .Map<Company>(m => m
                 .AutoMap()
                 .Properties(ps => ps
                     .Nested<Employee>(n => n
                         .Name(nn => nn.Employees)
                     )
                 )
             )
         );

        await client.Indices.CreateAsync("company2", c => c
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

        await client.Indices.CreateAsync("people", c => c
            .Map<Person>(p => p
                .AutoMap() // automatically create the mapping from the type
                .Properties(props => props
                    .Keyword(t => t.Name("initials")) // create an additional field to store the initials
                    .Ip(t => t.Name(dv => dv.IpAddress)) // map field as IP Address type
                    .Object<GeoIp>(t => t.Name(dv => dv.GeoIp)) // map GeoIp as object
                )
            )
        );
    }

}