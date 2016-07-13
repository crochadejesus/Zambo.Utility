//
//  ListElementEquality.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 07/07/2016 23:02
//
//  Copyright (c) 2016 Zambo Tecnologia Ltda
//

using System.Collections.Generic;

namespace Zambo.Utility
{
	/// <summary>
	/// Verifica se o conteúdo de duas listas são exatamente iguais
	/// http://www.dotnetperls.com/list-equals
	/// </summary>
	public class ListElementEquality
	{
		/// <summary>
		/// Verifica se o conteúdo de duas listas são exatamente iguais
		/// </summary>
		/// <typeparam name="T"> Assumirá o tipo da lista que for passada </typeparam>
		/// <param name="a"> Lista 1 que deve ser comparada </param>
		/// <param name="b"> Lista 2 que deve ser comparada </param>
		/// <returns> Verdadeiro caso sejam iguais, falso caso sejam diferentes </returns>
		public static bool UnorderedEqual<T>(ICollection<T> a, ICollection<T> b)
		{
			// 1
			// Require that the counts are equal
			if (a.Count != b.Count)
			{
				return false;
			}

			// 2
			// Initialize new Dictionary of the type
			Dictionary<T, int> d = new Dictionary<T, int>();

			// 3
			// Add each key's frequency from collection A to the Dictionary
			foreach (T item in a)
			{
				int c;
				if (d.TryGetValue(item, out c))
				{
					d[item] = c + 1;
				}
				else
				{
					d.Add(item, 1);
				}
			}

			// 4
			// Add each key's frequency from collection B to the Dictionary
			// Return early if we detect a mismatch
			foreach (T item in b)
			{
				int c;
				if (d.TryGetValue(item, out c))
				{
					if (c == 0)
					{
						return false;
					}
					else
					{
						d[item] = c - 1;
					}
				}
				else
				{
					// Not in dictionary
					return false;
				}
			}

			// 5
			// Verify that all frequencies are zero
			foreach (int v in d.Values)
			{
				if (v != 0)
				{
					return false;
				}
			}

			// 6
			// We know the collections are equal
			return true;
		}
	}
}

