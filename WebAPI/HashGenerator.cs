using System.Security.Cryptography;
using System.Text;

namespace WebAPI
{
    public static class HashGenerator
    {
        public static string Generate(string data, string salt)
        {
            string result = "";
            var bytes = Encoding.ASCII.GetBytes(data+salt);
            var sha = new SHA512Managed();
            var hash = sha.ComputeHash(bytes);
            foreach (byte b in hash)
                result += b.ToString("x2");
            return result;
        }

        public static string GetSalt(int length)
        {
            string result = "";
            var salt = new byte[length];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }
            foreach(byte b in salt)
                result+= b.ToString("x2");
            return result;
        }
    }
}
