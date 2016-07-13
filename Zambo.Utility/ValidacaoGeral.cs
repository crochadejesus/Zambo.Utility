//
//  ValidacaoGeral.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 07/07/2016 22:54
//
//  Copyright (c) 2016 Zambo Tecnologia Ltda
//

using System.Collections.Generic;

namespace Zambo.Utility
{
	internal class ValidacaoGeral
	{
		/// <summary>
		/// Inicia uma nova instância da classe <see cref="ValidacaoGeral" />
		/// </summary>
		public ValidacaoGeral()
		{
			this.Campo = new List<string>();
		}

		/// <summary>
		/// Obtém ou define uma lista de campos a serem adicionados na mensagem de alerta.
		/// </summary>
		public IList<string> Campo { get; set; }

		/// <summary>
		/// Método responsável por adicionar os campos a mensagem de alerta de campos em branco.
		/// </summary>
		/// <returns> Retorna a mensagem de alerta a ser exibida. </returns>
		public string AlertarCamposNaoInformados()
		{
			var mensagemDeAlerta = string.Empty;

			if (this.Campo.Count > 0)
			{
				System.Text.StringBuilder mensagemCamposInvalidos = new System.Text.StringBuilder();
				mensagemCamposInvalidos.Append("<span style='text-align:left'>");
				mensagemCamposInvalidos.Append("Favor preencher corretamente os campos abaixo:" + "</span><br>");

				foreach (var item in this.Campo)
				{
					mensagemCamposInvalidos.Append("- " + item + "<br/>");
				}

				mensagemDeAlerta = mensagemCamposInvalidos.ToString();
			}

			return mensagemDeAlerta;
		}
	}
}

