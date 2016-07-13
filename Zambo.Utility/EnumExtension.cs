//
//  EnumExtension.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 07/07/2016 22:34
//
//  Copyright (c) 2016 Zambo Tecnologia Ltda
//

using System;
using System.ComponentModel;
using System.Reflection;

namespace Zambo.Utility
{
	public static class EnumExtension
	{
		/// <summary>
		/// Recupera a descrição de uma enumeration de acordo com a Anotation [Description] de cada item da enum.
		/// </summary>
		/// <param name="value">Enum que será retornada a descrição.</param>
		/// <returns>Descrição da enum.</returns>
		public static string RecuperarDescricaoEnum(this Enum value)
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());

			DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

			if (attributes != null && attributes.Length > 0)
				return attributes[0].Description;
			else
				return value.ToString();
		}

		public static int RecuperarValorEnumOld(this Enum value)
		{
			return Convert.ToInt32(value);
		}
	}
}

