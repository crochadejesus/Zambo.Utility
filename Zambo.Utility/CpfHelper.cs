//
//  CpfHelper.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 07/07/2016 22:51
//
//  Copyright (c) 2016 Zambo Tecnologia Ltda
//

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Zambo.Utility
{
	public class CpfHelper
	{
		private static readonly string[] CpfsIgnorados;

		static CpfHelper()
		{
			CpfsIgnorados = new string[10];
			for (var num = 0; num < 10; num++)
			{
				CpfsIgnorados[num] = new string((char)('0' + num), 11);
			}
		}

		public static bool ValidaCPF(string cpf)
		{
			return ValidaFormatoCPF(cpf) && ValidaDigitoVerificadorCPF(cpf);
		}

		private static bool ValidaDigitoVerificadorCPF(string cpf)
		{
			cpf = cpf.Replace(".", "").Replace("-", "").Replace("\"", "");

			if (CpfsIgnorados.Any(c => c.Equals(cpf))) return false;

			var multiplicador1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
			var multiplicador2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
			var tempCpf = cpf.Substring(0, 9);
			var soma = 0;
			for (var i = 0; i < 9; i++)
			{
				soma += Int32.Parse(tempCpf[i].ToString()) * multiplicador1[i];
			}
			var resto = soma % 11;
			resto = resto < 2 ? 0 : 11 - resto;
			var digito = resto.ToString();
			tempCpf = tempCpf + digito;
			soma = 0;
			for (var i = 0; i < 10; i++)
			{
				soma += Int32.Parse(tempCpf[i].ToString()) * multiplicador2[i];
			}
			resto = soma % 11;
			resto = resto < 2 ? 0 : 11 - resto;
			digito = digito + resto;
			return cpf.EndsWith(digito);
		}

		private static bool ValidaFormatoCPF(string cpf)
		{
			return Regex.Match(cpf, @"^\d{3}.\d{3}.\d{3}-\d{2}$").Success;
		}
	}
}

