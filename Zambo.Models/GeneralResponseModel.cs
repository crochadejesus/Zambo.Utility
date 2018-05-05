//
//  GeneralResponseModel.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 07/07/2016 21:57
//
//  Copyright (c) 2016  Zambo Tecnologia Ltda
//

namespace Zambo.Models
{
	public struct GeneralResponseModel<T>
	{
		public T Response { get; set; }
		public string Message { get; set; }
		public bool Success { get; set; }
	}
}

