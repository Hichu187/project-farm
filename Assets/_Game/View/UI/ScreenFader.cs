using System;
using UnityEngine;

namespace VeQue.View.UI
{
    // Màn tối phủ cả màn hình cho chuyển cảnh (đi câu / về nhà). Chặn chạm trong lúc tối / đang chuyển.
    // Dùng giờ thật (không phụ thuộc timeScale).
    public sealed class ScreenFader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;

        private float _from;
        private float _to;
        private float _start;
        private float _duration;
        private Action _done;
        private bool _running;

        public void FadeOut(float duration, Action done) => Run(1f, duration, done);

        public void FadeIn(float duration, Action done) => Run(0f, duration, done);

        private void Awake()
        {
            if (_group == null)
                _group = GetComponent<CanvasGroup>();
        }

        private void Run(float to, float duration, Action done)
        {
            _from = _group.alpha;
            _to = to;
            _start = Time.unscaledTime;
            _duration = Mathf.Max(0f, duration);
            _done = done;
            _running = true;
            _group.blocksRaycasts = true;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            if (_duration == 0f)
                Finish();
        }

        private void Update()
        {
            if (!_running)
                return;

            float t = (Time.unscaledTime - _start) / _duration;
            if (t >= 1f)
            {
                Finish();
                return;
            }
            _group.alpha = Mathf.Lerp(_from, _to, t);
        }

        private void Finish()
        {
            _running = false;
            _group.alpha = _to;
            _group.blocksRaycasts = _to > 0f;
            Action done = _done;
            _done = null;
            done?.Invoke();
        }
    }
}
