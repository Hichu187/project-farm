using UnityEngine;

namespace VeQue.View.Fishing
{
    // Âm thanh kéo cá (Đợt 2 Phần 6): mỗi mũi tên đúng một nốt cao dần (đô – rê – mi…), bấm sai một tiếng trầm ngắn.
    // Tiếng sinh bằng code lúc khởi động (không thêm file âm thanh, không tăng dung lượng build).
    // Web: trình duyệt chỉ cho phát sau lần chạm đầu – lúc kéo cá người chơi đã chạm nhiều lần nên không sao.
    [RequireComponent(typeof(AudioSource))]
    public sealed class PullSound : MonoBehaviour
    {
        // Thang đô trưởng từ C5, đủ cho chuỗi dài nhất (10 mũi tên); dài hơn thì giữ nốt cuối.
        private static readonly float[] Notes = { 523.25f, 587.33f, 659.25f, 698.46f, 783.99f, 880f, 987.77f, 1046.5f, 1174.66f, 1318.51f };
        private const int SampleRate = 22050;

        [Range(0f, 1f)] [SerializeField] private float _volume = 0.35f;
        [SerializeField] private float _noteSeconds = 0.12f;
        [SerializeField] private float _mistakeSeconds = 0.2f;
        [SerializeField] private float _mistakeHz = 110f;

        private AudioSource _source;
        private AudioClip[] _notes;
        private AudioClip _mistake;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _notes = new AudioClip[Notes.Length];
            for (int i = 0; i < Notes.Length; i++)
                _notes[i] = Tone($"pull_note_{i}", Notes[i], _noteSeconds, false);
            _mistake = Tone("pull_mistake", _mistakeHz, _mistakeSeconds, true);
        }

        private void OnDestroy()
        {
            if (_notes != null)
            {
                foreach (AudioClip clip in _notes)
                    Destroy(clip);
            }
            if (_mistake != null)
                Destroy(_mistake);
        }

        // index: số mũi tên đã đúng trừ 1 (0 = mũi đầu).
        public void PlayCorrect(int index)
        {
            if (_notes == null || _notes.Length == 0)
                return;
            _source.PlayOneShot(_notes[Mathf.Clamp(index, 0, _notes.Length - 1)], _volume);
        }

        public void PlayMistake()
        {
            if (_mistake != null)
                _source.PlayOneShot(_mistake, _volume);
        }

        // Sóng sin (nốt) hoặc vuông dịu (tiếng sai), vào nhanh 5 ms rồi tắt dần.
        private static AudioClip Tone(string name, float hz, float seconds, bool square)
        {
            int count = Mathf.Max(1, Mathf.RoundToInt(SampleRate * seconds));
            var data = new float[count];
            int attack = Mathf.Max(1, SampleRate / 200);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SampleRate;
                float wave = Mathf.Sin(2f * Mathf.PI * hz * t);
                if (square)
                    wave = Mathf.Clamp(wave * 3f, -1f, 1f) * 0.6f;
                float envelope = Mathf.Min(1f, i / (float)attack) * (1f - i / (float)count);
                data[i] = wave * envelope * envelope;
            }
            AudioClip clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
