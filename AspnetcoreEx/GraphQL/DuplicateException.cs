namespace AspnetcoreEx.GraphQL;

public class DuplicateException : Exception
{
    public DuplicateException(int id)
        : base($"user with id: {id} is existed!")
    {
    }
}