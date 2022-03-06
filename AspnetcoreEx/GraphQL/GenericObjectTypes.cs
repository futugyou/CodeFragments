namespace AspnetcoreEx.GraphQL;

public class GenericResponse
{
    public string Status { get; set; }
    public object Payload { get; set; }
}

public class GenericResponseType<T> : ObjectType<GenericResponse>
    where T : class, IOutputType
{
    protected override void Configure(
        IObjectTypeDescriptor<GenericResponse> descriptor)
    {
        descriptor.Field(f => f.Status);

        descriptor
            .Field(f => f.Payload)
            .Type<T>();
    }
}

public class GenericQuery
{
    public GenericResponse GetGenericResponse()
    {
        return new GenericResponse
        {
            Status = "OK",
            Payload = 123
        };
    }
}

public class GenericQueryType : ObjectType<GenericQuery>
{
    protected override void Configure(IObjectTypeDescriptor<GenericQuery> descriptor)
    {
        descriptor
            .Field(f => f.GetGenericResponse())
            .Type<GenericResponseType<IntType>>()
            ;
    }
}