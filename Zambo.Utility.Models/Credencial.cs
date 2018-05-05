namespace Zambo.Utility.Models
{
    public class Credencial
    {
        public Credencial(string usuario, string dominio, string senha)
        {
            Usuario = usuario;
            Dominio = dominio;
            Senha = senha;
        }

        public string Usuario { get; private set; }
        public string Dominio { get; private set; }
        public string Senha { get; private set; }
    }
}