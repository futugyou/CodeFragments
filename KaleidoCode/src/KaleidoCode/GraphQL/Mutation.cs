
using Microsoft.AspNetCore.WebUtilities;

namespace KaleidoCode.GraphQL;

public class Mutation
{
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

public record ProfilePictureUploadPayload(string UploadUrl);
