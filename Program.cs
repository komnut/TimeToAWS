using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using DotNetEnv;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TimeToAWS
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Env.Load();

            // Bucket and credential information
            string bucketName = Env.GetString("BUCKET_NAME"); ;
            string accessKey = Env.GetString("ACCESS_KEY"); ;
            string secretKey = Env.GetString("SECRET_KEY"); ;

            // Generate a filename based on the current timestamp and date
            string currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            long currentTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string keyName = $"example-domain/Org_Code/logType/Member_Id/{currentDate}/{currentTimeStamp}.log";

            // Check if the file exists
            string filePath = @"D:\LogPath\20241009.txt";
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found at: " + filePath);
                return;
            }

            // Set up AWS credentials and create an S3 client
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var s3Client = new AmazonS3Client(credentials, RegionEndpoint.APSouth1);

            try
            {
                // Read the file and convert it to Base64
                byte[] fileBytes = File.ReadAllBytes(filePath);
                string base64Content = Convert.ToBase64String(fileBytes);
                var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(base64Content));

                // Configure the PutObjectRequest for uploading
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    InputStream = contentStream,
                    ContentType = "text/plain",
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256, // AES256 encryption
                };

                // Add metadata for content encoding
                //putRequest.Metadata.Add("x-amz-meta-content-encoding", "base64");

                // Upload the file to S3
                var response = await s3Client.PutObjectAsync(putRequest);
                Console.WriteLine("Upload completed successfully!");
                Console.ReadKey();
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"AWS S3 Error: {ex.Message}");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                Console.ReadKey();
            }
        }
    }
}






////Error encountered on server. Message:'User: arn:aws:iam::692647644057:user/hrconnex is not authorized to perform: s3:PutObject on resource: "arn:aws:s3:::hrconnexuat.entomohr.sc.co/your-file-name" because no identity-based policy allows the s3:PutObject action' when writing an object
////Error encountered on server. Message:'User: arn:aws:iam::692647644057:user/hrconnex is not authorized to perform: s3:PutObject on resource: "arn:aws:s3:::hrconnexuat.entomohr.sc.co/20241009.txt" because no identity-based policy allows the s3:PutObject action' when writing an object
////AWS S3 Error: User: arn: aws: iam::692647644057:user / hrconnex is not authorized to perform: s3: PutObject on resource: "arn:aws:s3:::hrconnexuat.entomohr.sc.co/example-domain/Org_Code/logType/Member_Id/2024-10-15/1728979480.log" because no identity-based policy allows the s3:PutObject action
////AWS S3 Error: User: arn:aws:iam::692647644057:user/hrconnex is not authorized to perform: s3:PutObject on resource: "arn:aws:s3:::hrconnexuat.entomohr.sc.co/example-domain/Org_Code/logType/Member_Id/2024-10-15/1728979663.log" because no identity-based policy allows the s3:PutObject action
////AWS S3 Error: User: arn:aws:iam::692647644057:user/hrconnex is not authorized to perform: s3:PutObject on resource: "arn:aws:s3:::hrconnexuat.entomohr.sc.co/example-domain/Org_Code/logType/Member_Id/2024-10-15/1728979757.log" because no identity-based policy allows the s3:PutObject action






