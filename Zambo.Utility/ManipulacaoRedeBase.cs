using Zambo.Utility.Models;

namespace Zambo.Utility
{
    internal abstract class ManipulacaoRedeBase : Interface.IManipulacaoRedeBase
    {
        [System.Runtime.InteropServices.DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out System.IntPtr phToken);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private extern static bool CloseHandle(System.IntPtr handle);

        /// <summary>
        /// Deve-se chamar este método para se autenticar no servidor da rede para ter acesso aos diretórios e manipular arquivos,
        /// sempre antes de qualquer ação (Criar, Apagar ou Listar).
        /// </summary>
        /// <param name="credencial"></param>
        /// <returns></returns>
        public Models.WindowsIdentity AbrirConexaoServidor(Models.Credencial credencial)
        {
            System.IntPtr token;
            System.Security.Principal.WindowsIdentity wi;
            Models.WindowsIdentity windowsIdentity = new Models.WindowsIdentity();

            if (LogonUser(credencial.Usuario,
                credencial.Dominio,
                credencial.Senha,
                9, // LOGON32_LOGON_NEW_CREDENTIALS
                0, // LOGON32_PROVIDER_DEFAULT
                out token))
            {
                wi = new System.Security.Principal.WindowsIdentity(token);
                windowsIdentity.Token = token;
                windowsIdentity.Wic = wi.Impersonate();
                windowsIdentity.RetornoAcaoModel.Resultado = true;
            }
            else
            {
                windowsIdentity.RetornoAcaoModel.Resultado = false;
                windowsIdentity.RetornoAcaoModel.Mensagem = string.Format("LogonUser() falhou ao se conectar com o código de erro {0}", System.Runtime.InteropServices.Marshal.GetLastWin32Error());
            }

            return windowsIdentity;
        }

        /// <summary>
        /// Deve-se chamar este método sempre depois de executar qualquer ação (Criar ou Apagar). Sem isso a ação não será executada.
        /// Depois do Listar não precisa.
        /// </summary>
        /// <param name="windowsIdentity"></param>
        public void FecharConexaoServidor(Models.WindowsIdentity windowsIdentity)
        {
            windowsIdentity.Wic.Undo();
            CloseHandle(windowsIdentity.Token);
        }
    }
}