using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.View.Fishing
{
    // Công cụ xem thử cá shiny (Đợt 2 Phần 18), mở bằng menu Về Quê → Cá shiny: xem thử (khi Play); chạy lại menu để đóng.
    // Bảng phủ: mỗi hàng 1 kiểu (hàng đầu là cá gốc), mỗi cột 1 loài, trong khung cuộn có RectMask2D (thử cắt khi cuộn).
    // Nút "Ẩn bảng": xem 9 con dạng SpriteRenderer giữa màn hình (con lẻ lật ngang) để thử bản world.
    // Chỉ dựng lúc chạy, không lưu vào scene.
    public sealed class ShinyPreview : MonoBehaviour
    {
        private const float Cell = 40f;
        private const float Icon = 32f;
        private const float LabelWidth = 96f;
        private const float HeaderHeight = 18f;

        private static ShinyPreview _instance;

        private GameObject _panel;
        private GameObject _world;
        private TMP_FontAsset _font;

        public static void Toggle(FishingDefinition fishing, TMP_FontAsset font)
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
                return;
            }
            var go = new GameObject(nameof(ShinyPreview));
            _instance = go.AddComponent<ShinyPreview>();
            _instance._font = font;
            _instance.Build(fishing);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private void Build(FishingDefinition fishing)
        {
            var kinds = new List<ShinyKind> { ShinyKind.None };
            for (int k = 1; k <= 8; k++)
            {
                if (fishing.ShinyPalette.TryGet((ShinyKind)k, out _))
                    kinds.Add((ShinyKind)k);
            }

            BuildWorld(fishing, kinds);

            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390f, 844f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Nút luôn hiện (kể cả khi ẩn bảng).
            Button toggle = MakeButton(canvasGo.transform, "Ẩn bảng", new Vector2(-8f, -8f));
            Button close = MakeButton(canvasGo.transform, "Đóng", new Vector2(-96f, -8f));
            close.onClick.AddListener(() => Destroy(gameObject));

            _panel = new GameObject("Panel", typeof(RectTransform));
            _panel.transform.SetParent(canvasGo.transform, false);
            _panel.transform.SetAsFirstSibling();
            Stretch(_panel.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
            _panel.AddComponent<Image>().color = new Color(0.48f, 0.71f, 0.71f, 1f);
            MakeText(_panel.transform, "Cá shiny – xem thử (kéo để cuộn)", new Vector2(8f, -12f), 10f, TextAlignmentOptions.TopLeft, 260f);

            toggle.onClick.AddListener(() =>
            {
                bool show = !_panel.activeSelf;
                _panel.SetActive(show);
                toggle.GetComponentInChildren<TMP_Text>().text = show ? "Ẩn bảng" : "Hiện bảng";
            });

            // Khung cuộn.
            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(_panel.transform, false);
            RectTransform viewport = viewportGo.GetComponent<RectTransform>();
            Stretch(viewport, 8f, 8f, 48f, 8f);
            viewportGo.AddComponent<Image>().color = new Color(0.59f, 0.80f, 0.80f, 1f);
            viewportGo.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewport, false);
            RectTransform content = contentGo.GetComponent<RectTransform>();
            content.anchorMin = content.anchorMax = content.pivot = new Vector2(0f, 1f);
            content.sizeDelta = new Vector2(LabelWidth + fishing.Fish.Count * Cell, HeaderHeight + kinds.Count * Cell);

            var scroll = _panel.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = true;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            for (int c = 0; c < fishing.Fish.Count; c++)
            {
                MakeText(content, (c + 1).ToString(), new Vector2(LabelWidth + c * Cell, -2f), 7f, TextAlignmentOptions.Top, Cell);
            }
            for (int r = 0; r < kinds.Count; r++)
            {
                ShinyKind kind = kinds[r];
                float y = -(HeaderHeight + r * Cell);
                ShinyPalette.Entry entry = fishing.GetShiny(kind);
                string label = entry != null ? entry.DisplayName : "Gốc";
                MakeText(content, label, new Vector2(4f, y - Cell * 0.5f + 6f), 8f, TextAlignmentOptions.TopLeft, LabelWidth - 6f);
                for (int c = 0; c < fishing.Fish.Count; c++)
                {
                    FishDefinition fish = fishing.Fish[c];
                    if (fish == null || fish.Icon == null)
                        continue;
                    var iconGo = new GameObject(fish.Id, typeof(RectTransform));
                    iconGo.transform.SetParent(content, false);
                    RectTransform rt = iconGo.GetComponent<RectTransform>();
                    rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
                    rt.sizeDelta = new Vector2(Icon, Icon);
                    rt.anchoredPosition = new Vector2(LabelWidth + c * Cell + (Cell - Icon) * 0.5f, y - (Cell - Icon) * 0.5f);
                    var image = iconGo.AddComponent<Image>();
                    image.sprite = fish.Icon;
                    image.raycastTarget = false;
                    ShinyLook.Apply(image, entry, r * 31 + c);
                }
            }
        }

        // Một hàng SpriteRenderer giữa màn hình: con thứ lẻ lật ngang (như cá bay về bên phải).
        private void BuildWorld(FishingDefinition fishing, List<ShinyKind> kinds)
        {
            Sprite sprite = null;
            foreach (FishDefinition fish in fishing.Fish)
            {
                if (fish != null && fish.Icon != null)
                {
                    sprite = fish.Icon;
                    break;
                }
            }
            Camera cam = Camera.main;
            if (sprite == null || cam == null)
                return;

            _world = new GameObject("World");
            _world.transform.SetParent(transform, false);
            float step = sprite.bounds.size.x + 0.25f;
            Vector3 center = cam.transform.position;
            center.z = 0f;
            var block = new MaterialPropertyBlock();
            for (int i = 0; i < kinds.Count; i++)
            {
                var go = new GameObject(kinds[i].ToString());
                go.transform.SetParent(_world.transform, false);
                go.transform.position = center + new Vector3((i - (kinds.Count - 1) * 0.5f) * step, 0f, 0f);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = 5000;
                renderer.flipX = i % 2 == 1;
                Material normal = renderer.sharedMaterial;
                ShinyLook.Apply(renderer, fishing.GetShiny(kinds[i]), i * 7, normal, block);
            }
        }

        private Button MakeButton(Transform parent, string label, Vector2 topRight)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(80f, 32f);
            rt.anchoredPosition = topRight;
            go.AddComponent<Image>().color = new Color(0.95f, 0.85f, 0.6f, 1f);
            var button = go.AddComponent<Button>();
            TMP_Text text = MakeText(go.transform, label, Vector2.zero, 10f, TextAlignmentOptions.Center, 80f);
            Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
            return button;
        }

        private TMP_Text MakeText(Transform parent, string value, Vector2 topLeft, float size, TextAlignmentOptions align, float width)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(width, 20f);
            rt.anchoredPosition = topLeft;
            var text = go.AddComponent<TextMeshProUGUI>();
            if (_font != null)
                text.font = _font;
            text.text = value;
            text.fontSize = size;
            text.alignment = align;
            text.color = new Color(0.094f, 0.078f, 0.145f, 1f);
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rt, float left, float right, float top, float bottom)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }
    }
}
