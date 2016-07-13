//
//  RetornoAcaoModel.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 07/07/2016 21:57
//
//  Copyright (c) 2016  Zambo Tecnologia Ltda
//

namespace Zambo.Models
{
	public struct RetornoAcaoModel
	{
		string mensagem;
		bool resultado;

		public string Mensagem
		{
			get
			{
				return this.mensagem;
			}

			set
			{
				this.mensagem = value;
			}
		}

		public bool Resultado
		{
			get
			{
				return this.resultado;
			}

			set
			{
				this.resultado = value;
			}
		}
	}
}

