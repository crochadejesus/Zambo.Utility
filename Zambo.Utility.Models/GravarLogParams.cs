using System.Collections.Generic;
using System.Text;

namespace Zambo.Utility.Models
{
    public struct GravarLogParams
    {
        public GravarLogParams(string path, StringBuilder stringBuilder, int totalBanco, Credencial credencial) : this()
        {
            Path = path;
            StringBuilder = stringBuilder;
            Credencial = credencial;
        }

        public string Path { get; private set; }
        public StringBuilder StringBuilder { get; private set; }
        public Credencial Credencial { get; private set; }
    }
}