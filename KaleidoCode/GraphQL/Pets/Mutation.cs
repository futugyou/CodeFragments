
namespace KaleidoCode.GraphQL.Pets;

[ExtendObjectType(typeof(Mutation))]
public class PetsMutation
{
    public IPet? CreatePet(PetInput input)
    {
        if (input.Cat != null)
        {
            return input.Cat;
        }
        return input.Dog;
    }

}
