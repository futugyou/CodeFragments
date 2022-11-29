namespace AspnetcoreEx.GraphQL;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.WebUtilities;

public record AddUserRequest(int id, string name, int age);
public record AddUserResponse(User use);
public record ProfilePictureUploadPayload(string UploadUrl);

public class Mutation
{
    /// mutation {
    ///     addUser(user: { id: 101, name: "two", age: 20 }) {
    ///         use {
    ///             id
    ///             name
    ///             age
    ///         }
    ///     }
    /// }
    [Error(typeof(DuplicateException))]
    public async Task<AddUserResponse> AddUser(AddUserRequest user, [Service] IUserRepository repository, [Service] ITopicEventSender sender)
    {
        var raw = repository.GetUserById(user.id);
        if (raw != null)
        {
            throw new DuplicateException(user.id);
        }
        User u = new User
        {
            Id = user.id,
            Name = user.name,
            Age = user.age,
        };
        var uselist = repository.AddUser(u);
        Console.WriteLine("send user create message, user id: " + u.Id);
        await sender.SendAsync("userCreated", u);
        return new AddUserResponse(uselist.FirstOrDefault(o => o.Id == user.id)!);
    }

    //public IPet? CreatePet(PetInput input)
    //{
    //    if (input.Cat != null)
    //    {
    //        return input.Cat;
    //    }
    //    return input.Dog;
    //}

    public async Task<bool> UploadFileAsync(IFile file)
    {
        var fileName = file.Name;
        var fileSize = file.Length;

        await using Stream stream = file.OpenReadStream();
        using var fs = System.IO.File.Create("./upload/" + fileName);
        await stream.CopyToAsync(fs);
        // We can now work with standard stream functionality of .NET
        // to handle the file.
        return true;
    }

    //[Authorize]
    public ProfilePictureUploadPayload UploadProfilePicture()
    {
        var baseUrl = "https://blob.chillicream.com/upload";

        // Here we can handle our authorization logic

        // If the user is allowed to upload the profile picture
        // we generate the token
        var token = "myuploadtoken";

        var uploadUrl = QueryHelpers.AddQueryString(baseUrl, "token", token);

        return new(uploadUrl);
    }
}

public class MutationType : ObjectType<Mutation>
{
    protected override void Configure(IObjectTypeDescriptor<Mutation> descriptor)
    {
        descriptor
        .Field(f => f.AddUser(default!, default!, default!))
        .Argument("user", a => a.Description("this is user input parameter description!"))
        //.Error<DuplicateException>()
        .UseMutationConvention();
    }
}