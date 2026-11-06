using System;
using System.Text;

namespace VeQue.Logic
{
    // Mã sao lưu dạng "VQ1-<base64 của JSON>-<checksum>".
    // Checksum chỉ phát hiện lỗi khi chép mã, không ngăn người chơi cố tình sửa.
    public static class BackupCode
    {
        private const string Prefix = "VQ1-";
        private const char Separator = '-';

        public static string Encode(string json)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            return Prefix + Convert.ToBase64String(bytes) + Separator + Checksum(bytes);
        }

        public static bool TryDecode(string code, out string json)
        {
            json = null;
            if (string.IsNullOrWhiteSpace(code))
                return false;

            string clean = RemoveWhitespace(code);
            int end = clean.LastIndexOf(Separator);
            if (!clean.StartsWith(Prefix, StringComparison.Ordinal) || end < Prefix.Length)
                return false;

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(clean.Substring(Prefix.Length, end - Prefix.Length));
            }
            catch (FormatException)
            {
                return false;
            }

            if (clean.Substring(end + 1) != Checksum(bytes))
                return false;

            json = Encoding.UTF8.GetString(bytes);
            return true;
        }

        // Chép mã trên điện thoại hay dính khoảng trắng hoặc xuống dòng.
        private static string RemoveWhitespace(string text)
        {
            var builder = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (!char.IsWhiteSpace(c))
                    builder.Append(c);
            }
            return builder.ToString();
        }

        // FNV-1a 32 bit.
        private static string Checksum(byte[] bytes)
        {
            uint hash = 2166136261;
            foreach (byte b in bytes)
                hash = unchecked((hash ^ b) * 16777619);
            return hash.ToString("x8");
        }
    }
}
