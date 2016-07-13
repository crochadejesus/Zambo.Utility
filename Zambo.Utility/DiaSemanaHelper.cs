//
//  DiaSemanaHelper.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 07/07/2016 22:52
//
//  Copyright (c) 2016 Zambo Tecnologia Ltda
//

using System;
using System.Collections.Generic;

namespace Zambo.Utility
{
	public static class DiaSemanaHelper
	{
		public static IEnumerable<DayOfWeek> GetDiaSemanaEmIngles(IEnumerable<string> diasSemanaPortugues)
		{
			var lista = new List<DayOfWeek>();

			foreach (string dia in diasSemanaPortugues)
			{
				lista.Add(GetDiaSemanaEmIngles(dia));
			}

			return lista;
		}

		public static DayOfWeek GetDiaSemanaEmIngles(string diaSemanaPortugues)
		{
			DayOfWeek weekDay = new DayOfWeek();
			switch (diaSemanaPortugues)
			{
				case "Domingo":
					weekDay = DayOfWeek.Sunday;
					break;
				case "Segunda":
					weekDay = DayOfWeek.Monday;
					break;
				case "Terça":
					weekDay = DayOfWeek.Tuesday;
					break;
				case "Quarta":
					weekDay = DayOfWeek.Wednesday;
					break;
				case "Quinta":
					weekDay = DayOfWeek.Thursday;
					break;
				case "Sexta":
					weekDay = DayOfWeek.Friday;
					break;
				case "Sábado":
					weekDay = DayOfWeek.Saturday;
					break;
			}
			return weekDay;
		}

		public static string GetDiaSemanaEmPortugues(DayOfWeek weekDay)
		{
			string diaSemana = string.Empty;
			switch (weekDay)
			{
				case DayOfWeek.Sunday:
					diaSemana = "Domingo";
					break;
				case DayOfWeek.Monday:
					diaSemana = "Segunda";
					break;
				case DayOfWeek.Tuesday:
					diaSemana = "Terça";
					break;
				case DayOfWeek.Wednesday:
					diaSemana = "Quarta";
					break;
				case DayOfWeek.Thursday:
					diaSemana = "Quinta";
					break;
				case DayOfWeek.Friday:
					diaSemana = "Sexta";
					break;
				case DayOfWeek.Saturday:
					diaSemana = "Sábado";
					break;
			}
			return diaSemana;
		}
	}
}

