using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
    }
}
