//
//  MensagemEmailModel.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 07/07/2016 22:08
//
//  Copyright (c) 2016 Zambo Tecnologia Ltda
//

namespace Zambo.Utility.Models
{
	public struct MensagemEmailModel
	{
		public string De { get; set; }
		public string Reply { get; set; }
		public string[] Para { get; set; }
		public string[] Cc { get; set; }
		public string[] Cco { get; set; }
		public string Assunto { get; set; }
		public string Corpo { get; set; }
		public bool IsHtml { get; set; }
	}
}

