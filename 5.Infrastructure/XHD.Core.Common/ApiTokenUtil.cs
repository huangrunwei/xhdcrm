using System;
using System.Security.Cryptography;
using System.Text;

namespace XHD.Core.Common
{
    /// <summary>
    /// API / MCP 长期令牌的生成与哈希。数据库只保存哈希。
    /// </summary>
    public static class ApiTokenUtil
    {
        public const string Prefix = "xhdcrm_";

        public static string Generate()
        {
            return Prefix + Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        }

        public static string Hash(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim()));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        public static string DisplayPrefix(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;
            var value = token.Trim();
            return value.Length <= 18 ? value : value[..18];
        }

        public static bool IsApiToken(string token)
        {
            return !string.IsNullOrWhiteSpace(token)
                   && token.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
