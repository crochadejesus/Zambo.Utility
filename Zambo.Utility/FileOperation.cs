namespace Zambo.Utility
{
    public class FileOperation
    {
       // http://www.digitalcoding.com/Code-Snippets/C-Sharp/C-Code-Snippet-Save-byte-array-to-file.html       
       public bool ByteArrayToFile(string path, byte[] bytes)
       {
         if (string.IsNullOrWhiteSpace(path))
         {
            throw new System.ArgumentException("The Path can't be null or white space.", nameof(path));
         }

         if (bytes == null)
         {
            throw new System.ArgumentNullException(nameof(bytes));
         }

         bool response;

             // Open file for reading
            System.IO.FileStream fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write);

            // Writes a block of bytes to this stream using data from a byte array.
            fileStream.WriteAsync(bytes, 0, bytes.Length);
            
            // Close file stream
            fileStream.Close();
            response = true;

          return response;
       }
    }
}