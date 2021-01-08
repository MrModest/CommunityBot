using System.Linq;
using System.Text;

namespace CommunityBot.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        /// String Null or Empty or WhiteSpace
        /// </summary>
        public static bool IsBlank(this string? value)
        {
            return string.IsNullOrWhiteSpace(value?.Trim());
        }

        /// <summary>
        /// String not Null or Empty or WhiteSpace
        /// </summary>
        public static bool IsNotBlank(this string? value)
        {
            return !string.IsNullOrWhiteSpace(value?.Trim());
        }
        
        public static string CreateMd5(string input)
        {
            using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            return string.Join("", hashBytes.Select(b => b.ToString("X2")));
        }
    }
}