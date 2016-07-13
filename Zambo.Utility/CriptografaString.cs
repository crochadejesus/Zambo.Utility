namespace Zambo.Utility
{
	public class CriptografaString
	{
		// Arbitrary key and iv vector.
		// You will want to generate (and protect) your own when using encryption.
		const string actionKey = "EA81AA1D5FC1EC53E84F30AA746139EEBAFF8A9B76638895";
		const string actionIv = "87AF7EA221F3FFF5";

		System.Security.Cryptography.TripleDESCryptoServiceProvider des3;

		public CriptografaString()
		{
			this.des3 = new System.Security.Cryptography.TripleDESCryptoServiceProvider();
			this.des3.Mode = System.Security.Cryptography.CipherMode.CBC;
		}

		static System.Byte[] ConvertStringToByArray(string s)
		{
			return (new System.Text.UnicodeEncoding()).GetBytes(s);
		}

		public static string MD5(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return null;
			}
			System.Byte[] toHash = ConvertStringToByArray(s);
			byte[] hashValue = ((System.Security.Cryptography.HashAlgorithm)System.Security.Cryptography.CryptoConfig.CreateFromName("MD5")).ComputeHash(toHash);
			return System.BitConverter.ToString(hashValue);
		}

		public static string Base64Encode(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return string.Empty;
			}
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(key);
			return System.Convert.ToBase64String(buffer);
		}

		public static string Base64Decode(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return "";
			}
			byte[] buffer = System.Convert.FromBase64String(key);
			return System.Text.Encoding.UTF8.GetString(buffer);
		}

		public string GenerateKey()
		{
			this.des3.GenerateKey();
			return BytesToHex(this.des3.Key);
		}

		public string GenerateIV()
		{
			this.des3.GenerateIV();
			return BytesToHex(this.des3.IV);
		}

		byte[] HexToBytes(string hex)
		{
			byte[] bytes = new byte[hex.Length / 2];
			for (int i = 0; i < hex.Length / 2; i++)
			{
				string code = hex.Substring(i * 2, 2);
				bytes[i] = byte.Parse(code, System.Globalization.NumberStyles.HexNumber);
			}
			return bytes;
		}

		string BytesToHex(byte[] bytes)
		{
			System.Text.StringBuilder hex = new System.Text.StringBuilder();
			for (int i = 0; i < bytes.Length; i++)
			{
				hex.AppendFormat("{0:X2}", bytes[i]);
			}
			return hex.ToString();
		}

		public string Encrypt(string data, string key, string iv)
		{
			byte[] bdata = System.Text.Encoding.UTF8.GetBytes(data);
			byte[] bkey = this.HexToBytes(key);
			byte[] biv = this.HexToBytes(iv);

			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			System.Security.Cryptography.CryptoStream encStream = new System.Security.Cryptography.CryptoStream(stream,
				this.des3.CreateEncryptor(bkey, biv), System.Security.Cryptography.CryptoStreamMode.Write);

			encStream.Write(bdata, 0, bdata.Length);
			encStream.FlushFinalBlock();
			encStream.Close();

			return BytesToHex(stream.ToArray());
		}

		public string Decrypt(string data, string key, string iv)
		{
			byte[] bdata = this.HexToBytes(data);
			byte[] bkey = this.HexToBytes(key);
			byte[] biv = this.HexToBytes(iv);

			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			System.Security.Cryptography.CryptoStream encStream = new System.Security.Cryptography.CryptoStream(stream,
				this.des3.CreateDecryptor(bkey, biv), System.Security.Cryptography.CryptoStreamMode.Write);

			encStream.Write(bdata, 0, bdata.Length);
			encStream.FlushFinalBlock();
			encStream.Close();

			return System.Text.Encoding.UTF8.GetString(stream.ToArray());
		}

		public string ActionEncrypt(string data)
		{
			return Encrypt(data, actionKey, actionIv);
		}

		public string ActionDecrypt(string data)
		{
			return Decrypt(data, actionKey, actionIv);
		}
	}
}

