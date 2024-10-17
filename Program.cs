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


            LogMessage($"Start upload file to S3 {currentDate}\n");

            // Check if the file exists
            filePath += @"20241009.txt";
            if (!File.Exists(filePath))
            {
                string message = GenerateUploadMessage(DateTime.Now, "Failed", $"File not found at {filePath}");
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
                string successMessage = GenerateUploadMessage(DateTime.Now, "Success", $"File uploaded successfully to {keyName}");
                LogMessage(successMessage);
                Console.WriteLine(successMessage);

                // Perform backup after successful upload
                BackupFile(filePath);

                Console.ReadKey();
            }
            catch (AmazonS3Exception ex)
            {
                string generalError = GenerateUploadMessage(DateTime.Now, "Failed", $"AWS S3 Error: {ex.Message}");
                LogMessage(generalError);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                string generalError = GenerateUploadMessage(DateTime.Now, "Failed", $"General Error: {ex.Message}");
                LogMessage(generalError);
                Console.ReadKey();
            }
        }

        // Function to generate upload messages
        private static string GenerateUploadMessage(DateTime timestamp, string status, string description)
        {
            return $"Upload Time : {timestamp}\n" +
                   $"Upload Status : {status}\n" +
                   $"Upload Description : {description}\n" +
                   "/**************************************************************************************/\n";
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
                string generalError = GenerateUploadMessage(DateTime.Now, "Failed", $"[LogMessage] General Error: {ex.Message}");
                LogMessage(generalError);
                Console.WriteLine(generalError);
            }
        }

        // Method to back up the uploaded file to a "backup" folder in the application path
        private static void BackupFile(string originalFilePath)
        {
            try
            {
                // Get the application's directory
                string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // Create a "backup" folder if it doesn't exist
                string backupFolderPath = Path.Combine(appPath, "backup");
                if (!Directory.Exists(backupFolderPath))
                {
                    Directory.CreateDirectory(backupFolderPath);
                }

                // Copy the file to the backup folder with a timestamp in the name
                string backupFileName = Path.GetFileName(originalFilePath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFilePath = Path.Combine(backupFolderPath, $"{timestamp}_{backupFileName}");

                File.Copy(originalFilePath, backupFilePath, overwrite: true);

                string generalError = GenerateUploadMessage(DateTime.Now, "Success", $"[BackupFile] File successfully backed up to: {backupFilePath}");
                LogMessage(generalError);
                Console.WriteLine(generalError);
            }
            catch (Exception ex)
            {
                string generalError = GenerateUploadMessage(DateTime.Now, "Failed", $"[BackupFile] Failed to back up file: {ex.Message}");
                LogMessage(generalError);
                Console.WriteLine(generalError);
            }
        }
    }
}




