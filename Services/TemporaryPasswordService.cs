using System.Security.Cryptography;

namespace ObreshkovLibrary.Services
{
    public class TemporaryPasswordService
    {
        public string Generate()
        {
            var number = RandomNumberGenerator.GetInt32(0, 1000000);
            return number.ToString("D6");
        }
    }
}