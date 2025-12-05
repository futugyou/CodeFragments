
namespace GraphQLStack;

public class Query
{
    // query {
    //    globalState
    // }
    // {
    //   "data": {
    //     "globalState": "testvalue"
    //   }
    // }
    public string GlobalState([GlobalState("testname")] string message)
    {
        return message;
    }
}
