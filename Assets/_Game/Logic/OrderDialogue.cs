using System;
using System.Collections.Generic;
using System.Text;

namespace VeQue.Logic
{
    // Đơn hàng có thoại (Đợt 2 Phần 11, thiết kế Docs/thiet-ke-dot-2.md mục 2).

    // Một người làng đặt đơn.
    public sealed class NpcDef
    {
        public string Id { get; }
        public string DisplayName { get; }
        // Cách tự xưng trong câu ({title}), viết thường: "bà", "chú", "em"… (Format tự viết hoa đầu câu).
        public string Title { get; }
        public IReadOnlyList<string> PreferredCrops { get; }
        // Trọng số được chọn cho đơn nông sản / đơn cá (0 = không đặt loại đơn đó).
        public int Weight { get; }
        public int FishWeight { get; }

        public NpcDef(string id, string displayName, string title, IReadOnlyList<string> preferredCrops, int weight, int fishWeight = 0)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Người làng cần id.", nameof(id));
            if (weight < 0 || fishWeight < 0)
                throw new ArgumentOutOfRangeException(nameof(weight));
            Id = id;
            DisplayName = string.IsNullOrEmpty(displayName) ? id : displayName;
            Title = title ?? "";
            PreferredCrops = new List<string>(preferredCrops ?? Array.Empty<string>()).AsReadOnly();
            Weight = weight;
            FishWeight = fishWeight;
        }

        public bool Prefers(string cropId)
        {
            foreach (string crop in PreferredCrops)
            {
                if (crop == cropId)
                    return true;
            }
            return false;
        }
    }

    public enum OrderLineKind
    {
        Request = 0,
        Urgent = 1,
        Fish = 2,
        Thanks = 3,
    }

    // Một câu thoại. NpcId null = câu dùng chung. CropTag khác null: chỉ dùng khi món đầu của đơn là cây / cá này.
    // Chỗ trống: {title} {qty} {crop} {dish}.
    public sealed class OrderLineDef
    {
        public string NpcId { get; }
        public OrderLineKind Kind { get; }
        public string Text { get; }
        public string CropTag { get; }

        public bool NeedsDish => Text.IndexOf("{dish}", StringComparison.Ordinal) >= 0;

        public OrderLineDef(string npcId, OrderLineKind kind, string text, string cropTag = null)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Câu thoại rỗng.", nameof(text));
            NpcId = string.IsNullOrEmpty(npcId) ? null : npcId;
            Kind = kind;
            Text = text;
            CropTag = string.IsNullOrEmpty(cropTag) ? null : cropTag;
        }
    }

    // Người làng + câu thoại + món ăn theo cây. Empty = không có thoại (đơn không gắn người).
    public sealed class OrderDialogue
    {
        public static readonly OrderDialogue Empty = new OrderDialogue(null, null, null);

        private readonly Dictionary<string, NpcDef> _npcs = new Dictionary<string, NpcDef>();
        private readonly Dictionary<string, IReadOnlyList<string>> _dishes = new Dictionary<string, IReadOnlyList<string>>();

        public IReadOnlyList<NpcDef> Npcs { get; }
        public IReadOnlyList<OrderLineDef> Lines { get; }

        public OrderDialogue(IEnumerable<NpcDef> npcs, IEnumerable<OrderLineDef> lines,
            IEnumerable<KeyValuePair<string, IReadOnlyList<string>>> dishes)
        {
            var npcList = new List<NpcDef>();
            if (npcs != null)
            {
                foreach (NpcDef npc in npcs)
                {
                    if (npc == null)
                        throw new ArgumentException("Có người làng null.");
                    if (_npcs.ContainsKey(npc.Id))
                        throw new ArgumentException($"Trùng id người làng '{npc.Id}'.");
                    _npcs.Add(npc.Id, npc);
                    npcList.Add(npc);
                }
            }
            var lineList = new List<OrderLineDef>();
            if (lines != null)
            {
                foreach (OrderLineDef line in lines)
                {
                    if (line == null)
                        throw new ArgumentException("Có câu thoại null.");
                    if (line.NpcId != null && !_npcs.ContainsKey(line.NpcId))
                        throw new ArgumentException($"Câu thoại của người không có: '{line.NpcId}'.");
                    lineList.Add(line);
                }
            }
            if (dishes != null)
            {
                foreach (KeyValuePair<string, IReadOnlyList<string>> pair in dishes)
                {
                    if (string.IsNullOrEmpty(pair.Key) || pair.Value == null || pair.Value.Count == 0)
                        continue;
                    _dishes[pair.Key] = new List<string>(pair.Value).AsReadOnly();
                }
            }
            Npcs = npcList.AsReadOnly();
            Lines = lineList.AsReadOnly();
        }

        public bool TryGetNpc(string id, out NpcDef npc)
        {
            if (id == null)
            {
                npc = null;
                return false;
            }
            return _npcs.TryGetValue(id, out npc);
        }

        public bool TryGetLine(int index, out OrderLineDef line)
        {
            line = index >= 0 && index < Lines.Count ? Lines[index] : null;
            return line != null;
        }

        public IReadOnlyList<string> GetDishes(string cropId)
            => cropId != null && _dishes.TryGetValue(cropId, out IReadOnlyList<string> list) ? list : Array.Empty<string>();

        // Món theo seed của đơn (null nếu cây không có món).
        public string PickDish(string cropId, uint seed)
        {
            IReadOnlyList<string> dishes = GetDishes(cropId);
            return dishes.Count == 0 ? null : dishes[(int)(seed % (uint)dishes.Count)];
        }

        // Điền chỗ trống rồi viết hoa chữ đầu câu. View đưa tên cây / cá viết thường ("rau muống", "cá rô");
        // title viết thường ("bà", "em") nên giữa câu vẫn đúng.
        public static string Format(string text, string title, int qty, string itemName, string dish)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            string result = text
                .Replace("{title}", title ?? "")
                .Replace("{qty}", qty.ToString())
                .Replace("{crop}", itemName ?? "")
                .Replace("{dish}", dish ?? itemName ?? "");
            return CapitalizeSentences(result);
        }

        // Viết hoa chữ cái đầu câu và chữ cái đầu sau ". " "! " "? ".
        private static string CapitalizeSentences(string text)
        {
            var chars = text.ToCharArray();
            bool start = true;
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (start && char.IsLetter(c))
                {
                    chars[i] = char.ToUpperInvariant(c);
                    start = false;
                }
                else if (c == '.' || c == '!' || c == '?')
                {
                    start = true;
                }
                else if (!char.IsWhiteSpace(c))
                {
                    start = false;
                }
            }
            return new string(chars);
        }

        // Câu hợp lệ: đúng loại, của người này hoặc dùng chung, khớp CropTag, đủ món nếu câu cần {dish}.
        internal bool IsCandidate(OrderLineDef line, OrderLineKind kind, string npcId, string itemId, bool hasDish)
        {
            if (line.Kind != kind)
                return false;
            if (line.NpcId != null && line.NpcId != npcId)
                return false;
            if (line.CropTag != null && line.CropTag != itemId)
                return false;
            return !line.NeedsDish || hasDish;
        }

        // Chọn câu theo random; tránh câu exclude nếu còn câu khác. -1 nếu không có câu nào.
        internal int PickLine(OrderLineKind kind, string npcId, string itemId, bool hasDish, int exclude, ref uint random)
        {
            int count = 0;
            int other = 0;
            for (int i = 0; i < Lines.Count; i++)
            {
                if (!IsCandidate(Lines[i], kind, npcId, itemId, hasDish))
                    continue;
                count++;
                if (i != exclude)
                    other++;
            }
            if (count == 0)
                return -1;
            bool skipExclude = other > 0 && other < count;
            int pick = SeededRandom.Range(ref random, 0, skipExclude ? other : count);
            for (int i = 0; i < Lines.Count; i++)
            {
                if (!IsCandidate(Lines[i], kind, npcId, itemId, hasDish) || (skipExclude && i == exclude))
                    continue;
                if (pick-- == 0)
                    return i;
            }
            return -1;
        }

        // ---------- Đọc order_lines.csv ----------

        // Dòng đầu là tiêu đề (npcId,kind,text,cropTag). Trường có dấu phẩy / ngoặc kép thì bọc "…", "" = một dấu ".
        // Dòng trống hoặc bắt đầu bằng # bỏ qua. Kiểu câu: Request / Urgent / Fish / Thanks (không phân biệt hoa thường).
        public static List<OrderLineDef> ParseLinesCsv(string csv)
        {
            var result = new List<OrderLineDef>();
            if (string.IsNullOrEmpty(csv))
                return result;
            List<List<string>> rows = ParseCsv(csv);
            for (int r = 1; r < rows.Count; r++)
            {
                List<string> row = rows[r];
                if (row.Count == 0 || (row.Count == 1 && row[0].Trim().Length == 0))
                    continue;
                string first = row[0].Trim();
                if (first.StartsWith("#", StringComparison.Ordinal))
                    continue;
                if (row.Count < 3)
                    throw new FormatException($"order_lines.csv dòng {r + 1}: cần ít nhất 3 cột.");
                if (!Enum.TryParse(row[1].Trim(), true, out OrderLineKind kind) || !Enum.IsDefined(typeof(OrderLineKind), kind))
                    throw new FormatException($"order_lines.csv dòng {r + 1}: loại câu lạ '{row[1]}'.");
                string tag = row.Count > 3 ? row[3].Trim() : null;
                result.Add(new OrderLineDef(first, kind, row[2].Trim(), tag));
            }
            return result;
        }

        private static List<List<string>> ParseCsv(string csv)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;
            int i = 0;
            if (csv.Length > 0 && csv[0] == '﻿')
                i = 1;
            for (; i < csv.Length; i++)
            {
                char c = csv[i];
                if (quoted)
                {
                    if (c == '"')
                    {
                        if (i + 1 < csv.Length && csv[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else
                        {
                            quoted = false;
                        }
                    }
                    else
                    {
                        field.Append(c);
                    }
                    continue;
                }
                if (c == '"' && field.Length == 0)
                {
                    quoted = true;
                }
                else if (c == ',')
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if (c == '\n' || c == '\r')
                {
                    if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                        i++;
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = new List<string>();
                }
                else
                {
                    field.Append(c);
                }
            }
            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString());
                rows.Add(row);
            }
            return rows;
        }
    }
}
