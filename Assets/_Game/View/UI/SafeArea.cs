using UnityEngine;

namespace VeQue.View.UI
{
    // Co RectTransform vào vùng an toàn (tai thỏ, thanh home). Kiểm tra lại mỗi frame vì có thể xoay màn hình.
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _applied;
        private Vector2Int _appliedScreen;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != _applied || Screen.width != _appliedScreen.x || Screen.height != _appliedScreen.y)
                Apply();
        }

        private void Apply()
        {
            _applied = Screen.safeArea;
            _appliedScreen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            _rect.anchorMin = new Vector2(_applied.xMin / Screen.width, _applied.yMin / Screen.height);
            _rect.anchorMax = new Vector2(_applied.xMax / Screen.width, _applied.yMax / Screen.height);
        }
    }
}
