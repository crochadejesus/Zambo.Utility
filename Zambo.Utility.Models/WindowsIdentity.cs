namespace Zambo.Utility.Models
{
    public class WindowsIdentity
    {
        public System.IntPtr Token { get; set; }

        public System.Security.Principal.WindowsImpersonationContext Wic { get; set; }

        public Common.Model.RetornoAcaoModel<object> RetornoAcaoModel { get; set; } = new Common.Model.RetornoAcaoModel<object>();
    }
}