//
//  Cache.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 07/07/2016 22:48
//
//  Copyright (c) 2016 Zambo Tecnologia Ltda
//

using System;
using System.Runtime.Caching;

namespace Zambo.Utility
{
	public class Cache
	{
		private static ObjectCache cache = MemoryCache.Default;

		public static void Adicionar(string nomeCache, object item, int qtdHoras)
		{
			cache.Set(nomeCache, item, DateTimeOffset.Now.AddHours(qtdHoras));
		}

		public static void Adicionar(string nomeCache, object item)
		{
			cache.Set(nomeCache, item, DateTimeOffset.Now.AddDays(1));
		}

		public static T Obter<T>(string nomeCache)
		{
			return (T)cache.Get(nomeCache);
		}

		public static void Remover(string nomeCache)
		{
			cache.Remove(nomeCache);
		}
	}
}

