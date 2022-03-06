namespace AspnetcoreEx.GraphQL;

public class GenericResponse<T>
{
    public string Status { get; set; }
    public T Payload { get; set; }
}

public class GenericResponseType<TSchemaType, TRuntimeType> : ObjectType<GenericResponse<TRuntimeType>>
    where TSchemaType : class, IOutputType
{
    protected override void Configure(
        IObjectTypeDescriptor<GenericResponse<TRuntimeType>> descriptor)
    {
        descriptor.Field(f => f.Status);

        descriptor
            .Field(f => f.Payload)
            .Type<TSchemaType>();
    }
}

public class GenericQuery
{
    public GenericResponse<int> GetGenericResponse()
    {
        return new GenericResponse<int>
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
            .Type<GenericResponseType<IntType, int>>()
            ;
    }
}