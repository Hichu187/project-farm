using System.Collections;
using TMPro;
using UnityEngine;

namespace VeQue.View.UI
{
    // Thông báo ngắn, tự ẩn, không chặn chạm.
    public sealed class Toast : MonoBehaviour
    {
        [SerializeField] private GameObject _box;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _duration = 2f;

        private Coroutine _hide;

        public void Show(string message)
        {
            _text.text = message;
            _box.SetActive(true);
            if (_hide != null)
                StopCoroutine(_hide);
            _hide = StartCoroutine(HideLater());
        }

        private IEnumerator HideLater()
        {
            yield return new WaitForSecondsRealtime(_duration);
            _box.SetActive(false);
            _hide = null;
        }
    }
}
