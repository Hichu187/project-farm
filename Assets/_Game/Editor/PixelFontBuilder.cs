using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Font: tạo font pixel tiếng Việt.
    // Tạo TMP font asset tĩnh từ FatPix VN (NgnDang, itch.io; miễn phí cả thương mại, không bán lại font, nên ghi công).
    //
    // Không vẽ lại nét CFF: nét vector của FatPix được ghép theo từng chữ với tỉ lệ riêng (1 pixel = 43,2–45,1 đơn vị,
    // lệch nửa pixel), vẽ lại ra atlas thì pixel dày mỏng không đều. Thay vào đó đọc bảng SVG của font — mỗi chữ là
    // các ô <rect> 1×1 đúng lưới thiết kế — và chép thẳng vào atlas: 1 pixel thiết kế = 1 texel.
    // Khoảng cách chữ đặt đều: advance = bề rộng chữ + 1 pixel; chữ không có SVG (dấu cách) lấy advance từ hmtx.
    // Atlas Alpha8, lọc Point, Static (bản build không mang file .otf 1,9 MB). Ký tự font không có (· → …)
    // lấy từ font mặc định của TMP. Chạy lại sẽ tạo lại từ đầu; material viền giữ file cũ, chỉ gắn atlas mới.
    public static class PixelFontBuilder
    {
        public const string FontPath = "Assets/_Game/Art/Font/FatPix-VN-SVG.otf";
        public const string AssetPath = "Assets/_Game/Art/Font/FatPix Pixel.asset";
        // Material có viền pixel (shader VeQue/TMP Pixel Outline), dùng chung atlas với font asset.
        public const string OutlineMaterialPath = "Assets/_Game/Art/Font/FatPix Outline.mat";

        // Số pixel thiết kế ứng với "point size" của font asset: cỡ chữ TMP = N × PointSize / 3 thì mỗi pixel
        // thiết kế = N pixel màn hình khi Canvas ×3 (UiBuilder.PixelFontSizeFor).
        public const int PointSize = 24;

        private const string OutlineShaderName = "VeQue/TMP Pixel Outline";
        // Viền 1 pixel thiết kế = 1 texel.
        private const float OutlineTexels = 1f;
        // Khoảng trống quanh mỗi chữ trong atlas: > viền + 1 để viền không lấy nhầm chữ bên cạnh.
        private const int AtlasPadding = 3;
        private const int AtlasSize = 512;
        private const int LetterSpacing = 1;
        private const string Title = "Font pixel";

        private static readonly Regex GroupRegex = new Regex(
            "<g transform=\"scale\\(([-\\d.]+)(?:\\s*,\\s*([-\\d.]+))?\\)\\s*translate\\(([-\\d.]+)\\s*,\\s*([-\\d.]+)\\)\"\\s*>(.*?)</g>",
            RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex RectRegex = new Regex(
            "<rect x=\"(-?\\d+)\" y=\"(-?\\d+)\" width=\"(\\d+)\" height=\"(\\d+)\"", RegexOptions.Compiled);
        private static readonly Regex PathRegex = new Regex(
            "<path d=\"M\\s*(-?[\\d.]+)[\\s,]+(-?[\\d.]+)", RegexOptions.Compiled);

        // Chữ đã đọc từ SVG: lưới pixel (x sang phải, y lên trên, gốc = góc dưới-trái của khung chữ).
        private sealed class PixelGlyph
        {
            public uint Index;
            public int Width;
            public int Height;
            // Mép trên khung chữ, tính bằng pixel trên đường chân chữ.
            public int Top;
            public int Advance;
            public bool[,] Pixels;
            public GlyphRect Rect;
        }

        [MenuItem("Về Quê/Font: tạo font pixel tiếng Việt")]
        private static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null || !File.Exists(FontPath))
            {
                EditorUtility.DisplayDialog(Title, $"Không thấy {FontPath}.", "OK");
                return;
            }

            OpenTypeReader reader;
            try
            {
                reader = new OpenTypeReader(File.ReadAllBytes(FontPath));
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog(Title, "Không đọc được font: " + e.Message, "OK");
                return;
            }

            // Mã ký tự → chữ pixel. Nhiều ký tự có thể dùng chung một chữ.
            var glyphs = new Dictionary<uint, PixelGlyph>();
            var characters = new SortedDictionary<uint, uint>();
            float pixelUnits = 0f;
            int pixelUnitsCount = 0;
            foreach (KeyValuePair<uint, uint> pair in reader.CharacterToGlyph)
            {
                uint glyphIndex = pair.Value;
                if (!glyphs.ContainsKey(glyphIndex))
                {
                    PixelGlyph glyph = ReadGlyph(reader, glyphIndex, out float scale);
                    if (glyph == null)
                        continue;
                    glyphs[glyphIndex] = glyph;
                    if (scale > 0f)
                    {
                        pixelUnits += scale;
                        pixelUnitsCount++;
                    }
                }
                characters[pair.Key] = glyphIndex;
            }

            // Chữ không có SVG (dấu cách…): advance từ hmtx theo cỡ pixel trung bình.
            float averagePixel = pixelUnitsCount > 0 ? pixelUnits / pixelUnitsCount : 43.78f;
            foreach (KeyValuePair<uint, uint> pair in characters)
            {
                PixelGlyph glyph = glyphs[pair.Value];
                if (glyph.Advance == 0)
                    glyph.Advance = Mathf.Max(1, Mathf.RoundToInt(reader.GetAdvance(pair.Value) / averagePixel));
            }

            string missingVietnamese = MissingVietnamese(characters);
            if (missingVietnamese.Length > 0)
            {
                EditorUtility.DisplayDialog(Title, "Font thiếu chữ tiếng Việt: " + missingVietnamese, "OK");
                return;
            }

            var unique = new List<PixelGlyph>(glyphs.Values);
            if (!Pack(unique))
            {
                EditorUtility.DisplayDialog(Title, $"Atlas {AtlasSize}×{AtlasSize} không đủ chỗ cho {unique.Count} chữ.", "OK");
                return;
            }

            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(font, PointSize, AtlasPadding,
                GlyphRenderMode.RASTER, AtlasSize, AtlasSize, AtlasPopulationMode.Dynamic, false);
            if (asset == null)
            {
                EditorUtility.DisplayDialog(Title, "Không tạo được font asset (kiểm tra Include Font Data của file font).", "OK");
                return;
            }

            Texture2D atlas = asset.atlasTexture;
            DrawAtlas(atlas, unique);
            asset.faceInfo = BuildFaceInfo(asset.faceInfo, glyphs, characters);

            asset.glyphTable.Clear();
            asset.characterTable.Clear();
            foreach (PixelGlyph glyph in unique)
            {
                var metrics = new GlyphMetrics(glyph.Width, glyph.Height, 0f, glyph.Top, glyph.Advance);
                asset.glyphTable.Add(new Glyph(glyph.Index, metrics, glyph.Rect, 1f, 0));
            }
            var glyphByIndex = new Dictionary<uint, Glyph>();
            foreach (Glyph glyph in asset.glyphTable)
                glyphByIndex[glyph.index] = glyph;
            foreach (KeyValuePair<uint, uint> pair in characters)
                asset.characterTable.Add(new TMP_Character(pair.Key, asset, glyphByIndex[pair.Value]));
            asset.ReadFontAssetDefinition();

            if (AssetDatabase.LoadMainAssetAtPath(AssetPath) != null)
                AssetDatabase.DeleteAsset(AssetPath);

            asset.name = "FatPix Pixel";
            atlas.name = "FatPix Pixel Atlas";
            atlas.filterMode = FilterMode.Point;
            asset.material.name = "FatPix Pixel Material";
            if (TMP_Settings.defaultFontAsset != null)
                asset.fallbackFontAssetTable = new List<TMP_FontAsset> { TMP_Settings.defaultFontAsset };

            AssetDatabase.CreateAsset(asset, AssetPath);
            AssetDatabase.AddObjectToAsset(atlas, asset);
            AssetDatabase.AddObjectToAsset(asset.material, asset);

            // Tĩnh: bỏ tham chiếu tới file font nên bản build không mang theo, không sinh glyph lúc chạy.
            asset.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetPath);

            string outlineNote = BuildOutlineMaterial(atlas);
            EditorUtility.DisplayDialog(Title,
                $"Đã tạo {AssetPath}: {characters.Count} ký tự, {unique.Count} chữ, 1 pixel thiết kế = 1 texel.\n"
                + $"{outlineNote}\nChạy lại Về Quê → Tạo UI để dùng.", "OK");
            EditorGUIUtility.PingObject(asset);
        }

        // Đọc tài liệu SVG của một chữ. Mỗi chữ có 1 hoặc nhiều nhóm <g transform="scale(s) hoặc scale(s,-s)
        // translate(tx,ty)"> (chữ ghép: chữ gốc + dấu, mỗi nhóm tỉ lệ và độ lệch riêng); trong nhóm là các ô 1×1
        // dạng <rect x y width height> hoặc <path d="M x y L … Z">. Mọi nhóm đưa về lưới của nhóm đầu:
        // lấy tâm ô (đơn vị pixel của nhóm đầu, gốc = điểm gốc chữ, y lên trên) rồi làm tròn xuống.
        // Chữ không có SVG (dấu cách) → chữ rỗng, advance điền sau từ hmtx.
        private static PixelGlyph ReadGlyph(OpenTypeReader reader, uint glyphIndex, out float scale)
        {
            scale = 0f;
            string svg = reader.GetSvgDocument(glyphIndex);
            var empty = new PixelGlyph { Index = glyphIndex, Pixels = new bool[0, 0] };
            if (svg == null)
                return empty;

            var cells = new HashSet<Vector2Int>();
            float baseScale = 0f;
            foreach (Match group in GroupRegex.Matches(svg))
            {
                float sx = ParseFloat(group.Groups[1].Value);
                float sy = group.Groups[2].Success ? ParseFloat(group.Groups[2].Value) : sx;
                float tx = ParseFloat(group.Groups[3].Value);
                float ty = ParseFloat(group.Groups[4].Value);
                if (baseScale == 0f)
                    baseScale = Mathf.Abs(sx);
                float k = Mathf.Abs(sx) / baseScale;
                string body = group.Groups[5].Value;

                foreach (Match rect in RectRegex.Matches(body))
                {
                    int x = int.Parse(rect.Groups[1].Value, CultureInfo.InvariantCulture);
                    int y = int.Parse(rect.Groups[2].Value, CultureInfo.InvariantCulture);
                    int w = int.Parse(rect.Groups[3].Value, CultureInfo.InvariantCulture);
                    int h = int.Parse(rect.Groups[4].Value, CultureInfo.InvariantCulture);
                    for (int dx = 0; dx < w; dx++)
                    {
                        for (int dy = 0; dy < h; dy++)
                            cells.Add(ToCell(x + dx, y + dy, tx, ty, sy < 0f, k));
                    }
                }
                // Mọi path trong FatPix là ô 1×1 "M x y L x+1 y … Z" (đã kiểm tra 12/11/2026): lấy điểm M làm góc ô.
                foreach (Match path in PathRegex.Matches(body))
                {
                    float x = ParseFloat(path.Groups[1].Value);
                    float y = ParseFloat(path.Groups[2].Value);
                    cells.Add(ToCell(Mathf.RoundToInt(x), Mathf.RoundToInt(y), tx, ty, sy < 0f, k));
                }
            }
            scale = baseScale;
            if (cells.Count == 0)
                return empty;

            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            foreach (Vector2Int c in cells)
            {
                minX = Mathf.Min(minX, c.x);
                maxX = Mathf.Max(maxX, c.x);
                minY = Mathf.Min(minY, c.y);
                maxY = Mathf.Max(maxY, c.y);
            }

            var glyph = new PixelGlyph
            {
                Index = glyphIndex,
                Width = maxX - minX + 1,
                Height = maxY - minY + 1,
                Top = maxY + 1,
            };
            glyph.Advance = glyph.Width + LetterSpacing;
            glyph.Pixels = new bool[glyph.Width, glyph.Height];
            foreach (Vector2Int c in cells)
                glyph.Pixels[c.x - minX, c.y - minY] = true;
            return glyph;
        }

        // Ô (X, Y) của SVG (y hướng xuống) → ô trên lưới chữ (x sang phải, y lên trên, 0 = hàng ngay trên chân chữ).
        // flipped: nhóm scale(s,-s) đã lật trục y sẵn.
        private static Vector2Int ToCell(int x, int y, float tx, float ty, bool flipped, float k)
        {
            float centerX = (x + 0.5f + tx) * k;
            float centerY = (flipped ? y + 0.5f + ty : -(y + 0.5f + ty)) * k;
            return new Vector2Int(Mathf.FloorToInt(centerX), Mathf.FloorToInt(centerY));
        }

        // Xếp theo hàng (cao trước), chừa AtlasPadding quanh mỗi chữ.
        private static bool Pack(List<PixelGlyph> glyphs)
        {
            glyphs.Sort((a, b) => b.Height != a.Height ? b.Height.CompareTo(a.Height) : a.Index.CompareTo(b.Index));
            int x = AtlasPadding, y = AtlasPadding, rowHeight = 0;
            foreach (PixelGlyph glyph in glyphs)
            {
                if (glyph.Width == 0 || glyph.Height == 0)
                {
                    glyph.Rect = new GlyphRect(0, 0, 0, 0);
                    continue;
                }
                if (x + glyph.Width + AtlasPadding > AtlasSize)
                {
                    x = AtlasPadding;
                    y += rowHeight + AtlasPadding * 2;
                    rowHeight = 0;
                }
                if (y + glyph.Height + AtlasPadding > AtlasSize)
                    return false;
                glyph.Rect = new GlyphRect(x, y, glyph.Width, glyph.Height);
                x += glyph.Width + AtlasPadding * 2;
                rowHeight = Mathf.Max(rowHeight, glyph.Height);
            }
            return true;
        }

        // GlyphRect tính từ góc dưới-trái của texture, khớp với Pixels (y hướng lên).
        private static void DrawAtlas(Texture2D atlas, List<PixelGlyph> glyphs)
        {
            atlas.Reinitialize(AtlasSize, AtlasSize);
            var pixels = new Color32[AtlasSize * AtlasSize];
            var on = new Color32(255, 255, 255, 255);
            foreach (PixelGlyph glyph in glyphs)
            {
                for (int gx = 0; gx < glyph.Width; gx++)
                {
                    for (int gy = 0; gy < glyph.Height; gy++)
                    {
                        if (glyph.Pixels[gx, gy])
                            pixels[(glyph.Rect.y + gy) * AtlasSize + glyph.Rect.x + gx] = on;
                    }
                }
            }
            atlas.SetPixels32(pixels);
            atlas.filterMode = FilterMode.Point;
            atlas.Apply(false, false);
        }

        // Thông số dòng tính bằng pixel thiết kế, đo từ chữ thật: trên cùng (kể cả dấu chữ hoa), dưới cùng, H, x.
        private static FaceInfo BuildFaceInfo(FaceInfo face, Dictionary<uint, PixelGlyph> glyphs, SortedDictionary<uint, uint> characters)
        {
            // Đỉnh dòng theo chữ thường (kể cả có dấu, vd. "ấ"), không theo chữ hoa có dấu ("Ấ" cao hơn nhiều):
            // tính theo chữ hoa có dấu thì chữ thường bị lệch xuống khi canh giữa nút. Dấu chữ hoa được nhô lên trên.
            int ascent = 0, descent = 0;
            foreach (KeyValuePair<uint, uint> pair in characters)
            {
                PixelGlyph glyph = glyphs[pair.Value];
                if (glyph.Height == 0)
                    continue;
                descent = Mathf.Min(descent, glyph.Top - glyph.Height);
                if (pair.Key <= char.MaxValue && char.IsLower((char)pair.Key))
                    ascent = Mathf.Max(ascent, glyph.Top);
            }

            int cap = TopOf('H', glyphs, characters, ascent);
            ascent = Mathf.Max(ascent, cap);
            int mean = TopOf('x', glyphs, characters, cap / 2);
            int space = characters.TryGetValue(' ', out uint spaceIndex) ? glyphs[spaceIndex].Advance : 4;

            face.pointSize = PointSize;
            face.scale = 1f;
            face.ascentLine = ascent;
            face.descentLine = descent;
            face.lineHeight = ascent - descent + 1;
            face.capLine = cap;
            face.meanLine = mean;
            face.baseline = 0;
            face.underlineOffset = descent / 2f;
            face.underlineThickness = 1;
            face.strikethroughOffset = mean / 2f;
            face.strikethroughThickness = 1;
            face.superscriptOffset = cap / 2f;
            face.subscriptOffset = descent;
            face.tabWidth = space * 4;
            return face;
        }

        private static int TopOf(char c, Dictionary<uint, PixelGlyph> glyphs, SortedDictionary<uint, uint> characters, int fallback)
        {
            return characters.TryGetValue(c, out uint index) && glyphs[index].Height > 0 ? glyphs[index].Top : fallback;
        }

        // Material viền: giữ file .mat cũ (giữ GUID, giữ màu đã chỉnh tay), chỉ gắn lại atlas mới.
        private static string BuildOutlineMaterial(Texture2D atlas)
        {
            Shader shader = Shader.Find(OutlineShaderName);
            if (shader == null)
                return $"Không thấy shader {OutlineShaderName}, chưa tạo material viền.";

            var material = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterialPath);
            bool created = material == null;
            if (created)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, OutlineMaterialPath);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            // Atlas mới 1 pixel = 1 texel: viền và khoảng nới khung đều 1 texel.
            material.SetFloat("_OutlineTexels", OutlineTexels);
            material.SetFloat("_Padding", OutlineTexels);
            material.SetTexture(ShaderUtilities.ID_MainTex, atlas);
            material.SetFloat(ShaderUtilities.ID_TextureWidth, atlas.width);
            material.SetFloat(ShaderUtilities.ID_TextureHeight, atlas.height);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return created ? $"Đã tạo {OutlineMaterialPath}." : $"Đã gắn atlas mới cho {OutlineMaterialPath}.";
        }

        private static string MissingVietnamese(SortedDictionary<uint, uint> characters)
        {
            var builder = new StringBuilder();
            foreach (char c in VietnameseLetters())
            {
                if (!characters.ContainsKey(c))
                    builder.Append(c);
            }
            return builder.ToString();
        }

        private static IEnumerable<char> VietnameseLetters()
        {
            const string vowels = "aăâeêioôơuưy";
            string[] tones = { "", "̀", "́", "̃", "̉", "̣" };
            foreach (char vowel in vowels)
            {
                foreach (string tone in tones)
                {
                    yield return (vowel + tone).Normalize(NormalizationForm.FormC)[0];
                    yield return (char.ToUpper(vowel, CultureInfo.InvariantCulture) + tone).Normalize(NormalizationForm.FormC)[0];
                }
            }
            yield return 'đ';
            yield return 'Đ';
        }

        private static float ParseFloat(string value) => float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);

        // Đọc tối thiểu file OpenType: cmap (format 4, 12), hhea + hmtx (advance), bảng SVG (có thể nén gzip).
        private sealed class OpenTypeReader
        {
            private readonly byte[] _data;
            private readonly Dictionary<string, int> _tables = new Dictionary<string, int>();
            private readonly Dictionary<uint, string> _svg = new Dictionary<uint, string>();
            private readonly int _hmtx;
            private readonly int _metricsCount;

            public readonly Dictionary<uint, uint> CharacterToGlyph = new Dictionary<uint, uint>();

            public OpenTypeReader(byte[] data)
            {
                _data = data;
                int count = U16(4);
                for (int i = 0; i < count; i++)
                {
                    int record = 12 + i * 16;
                    string tag = Encoding.ASCII.GetString(data, record, 4);
                    _tables[tag] = (int)U32(record + 8);
                }
                if (!_tables.ContainsKey("cmap") || !_tables.ContainsKey("hhea") || !_tables.ContainsKey("hmtx"))
                    throw new InvalidDataException("thiếu bảng cmap / hhea / hmtx.");
                if (!_tables.ContainsKey("SVG "))
                    throw new InvalidDataException("font không có bảng SVG (cần bản FatPix-VN-SVG).");

                ReadCmap(_tables["cmap"]);
                _hmtx = _tables["hmtx"];
                _metricsCount = U16(_tables["hhea"] + 34);
                ReadSvg(_tables["SVG "]);
            }

            public int GetAdvance(uint glyph)
            {
                int index = (int)Math.Min(glyph, (uint)(_metricsCount - 1));
                return U16(_hmtx + index * 4);
            }

            public string GetSvgDocument(uint glyph) => _svg.TryGetValue(glyph, out string doc) ? doc : null;

            private void ReadCmap(int cmap)
            {
                int count = U16(cmap + 2);
                for (int i = 0; i < count; i++)
                {
                    int subtable = cmap + (int)U32(cmap + 4 + i * 8 + 4);
                    int format = U16(subtable);
                    if (format == 4)
                        ReadCmap4(subtable);
                    else if (format == 12)
                        ReadCmap12(subtable);
                }
            }

            private void ReadCmap4(int table)
            {
                int segments = U16(table + 6) / 2;
                int ends = table + 14;
                int starts = ends + segments * 2 + 2;
                int deltas = starts + segments * 2;
                int offsets = deltas + segments * 2;
                for (int s = 0; s < segments; s++)
                {
                    int end = U16(ends + s * 2);
                    int start = U16(starts + s * 2);
                    int delta = U16(deltas + s * 2);
                    int rangeOffset = U16(offsets + s * 2);
                    for (int c = start; c <= end && c != 0xFFFF; c++)
                    {
                        int glyph;
                        if (rangeOffset == 0)
                        {
                            glyph = (c + delta) & 0xFFFF;
                        }
                        else
                        {
                            glyph = U16(offsets + s * 2 + rangeOffset + (c - start) * 2);
                            if (glyph != 0)
                                glyph = (glyph + delta) & 0xFFFF;
                        }
                        if (glyph != 0)
                            CharacterToGlyph[(uint)c] = (uint)glyph;
                    }
                }
            }

            private void ReadCmap12(int table)
            {
                uint groups = U32(table + 12);
                for (int g = 0; g < groups; g++)
                {
                    int group = table + 16 + g * 12;
                    uint start = U32(group);
                    uint end = U32(group + 4);
                    uint glyph = U32(group + 8);
                    for (uint c = start; c <= end; c++)
                        CharacterToGlyph[c] = glyph + (c - start);
                }
            }

            private void ReadSvg(int svg)
            {
                int list = svg + (int)U32(svg + 2);
                int count = U16(list);
                for (int i = 0; i < count; i++)
                {
                    int entry = list + 2 + i * 12;
                    uint start = (uint)U16(entry);
                    uint end = (uint)U16(entry + 2);
                    int offset = (int)U32(entry + 4);
                    int length = (int)U32(entry + 8);
                    string doc = Decode(list + offset, length);
                    for (uint g = start; g <= end; g++)
                        _svg[g] = doc;
                }
            }

            private string Decode(int offset, int length)
            {
                if (length >= 2 && _data[offset] == 0x1F && _data[offset + 1] == 0x8B)
                {
                    using var input = new MemoryStream(_data, offset, length);
                    using var gzip = new GZipStream(input, CompressionMode.Decompress);
                    using var output = new MemoryStream();
                    gzip.CopyTo(output);
                    return Encoding.UTF8.GetString(output.ToArray());
                }
                return Encoding.UTF8.GetString(_data, offset, length);
            }

            private int U16(int offset) => (_data[offset] << 8) | _data[offset + 1];

            private uint U32(int offset) =>
                ((uint)_data[offset] << 24) | ((uint)_data[offset + 1] << 16) | ((uint)_data[offset + 2] << 8) | _data[offset + 3];
        }
    }
}
