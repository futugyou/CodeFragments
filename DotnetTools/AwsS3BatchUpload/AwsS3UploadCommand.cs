namespace AwsS3BatchUpload;

public class AwsS3UploadCommand : OaktonAsyncCommand<AwsS3UploadInput>
{

    public override Task<bool> Execute(AwsS3UploadInput input)
    {
        var client = new AmazonS3Client();
        var transferUtil = new TransferUtility(client);
        return UploadFullDirectoryAsync(transferUtil, input.BucketName, input.KeyPrefix, input.LocalPath);
    }

    private static async Task<bool> UploadFullDirectoryAsync(TransferUtility transferUtil,
                                                      string bucketName,
                                                      string keyPrefix,
                                                      string localPath)
    {
        if (Directory.Exists(localPath))
        {
            try
            {
                await transferUtil.UploadDirectoryAsync(new TransferUtilityUploadDirectoryRequest
                {
                    BucketName = bucketName,
                    KeyPrefix = keyPrefix,
                    Directory = localPath,
                    CannedACL = S3CannedACL.PublicRead,
                    SearchOption = SearchOption.AllDirectories,
                });

                return true;
            }
            catch (AmazonS3Exception s3Ex)
            {
                Console.WriteLine($"Can't upload the contents of {localPath} because:");
                Console.WriteLine(s3Ex?.Message);
                return false;
            }
        }
        else
        {
            Console.WriteLine($"The directory {localPath} does not exist.");
            return false;
        }
    }
}
