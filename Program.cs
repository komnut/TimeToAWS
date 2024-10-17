using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using DotNetEnv;
using System;
using System.IO;
using System.Reflection;
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
            string bucketName = Env.GetString("BUCKET_NAME"); 
            string accessKey = Env.GetString("ACCESS_KEY"); 
            string secretKey = Env.GetString("SECRET_KEY"); 
            string keyName = Env.GetString("KEYNAME");
            string filePath = Env.GetString("FILE_TIME_PATH");

            // Generate a filename based on the current timestamp and date
            string currentDate = DateTime.UtcNow.ToString("yyyyMMdd");
            long currentTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            keyName += $"{currentDate}/Mytimer_{currentTimeStamp}.txt";

            // Check if the file exists
            filePath += @"20241009.txt";
            if (!File.Exists(filePath))
            {
                string message = $"Upload Time : {DateTime.Now}\n" +
                                 $"Upload Status : Failed\n" +
                                 $"Upload Description : File not found at {filePath}\n" +
                                 "/**************************************************************************************/\n";
                LogMessage(message);
                Console.WriteLine(message);
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

                // Upload the file to S3
                var response = await s3Client.PutObjectAsync(putRequest);
                string successMessage = $"Upload Time : {DateTime.Now}\n" +
                                        $"Upload Status : Success\n" +
                                        $"Upload Description : File uploaded successfully to {keyName}\n" +
                                        "/**************************************************************************************/\n";
                LogMessage(successMessage);
                Console.WriteLine(successMessage);
                Console.ReadKey();
            }
            catch (AmazonS3Exception ex)
            {
                string errorMessage = $"Upload Time : {DateTime.Now}\n" +
                                      $"Upload Status : Failed\n" +
                                      $"Upload Description : AWS S3 Error: {ex.Message}\n" +
                                      "/**************************************************************************************/\n";
                LogMessage(errorMessage);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                string generalError = $"Upload Time : {DateTime.Now}\n" +
                                      $"Upload Status : Failed\n" +
                                      $"Upload Description : General Error: {ex.Message}\n" +
                                      "/**************************************************************************************/\n";
                LogMessage(generalError);
                Console.ReadKey();
            }
        }

        // Method to log messages to a file in the application directory
        private static void LogMessage(string message)
        {
            try
            {
                // Get the path of the application's directory
                string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string logFilePath = Path.Combine(appPath, "upload_log.txt");

                // Append the message to the log file
                File.AppendAllText(logFilePath, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }
    }
}




