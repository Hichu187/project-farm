using System.Text;

namespace VeQue.Logic
{
    // Ngoại hình + tên nhân vật người chơi chọn ở màn tạo nhân vật. Kiểu tóc và màu tóc là id do View định nghĩa
    // (Logic chỉ lưu, View tự về mặc định nếu id không còn).
    public sealed class CharacterLook
    {
        public const int MaxNameLength = 16;
        public const string DefaultHairStyle = "shorthair";
        // "Gốc" = giữ màu tóc nguyên bản của art.
        public const string DefaultHairColor = "goc";

        // false = chưa qua màn tạo nhân vật (game mới hoặc save cũ) → View hiện màn tạo nhân vật.
        public bool Created;
        public string Name = "";
        public string HairStyle = DefaultHairStyle;
        public string HairColor = DefaultHairColor;

        // Chuẩn hóa tên: bỏ ký tự điều khiển, gộp khoảng trắng, cắt khoảng trắng 2 đầu.
        // Hợp lệ khi còn 1..MaxNameLength ký tự.
        public static bool TryNormalizeName(string raw, out string name)
        {
            name = null;
            if (raw == null)
                return false;

            var builder = new StringBuilder(raw.Length);
            bool pendingSpace = false;
            foreach (char c in raw)
            {
                if (char.IsWhiteSpace(c))
                {
                    pendingSpace = builder.Length > 0;
                    continue;
                }
                if (char.IsControl(c) || char.IsSurrogate(c))
                    continue;

                if (pendingSpace)
                    builder.Append(' ');
                pendingSpace = false;
                builder.Append(c);
            }

            // Chuỗi gõ kiểu tổ hợp (chữ + dấu rời) chuyển về dạng dựng sẵn để đếm độ dài đúng.
            string result = builder.ToString();
            try
            {
                result = result.Normalize(NormalizationForm.FormC);
            }
            catch (System.Exception)
            {
                // Nền tảng thiếu bảng chuẩn hóa: giữ nguyên chuỗi.
            }
            if (result.Length < 1 || result.Length > MaxNameLength)
                return false;

            name = result;
            return true;
        }
    }
}
