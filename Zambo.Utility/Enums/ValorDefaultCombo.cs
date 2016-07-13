//
//  ValorDefaultCombo.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 07/07/2016 22:39
//
//  Copyright (c) 2016 Zambo Tecnologia Ltda
//

using System.ComponentModel;

namespace Zambo.Utility
{
	/// <summary>
	/// <c>Enum</c> de valor padrão para <c>DropDownList</c>
	/// </summary>
	public enum ValorDefaultCombo
	{

		/// <summary>
		/// Opção sem valor
		/// </summary>
		[Description("")]
		EmBranco,

		/// <summary>
		/// Opção Selecionar
		/// </summary>
		[Description("Selecionar")]
		Selecionar,

		/// <summary>
		/// Opção Todos
		/// </summary>
		[Description("Todos")]
		Todos,

		/// <summary>
		/// Opção Todas
		/// </summary>
		[Description("Todas")]
		Todas,

		/// <summary>
		/// Opção Todos / Todas
		/// </summary>
		[Description("Todos / Todas")]
		TodosTodas,

		/// <summary>
		/// Opção Selecione
		/// </summary>
		[Description("Selecione")]
		Selecione,
		Nenhum,
	}
}

