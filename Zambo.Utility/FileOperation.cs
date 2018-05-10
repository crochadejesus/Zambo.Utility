namespace Zambo.Utility
{
    public class FileOperation
    {
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
