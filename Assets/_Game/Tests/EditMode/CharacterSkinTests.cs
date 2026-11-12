using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VeQue.View.Character;

namespace VeQue.Tests
{
    // Kiểm tra CharacterSkin.asset thật (tạo bằng menu Về Quê → Nhân vật Sunnyside: import + gán sprite).
    public class CharacterSkinTests
    {
        private const string SkinPath = "Assets/_Game/Data/CharacterSkin.asset";

        private static CharacterSkin LoadSkin()
        {
            var skin = AssetDatabase.LoadAssetAtPath<CharacterSkin>(SkinPath);
            Assert.IsNotNull(skin, $"Không thấy {SkinPath}. Chạy menu Về Quê → Nhân vật Sunnyside: import + gán sprite.");
            return skin;
        }

        [TestCase(CharacterAnim.Idle, 9)]
        [TestCase(CharacterAnim.Walk, 8)]
        [TestCase(CharacterAnim.Work, 8)]
        [TestCase(CharacterAnim.Water, 5)]
        [TestCase(CharacterAnim.Cast, 15)]
        [TestCase(CharacterAnim.Wait, 9)]
        [TestCase(CharacterAnim.Reel, 13)]
        [TestCase(CharacterAnim.Caught, 10)]
        public void Skin_EveryLayerHasAllFrames(CharacterAnim anim, int frames)
        {
            CharacterSkin skin = LoadSkin();
            CharacterSkin.Clip clip = skin.Get(anim);

            Assert.AreEqual(frames, clip.Body.Length, $"{anim}: thân. Chạy lại menu Nhân vật Sunnyside.");
            Assert.AreEqual(frames, clip.Tools.Length, $"{anim}: dụng cụ");
            Assert.That(clip.Body, Has.None.Null, $"{anim}: thân thiếu khung");
            Assert.That(clip.Tools, Has.None.Null, $"{anim}: dụng cụ thiếu khung");
            Assert.Greater(skin.GetDuration(anim), 0f);

            Assert.AreEqual(6, skin.HairCount);
            for (int i = 0; i < skin.HairCount; i++)
            {
                CharacterSkin.HairSet hair = skin.GetHair(i);
                Sprite[] hairFrames = hair.Get(anim);
                Assert.AreEqual(frames, hairFrames.Length, $"{anim}: tóc {hair.Id}");
                Assert.That(hairFrames, Has.None.Null, $"{anim}: tóc {hair.Id} thiếu khung");
            }
        }

        [TestCase(CharacterAnim.Cast, false)]
        [TestCase(CharacterAnim.Wait, true)]
        [TestCase(CharacterAnim.Reel, true)]
        [TestCase(CharacterAnim.Caught, false)]
        public void Skin_FishingClipsLoopAsPlanned(CharacterAnim anim, bool loop)
        {
            Assert.AreEqual(loop, LoadSkin().Get(anim).Loop, anim.ToString());
        }
    }
}
