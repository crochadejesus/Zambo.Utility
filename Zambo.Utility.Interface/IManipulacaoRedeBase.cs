namespace Zambo.Utility.Interface
{
    public interface IManipulacaoRedeBase
    {
        Models.WindowsIdentity AbrirConexaoServidor(Models.Credencial credencial);

        void FecharConexaoServidor(Models.WindowsIdentity windowsIdentity);
    }
}