namespace Zambo.Utility.Models
{
    public class MoverArquivosParam
    {
        public Models.Credencial Credencial { get; set; }

        public System.IO.DirectoryInfo Di { get; set; }

        public string DiretorioDestino { get; set; }

        public bool DirSeraMovidoAnoExist { get; set; }

        public Models.GeneralResponseModel<System.Text.StringBuilder> RetornoAcaoModel { get; set; }
    }
}