using System.IO;
using System.Linq;
using Zambo.Utility.Models;

namespace Zambo.Utility
{
    internal class ManipulacaoRede : ManipulacaoRedeBase, Interface.IManipulacaoRede
    {
        private Credencial _credencial = null;

        public Credencial Credencial
        {
            get
            {
                if (_credencial == null)
                {
                    _credencial = new Credencial(
                        usuario: System.Configuration.ConfigurationManager.AppSettings["usuario"],
                        senha: System.Configuration.ConfigurationManager.AppSettings["senha"],
                        dominio: System.Configuration.ConfigurationManager.AppSettings["dominio"])
                    { };
                }

                return _credencial;
            }
        }

        public Common.Model.RetornoAcaoModel<System.Text.StringBuilder> CopiarArquivos(MoverArquivosParam parametros)
        {
            if (parametros.DirSeraMovidoAnoExist)
            {
                if (parametros.RetornoAcaoModel == null)
                {
                    parametros.RetornoAcaoModel = new Common.Model.RetornoAcaoModel<System.Text.StringBuilder>
                    {
                        Retorno = new System.Text.StringBuilder()
                    };
                }

                FileInfo[] arquivosParaCopiar = GetFiles(parametros.Di, "*.*", parametros.Credencial);
                for (int j = 0; j < arquivosParaCopiar.Length; j++)
                {
                    try
                    {
                        string novoCaminho = string.Format("{0}\\{1}", parametros.DiretorioDestino, arquivosParaCopiar[j].Name);
                        FileCopy(arquivosParaCopiar[j].FullName, novoCaminho, true, parametros.Credencial);

                        parametros.RetornoAcaoModel.Resultado = true;
                    }
                    catch (System.UnauthorizedAccessException uae)
                    {
                        parametros.RetornoAcaoModel.Resultado = false;
                        parametros.RetornoAcaoModel.Retorno.AppendLine(uae.Message).AppendLine().Append(uae.InnerException.ToString()).AppendLine(uae.StackTrace);
                    }
                }
            }

            return parametros.RetornoAcaoModel;
        }

        public void CreateDirectory(string path, Credencial credencial)
        {
            var windowsIdentity = AbrirConexaoServidor(credencial);
            Directory.CreateDirectory(path);
            FecharConexaoServidor(windowsIdentity);
        }

        public FileStream CreateFile(string path, Credencial credencial)
        {
            FileStream fs = null;
            var windowsIdentity = AbrirConexaoServidor(credencial);
            fs = File.Create(path);
            FecharConexaoServidor(windowsIdentity);
            return fs;
        }

        public bool DirectoryExists(string path, Credencial credencial)
        {
            AbrirConexaoServidor(credencial);
            bool resultado = Directory.Exists(path);

            return resultado;
        }

        public void FileCopy(string sourceFileName, string destFileName, bool overwrite, Credencial credencial)
        {
            if (string.IsNullOrWhiteSpace(sourceFileName))
            {
                throw new System.ArgumentException("Nome do arquivo de origem não pode ser omitido", nameof(sourceFileName));
            }

            if (string.IsNullOrWhiteSpace(destFileName))
            {
                throw new System.ArgumentException("Nome do arquivo de destino não pode ser omitido", nameof(destFileName));
            }

            WindowsIdentity windowsIdentity = AbrirConexaoServidor(credencial);
            File.Copy(sourceFileName, destFileName, true);
            FecharConexaoServidor(windowsIdentity);
        }

        public void FileDelete(string path, Credencial credencial)
        {
            WindowsIdentity windowsIdentity = AbrirConexaoServidor(credencial);
            File.Delete(path);
            FecharConexaoServidor(windowsIdentity);
        }

        public string[] GetDirectories(string path, Credencial credencial)
        {
            AbrirConexaoServidor(credencial);
            string[] directoryList = Directory.GetDirectories(path);
            return directoryList;
        }

        public string[] GetDirectories(string path, string searchPattern, Credencial credencial)
        {
            AbrirConexaoServidor(credencial);
            string[] directoryList = Directory.GetDirectories(path, searchPattern);
            return directoryList;
        }

        public DirectoryInfo GetDirectoryInfo(string filePathFullName, Credencial credencial)
        {
            AbrirConexaoServidor(credencial);
            DirectoryInfo di = new DirectoryInfo(filePathFullName);
            return di;
        }

        public FileInfo GetFileInfo(string fileName, Credencial credencial)
        {
            AbrirConexaoServidor(credencial);
            FileInfo filePath = new FileInfo(fileName);
            return filePath;
        }

        public FileInfo[] GetFiles(DirectoryInfo di, string searchPattern, Credencial credencial)
        {
            AbrirConexaoServidor(credencial);
            FileInfo[] arquivosList = di.GetFiles(searchPattern);
            return arquivosList;
        }

        public IOrderedEnumerable<FileInfo> GetFilesOrdenedByCreationTime(FileInfo[] arquivosList, Credencial credencial)
        {
            AbrirConexaoServidor(credencial);
            IOrderedEnumerable<FileInfo> file = arquivosList.OrderBy(f => f.CreationTime);
            return file;
        }

        public void GravarLog(GravarLogParams parametros)
        {
            var windowsIdentity = AbrirConexaoServidor(parametros.Credencial);
            var sw = new StreamWriter(parametros.Path + ".log");
            sw.Write(parametros.StringBuilder.ToString());
            FecharConexaoServidor(windowsIdentity);
            sw.Close();
        }

        public Common.Model.RetornoAcaoModel<System.Text.StringBuilder> MoverArquivos(MoverArquivosParam parametros)
        {
            System.Collections.Generic.IList<Common.Model.RetornoAcaoModel<string>> listaResultadoCopiarApagar = new System.Collections.Generic.List<Common.Model.RetornoAcaoModel<string>>();

            if (parametros.DirSeraMovidoAnoExist)
            {
                if (parametros.RetornoAcaoModel == null)
                {
                    parametros.RetornoAcaoModel = new Common.Model.RetornoAcaoModel<System.Text.StringBuilder>
                    {
                        Retorno = new System.Text.StringBuilder()
                    };
                }

                FileInfo[] arquivosParaCopiarApagar = GetFiles(parametros.Di, "*.*", parametros.Credencial);
                for (int j = 0; j < arquivosParaCopiarApagar.Length; j++)
                {
                    try
                    {
                        string novoCaminho = string.Format("{0}\\{1}", parametros.DiretorioDestino, arquivosParaCopiarApagar[j].Name);
                        FileCopy(arquivosParaCopiarApagar[j].FullName, novoCaminho, true, parametros.Credencial);

                        listaResultadoCopiarApagar.Add(new Common.Model.RetornoAcaoModel<string>()
                        {
                            Resultado = true,
                            Retorno = arquivosParaCopiarApagar[j].FullName
                        });
                    }
                    catch (System.UnauthorizedAccessException uae)
                    {
                        listaResultadoCopiarApagar.Add(new Common.Model.RetornoAcaoModel<string>()
                        {
                            Resultado = false
                        });

                        parametros.RetornoAcaoModel.Resultado = false;
                        parametros.RetornoAcaoModel.Retorno.AppendLine(uae.Message).AppendLine().Append(uae.InnerException).AppendLine(uae.StackTrace);
                    }
                }

                for (int k = 0; k < listaResultadoCopiarApagar.Count; k++)
                {
                    if (listaResultadoCopiarApagar[k].Resultado)
                    {
                        try
                        {
                            FileDelete(listaResultadoCopiarApagar[k].Retorno, parametros.Credencial);
                        }
                        catch (System.Exception ex)
                        {
                            parametros.RetornoAcaoModel.Resultado = false;
                            parametros.RetornoAcaoModel.Retorno.AppendLine(ex.Message).AppendLine().Append(ex.InnerException).AppendLine(ex.StackTrace).ToString();
                        }
                    }
                }
            }

            parametros.RetornoAcaoModel.Resultado = true;
            return parametros.RetornoAcaoModel;
        }

        public void MoverArquivos(
            string filePathFullName,
            Interface.Gui.IConvercaoArquivo converterArquivo,
            DirectoryInfo directoryInfo,
            Common.Model.RetornoAcaoModel<System.Text.StringBuilder> retornoAcaoModel)
        {
            var dirSeraMovido = string.Format("{0}{1}", filePathFullName, System.Configuration.ConfigurationManager.AppSettings["finalizado"]);
            var diretorioDestino = string.Format("{0}\\{1}", dirSeraMovido, System.DateTime.Today.Year.ToString());

            // Move os arquivos de diretorios
            retornoAcaoModel = MoverArquivos(new MoverArquivosParam()
            {
                Di = directoryInfo,
                DirSeraMovidoAnoExist = ValidarExistenciaDiretorio(dirSeraMovido, diretorioDestino, Credencial),
                DiretorioDestino = diretorioDestino,
                Credencial = Credencial
            });

            Common.Helper.Validator.EnviarEmail(converterArquivo.ToString(), retornoAcaoModel.Retorno.ToString(), null);
        }

        public bool ValidarExistenciaDiretorio(string dirSeraMovido, string diretorioDestino, Credencial credencial)
        {
            // Validar existencia dos diretorios
            bool dirSeraMovidoExist = false;
            bool dirSeraMovidoAnoExist = false;

            if (DirectoryExists(dirSeraMovido, credencial))
            {
                dirSeraMovidoExist = true;
            }
            else
            {
                CreateDirectory(dirSeraMovido, credencial);
                dirSeraMovidoExist = true;
            }

            if (dirSeraMovidoExist)
            {
                if (DirectoryExists(diretorioDestino, credencial))
                {
                    dirSeraMovidoAnoExist = true;
                }
                else
                {
                    CreateDirectory(diretorioDestino, credencial);
                    dirSeraMovidoAnoExist = true;
                }
            }

            return dirSeraMovidoAnoExist;
        }
    }
}