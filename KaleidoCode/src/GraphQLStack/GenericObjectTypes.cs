namespace GraphQLStack;

public class GenericResponse<T>
{
    [GraphQLType(typeof(IdType))]
    public int Id { get; set; }
    public string Status { get; set; }
    [GraphQLType(typeof(EmailAddressType))]
    public string Email { get; set; }
    public T Payload { get; set; }
}

public class GenericResponseType<TSchemaType, TRuntimeType> : ObjectType<GenericResponse<TRuntimeType>>
    where TSchemaType : class, IOutputType
{
    protected override void Configure(
        IObjectTypeDescriptor<GenericResponse<TRuntimeType>> descriptor)
    {
        descriptor.Field(f => f.Status);
        descriptor.Field(f => f.Id).Type<IdType>();
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
            Id = 100,
            Status = "OK",
            Payload = 123,
            Email = "ab@cd.com"
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