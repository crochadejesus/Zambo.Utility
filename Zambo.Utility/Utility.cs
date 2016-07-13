//
//  Utility.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 07/07/2016 21:37
//
//  Copyright (c) 2016 Zambo Tecnologia Ltda
//

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Zambo.Utility
{
	public struct Utility
	{
		/// <summary>
		/// Verifica se a dataIda é maior que a dataVolta;
		/// Verifica se a dataIda é menor que hoje;
		/// Verifica se a dataVolta é menor que hoje;
		/// </summary>
		/// <returns>The datas sao validas.</returns>
		/// <param name="dataIda">Data ida.</param>
		/// <param name="dataVolta">Data volta.</param>
		System.Collections.Generic.IList<string> AsDatasSaoValidas(string dataIda, string dataVolta)
		{
			System.Collections.Generic.IList<string> inconsistencias = new System.Collections.Generic.List<string>();
			System.DateTime nDataIda = System.Convert.ToDateTime(dataIda);
			System.DateTime nDataVolta = System.Convert.ToDateTime(dataVolta);
			int one_day = 1000 * 60 * 60 * 24;// Dia em milisegundos
			System.DateTime hoje = new DateTime();
			// Verifica se Ida é maior que Volta
			var seIdaForMaior = Math.Ceiling((this.GetTime(nDataIda) - this.GetTime(nDataVolta)) / (one_day));
			if (seIdaForMaior > 0)
			{
				inconsistencias.Add("- A data de Ida não pode ser maior que a data da Volta.");
			}
			// Verificar se Ida é menor que hoje
			var seIdaMenorQueHoje = Math.Ceiling((this.GetTime(nDataIda) - this.GetTime(hoje)) / (one_day));
			if (seIdaMenorQueHoje < 0)
			{
				inconsistencias.Add("- A data de Ida não pode ser menor que Hoje.");
			}
			// Verificar se Volta é menor que hoje
			var seVoltaMenorQueHoje = Math.Ceiling((this.GetTime(nDataVolta) - this.GetTime(hoje)) / (one_day));
			if (seVoltaMenorQueHoje < 0)
			{
				inconsistencias.Add("- A data de Volta não pode ser menor que Hoje.");
			}

			return inconsistencias;
		}

		/// <summary>
		/// The GetTime() method returns the numeric value corresponding to the time for the specified date according to universal time.
		/// Corresponde ao getTime() do javascript
		/// </summary>
		/// <returns>The time.</returns>
		double GetTime(DateTime data)
		{
			double retval = 0;
			var st = new DateTime(1970, 1, 1);
			TimeSpan t = (data.ToUniversalTime() - st);
			retval = (double)(t.TotalMilliseconds + 0.5);
			return retval;
		}

		double MilliTimeStamp(DateTime TheDate)
		{
			DateTime d1 = new DateTime(1970, 1, 1);
			DateTime d2 = TheDate.ToUniversalTime();
			TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);

			return ts.TotalMilliseconds;
		}

		/// <summary>
		/// Métodos para capitalizar Strings, ou seja, tornar a primeira letra
		/// depois de cada espaço maiúscula.
		/// Raphael Cardoso http://csharpbrasil.com.br/2009/01/08/dica-capitalizar-string-em-c-sharp/
		/// </summary>
		/// <param name="value">String</param>
		/// <returns>string Capitalizada</returns>
		public string CapitalizeWords(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			if (value.Length == 0)
			{
				return (value);
			}

			System.Text.StringBuilder result = new System.Text.StringBuilder(value);
			result[0] = char.ToUpper(result[0]);

			for (int i = 1; i < result.Length; ++i)
			{
				if (char.IsWhiteSpace(result[i - 1]))
				{
					result[1] = char.ToUpper(result[i]);
				}
			}
			return (result.ToString());
		}

		/// <summary>
		/// Métodos para capitalizar Strings, ou seja, tornar a primeira letra
		/// depois de cada espaço maiúscula.
		/// Raphael Cardoso http://csharpbrasil.com.br/2009/01/08/dica-capitalizar-string-em-c-sharp/
		/// </summary>
		/// <param name="value"></param>
		/// <returns>string Capitalizada</returns>
		public string CapitalizeWordsCulture(string value)
		{
			return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
		}

		public DateTime DateIsNull(string valor)
		{
			DateTime data;

			switch (valor)
			{
				case "":
					data = DateTime.Parse("01/01/1900 00:00:00");
					return data;
				default:
					try
					{
						data = Convert.ToDateTime(valor);
						return data;
					}
					catch (FormatException fe)
					{
						throw new Exception(fe.Message + "\n " + fe.Source + "\n " + fe.StackTrace);
					}
					catch (Exception err)
					{
						throw new Exception(err.Message + "\n " + err.Source + "\n " + err.StackTrace);
					}
			}
		}

		public ushort UInt16IsEmpty(string valor)
		{
			ushort inteiro;

			switch (valor)
			{
				case "":
					inteiro = ushort.MinValue;
					return inteiro;
				default:
					try
					{
						inteiro = Convert.ToUInt16(valor);
						return inteiro;
					}
					catch (FormatException fe)
					{
						throw new Exception(fe.Message + "\n " + fe.Source + "\n " + fe.StackTrace);
					}
					catch (Exception err)
					{
						throw new Exception(err.Message + "\n " + err.Source + "\n " + err.StackTrace);
					}
			}
		}

		public byte ByteIsEmpty(string valor)
		{
			byte inteiro;

			switch (valor)
			{
				case "":
					inteiro = byte.MinValue;
					return inteiro;
				default:
					try
					{
						inteiro = Convert.ToByte(valor);
						return inteiro;
					}
					catch (FormatException)
					{
						throw;
					}
					catch (Exception)
					{
						throw;
					}
			}
		}

		/// <summary>
		/// Testará se o objeto passado é inteiro ou não, usando expressão regular.
		/// Se o conteúdo da variável não estiver no range de 0 até 9, não é inteiro
		/// sendo assim retornará 0.
		/// </summary>
		/// <param name="valor"> Objeto a ser testado </param>
		/// <returns> Retorna 0 se não for inteiro </returns>
		public int IsUint(object valor)
		{
			Regex rgx = new Regex("[0-9]", RegexOptions.IgnoreCase);
			MatchCollection matchs = rgx.Matches(valor.ToString());
			return matchs.Count;
		}

		/// <summary>
		/// Testará se o objeto passado é float ou não, usando expressão regular.
		/// Se o conteúdo da variável não estiver no fomato 9,9999, não é float
		/// sendo assim retornará 0.
		/// </summary>
		/// <param name="valor"> Objeto a ser testado </param>
		/// <returns> Retorna 0 se não for float </returns>
		public int IsSingle(object valor)
		{
			Regex rgx = new Regex("[9,9999]");
			MatchCollection matchs = rgx.Matches(valor.ToString());
			return matchs.Count;
		}

		/// <summary>
		/// Testa se o objeto passado é inteiro ou não, ussando TryParse.
		/// </summary>
		/// <param name="valor"> Objeto a ser testado </param>
		/// <returns> Verdadeiro ou Falso </returns>
		public bool TestaSeEhNumero(object valor)
		{
			int testaSeENumero;
			return (Int32.TryParse(valor.ToString(), out testaSeENumero));
		}

		/// <summary>
		/// Testará se o objeto passado é um endereço d e-mail válido, Se esta no mínimo no formato seuemail@seudominio.com.
		/// </summary>
		/// <param name="email"> Endereço de e-mail a ser testado </param>
		/// <returns> Retorna verdadeiro caso coincida com o padrão </returns>
		public bool TestaSeEmailEhValido(object email)
		{
			string template = @"\b[a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}";
			Regex rgx = new Regex(template);
			return rgx.IsMatch(email.ToString());
		}

		/// <summary>
		/// Testará se o nome de login informado não começa com número.
		/// </summary>
		/// <param name="login"> Nome de login a ser testado </param>
		/// <returns> Retorna verdadeiro caso coincida com o padrão </returns>
		public bool TestaSeLoginEhValido(object login)
		{
			string template = @"\b^[0-9][a-zA-Z]";
			Regex rgx = new Regex(template);
			return rgx.IsMatch(login.ToString());
		}

		/// <summary>
		/// Testará se o CEP esta com 9 digítos e no formato 99999-999
		/// </summary>
		/// <param name="cep"> CEP a ser testado </param>
		/// <returns> Verdadeiro se coincidir com o padrão </returns>
		public bool TestaSeCepEhValido(object cep)
		{
			string template = @"\b[0-9]{5}-[0-9]{3}";
			Regex rgx = new Regex(template);
			return rgx.IsMatch(cep.ToString());
		}

		/// <summary>
		/// Clonar uma lista
		/// </summary>
		/// <param name="listToClone">List to clone.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public IList<T> Clone<T>(this IList<T> listToClone)
		{
			var array = new T[listToClone.Count];
			listToClone.CopyTo(array, 0);

			return array.ToList();
		}
	}
}

