namespace AwsS3BatchUpload;

public class AwsS3UploadCommand : OaktonAsyncCommand<AwsS3UploadInput>
{
    public override async Task<bool> Execute(AwsS3UploadInput input)
    {
        var client = new AmazonS3Client();
        var transferUtil = new TransferUtility(client);

        var keys = await GetAllFilesAsync(client, input.BucketName, input.KeyPrefix);
        await DeleteFullDirectoryAsync(client, input.BucketName, keys);
        await UploadFullDirectoryAsync(transferUtil, input.BucketName, input.KeyPrefix, input.LocalPath);

        return true;
    }

    private static async Task<List<KeyVersion>> GetAllFilesAsync(AmazonS3Client client, string bucketName, string keyPrefix)
    {
        var listrequest = new ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = keyPrefix,
        };

        var keys = new List<KeyVersion>();
        try
        {
            var listresponse = await client.ListObjectsV2Async(listrequest);
            if (listresponse == null || listresponse.S3Objects == null || !listresponse.S3Objects.Any())
            {
                return keys;
            }

            foreach (var s3obj in listresponse.S3Objects)
            {
                keys.Add(new KeyVersion { Key = s3obj.Key });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return keys;
    }

    private static async Task DeleteFullDirectoryAsync(AmazonS3Client client, string bucketName, List<KeyVersion> keys)
    {
        if (!keys.Any())
        {
            return;
        }

        var multiObjectDeleteRequest = new DeleteObjectsRequest
        {
            BucketName = bucketName,
            Objects = keys,
        };

        try
        {
            await client.DeleteObjectsAsync(multiObjectDeleteRequest);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static async Task UploadFullDirectoryAsync(TransferUtility transferUtil, string bucketName, string keyPrefix, string localPath)
    {
        if (!Directory.Exists(localPath))
        {
            return;
        }

        var request = new TransferUtilityUploadDirectoryRequest
        {
            BucketName = bucketName,
            KeyPrefix = keyPrefix,
            Directory = localPath,
            CannedACL = S3CannedACL.PublicRead,
            SearchOption = SearchOption.AllDirectories,
        };

        try
        {
            await transferUtil.UploadDirectoryAsync(request);
        }
        catch (AmazonS3Exception s3Ex)
        {
            Console.WriteLine($"Can't upload the contents of {localPath} because:");
            Console.WriteLine(s3Ex?.Message);
        }
    }
}
