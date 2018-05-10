namespace Zambo.Utility
{
    public class FileOperation
    {
        public static void ApagarArquivos(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        public static string CriarDiretorio(string path)
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

        public static string GetAbsolutePath(string documentTypeName)
        {
            if (string.IsNullOrWhiteSpace(documentTypeName))
            {
                throw new ArgumentException("The Path can't be null or white space.", nameof(documentTypeName));
            }

            string path = string.Empty;

            switch (documentTypeName)
            {
                case "Photo":
                    {
                        path = Path.Combine(Environment.CurrentDirectory, WebConfig.GetPathImagesKeyValue());

                        break;
                    }
                default:
                    {
                        path = Path.Combine(Environment.CurrentDirectory, WebConfig.GetPathDocumentsKeyValue());
                        break;
                    }
            }

            return path;
        }

        
        public static byte[] RetrieveFile(string path)
        {
            byte[] file = new byte[0];
            // Verifica se ficheiro existe
            var fi = new FileInfo(path);
            if (fi.Exists)
            {
                file = File.ReadAllBytes(path);
            }

            return file;
        }
        
       // http://www.digitalcoding.com/Code-Snippets/C-Sharp/C-Code-Snippet-Save-byte-array-to-file.html       
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
    }
}
