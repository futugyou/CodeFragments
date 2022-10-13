namespace AwsS3BatchUpload;

public class AwsS3UploadInput
{
    public string BucketName { get; set; }
    public string KeyPrefix { get; set; }
    public string LocalPath { get; set; }
}
