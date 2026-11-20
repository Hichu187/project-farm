using System;
using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Đọc data đơn hàng có thoại (Đợt 2 Phần 11): npcs.json, order_lines.csv, dishes.json → OrderDialogue (Logic).
    // File thiếu thì phần đó trống; file sai cấu trúc thì báo lỗi rõ ràng (sửa data phải thấy ngay).
    public static class OrderDialogueLoader
    {
        [Serializable]
        private sealed class NpcFile
        {
            public NpcRow[] npcs;
        }

        [Serializable]
        private sealed class NpcRow
        {
            public string id;
            public string name;
            public string title;
            public string role;
            public string[] crops;
            public int weight;
            public int fishWeight;
        }

        [Serializable]
        private sealed class DishFile
        {
            public DishRow[] dishes;
        }

        [Serializable]
        private sealed class DishRow
        {
            public string crop;
            public string[] names;
        }

        public static OrderDialogue Load(TextAsset npcsJson, TextAsset linesCsv, TextAsset dishesJson)
        {
            var npcs = new List<NpcDef>();
            if (npcsJson != null)
            {
                NpcFile file = JsonUtility.FromJson<NpcFile>(npcsJson.text);
                if (file?.npcs == null)
                    throw new FormatException($"{npcsJson.name}: không đọc được danh sách \"npcs\".");
                foreach (NpcRow row in file.npcs)
                {
                    if (row == null)
                        continue;
                    npcs.Add(new NpcDef(row.id, row.name, row.title, row.crops, row.weight, row.fishWeight));
                }
            }

            List<OrderLineDef> lines = linesCsv != null ? OrderDialogue.ParseLinesCsv(linesCsv.text) : new List<OrderLineDef>();

            var dishes = new List<KeyValuePair<string, IReadOnlyList<string>>>();
            if (dishesJson != null)
            {
                DishFile file = JsonUtility.FromJson<DishFile>(dishesJson.text);
                if (file?.dishes == null)
                    throw new FormatException($"{dishesJson.name}: không đọc được danh sách \"dishes\".");
                foreach (DishRow row in file.dishes)
                {
                    if (row != null && !string.IsNullOrEmpty(row.crop) && row.names != null)
                        dishes.Add(new KeyValuePair<string, IReadOnlyList<string>>(row.crop, row.names));
                }
            }
            return new OrderDialogue(npcs, lines, dishes);
        }
    }
}
