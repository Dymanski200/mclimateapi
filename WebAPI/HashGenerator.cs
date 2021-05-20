using System.Security.Cryptography;
using System.Text;

namespace WebAPI
{
    public static class HashGenerator
    {
        public static string Generate(string data)
        {
            string result = "";
            var bytes = Encoding.ASCII.GetBytes(data);
            var sha = new SHA256Managed();
            var hash = sha.ComputeHash(bytes);
            foreach (byte b in hash)
                result += b.ToString("x2");
            return result;
        }
    }
}
