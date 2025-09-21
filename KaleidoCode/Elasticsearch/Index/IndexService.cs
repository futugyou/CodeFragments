using Nest;

namespace KaleidoCode.Elasticsearch;

public class IndexService
{
    public IndexService(ILogger<IndexService> log, ElasticClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly ElasticClient client;
    private readonly ILogger<IndexService> log;

    public void CreteElasticIndex()
    {
        client.Indices.Create("order", c => c.Map<OrderInfo>(m => m.AutoMap()));
        client.Indices.Create("order_propert_visitor", c => c.
            Map<OrderInfo>(m => m.AutoMap(new DisableDocValuesPropertyVisitor()))
        );

        client.Indices.Create("company", c => c
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
        client.Indices.Create("company1", c => c
            .Map<Company>(m => m
                .AutoMap()
                .Properties(ps => ps
                    .Nested<Employee>(n => n
                        .Name(nn => nn.Employees)
                    )
                )
            )
        );

        client.Indices.Create("company2", c => c
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

        client.Indices.Create("people", c => c
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