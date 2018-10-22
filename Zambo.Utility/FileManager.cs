using System;
using System.IO;
using Zambo.Infrastructure.Configuration;

namespace Zambo.Utility.Helpers
{
    public class FileManager
    {
        public static void ByteArrayToFile(string path, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("The Path can't be null or white space.", nameof(path));
            }

            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            File.WriteAllBytes(path, bytes);
        }

        public static string CreateDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("The Path can't be null or white space.", nameof(path));
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static void DeleteFile(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        public static string GetAbsolutePath(string documentTypeName, long requestId, string fileName)
        {
            return Path.Combine(CreateDirectory(GetRelativePath(documentTypeName, requestId)), fileName);
        }

        public static string GetRelativePath(string documentTypeName, long requestId)
        {
            if (string.IsNullOrWhiteSpace(documentTypeName))
            {
                throw new ArgumentException("The Path can't be null or white space.", nameof(documentTypeName));
            }
            if (requestId < 1)
            {
                throw new ArgumentException("The RequestID can't be equal zero.", nameof(requestId));
            }

            string path = string.Empty;

            switch (documentTypeName)
            {
                case "Photo":
                    {
                        path = Path.Combine(WebConfig.GetImagePathKeyValue(), requestId.ToString());

                        break;
                    }
                default:
                    {
                        path = Path.Combine(WebConfig.GetDocumentPathKeyValue(), requestId.ToString());
                        break;
                    }
            }

            return path;
        }

        public static byte[] RetrieveFile(string path)
        {
            byte[] file = new byte[0];
            // Verify if file exists
            var fi = new FileInfo(path);
            if (fi.Exists)
            {
                file = File.ReadAllBytes(path);
            }

            return file;
        }
    }
}
