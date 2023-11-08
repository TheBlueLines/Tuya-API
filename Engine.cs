using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace TTMC.Tuya
{
	internal class Engine
	{
		private static SHA256 sha = SHA256.Create();
		public static string Encrypt(string message, string key)
		{
			HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
			return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(message)));
		}
		public static string GetHash(string value)
		{
			return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).ToLower();
		}
		public static JsonSerializerOptions jsonSerializerOptions
		{
			get
			{
				return new()
				{
					WriteIndented = false,
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
					Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
				};
			}
		}
		public static Base ToBase(string text)
		{
			if (text.StartsWith('{') && text.EndsWith('}'))
			{
				Base? item = JsonSerializer.Deserialize<Base>(text);
				if (item != null)
				{
					if (item.success)
					{
						return item;
					}
					if (!string.IsNullOrEmpty(item.msg))
					{
						throw new(item.msg);
					}
				}
			}
			throw new(text);
		}
	}
}