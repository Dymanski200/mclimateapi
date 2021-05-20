using System;

namespace WebAPI
{
    public static class CodeGenerator
    {
        public static string Generate(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = "";
            for (int i = 0; i < length; i++)
            {
                result += chars[random.Next(chars.Length)];
            }
            return result;
        }
    }
}
