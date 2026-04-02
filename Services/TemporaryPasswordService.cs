using System.Security.Cryptography;

namespace ObreshkovLibrary.Services
{
    public class TemporaryPasswordService
    {
        private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string Lower = "abcdefghijkmnopqrstuvwxyz";
        private const string Digits = "23456789";
        private const string Symbols = "!@#$%";

        public string Generate()
        {
            var all = Upper + Lower + Digits + Symbols;

            var chars = new List<char>
            {
                Upper[RandomNumberGenerator.GetInt32(Upper.Length)],
                Lower[RandomNumberGenerator.GetInt32(Lower.Length)],
                Digits[RandomNumberGenerator.GetInt32(Digits.Length)],
                Symbols[RandomNumberGenerator.GetInt32(Symbols.Length)]
            };

            for (int i = chars.Count; i < 10; i++)
            {
                chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
            }

            return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
        }
    }
}