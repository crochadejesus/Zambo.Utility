using Zambo.Utility.Models;

namespace Zambo.Utility.Interface
{
    public interface IManipulacaoRede
    {
        void CreateDirectory(string path, Credencial credencial);

        bool DirectoryExists(string path, Credencial credencial);

        void FileCopy(string sourceFileName, string destFileName, bool overwrite, Credencial credencial);

        string[] GetDirectories(string path, Credencial credencial);

        string[] GetDirectories(string path, string searchPattern, Credencial credencial);

        System.IO.DirectoryInfo GetDirectoryInfo(string filePathFullName, Credencial credencial);

        System.IO.FileInfo GetFileInfo(string fileName, Credencial credencial);

        System.IO.FileInfo[] GetFiles(System.IO.DirectoryInfo di, string searchPattern, Credencial credencial);

        System.Linq.IOrderedEnumerable<System.IO.FileInfo> GetFilesOrdenedByCreationTime(System.IO.FileInfo[] arquivosList, Credencial credencial);

        void GravarLog(GravarLogParams parametros);

        Models.GeneralResponseModel<System.Text.StringBuilder> MoverArquivos(MoverArquivosParam parametros);

        bool ValidarExistenciaDiretorio(string dirSeraMovido, string diretorioDestino, Credencial credencial);
    }
}