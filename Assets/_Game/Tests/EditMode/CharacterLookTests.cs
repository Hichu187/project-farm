using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    public class CharacterLookTests
    {
        private const string Zone1 = "khu1";
        private const string RauMuong = "rau_muong";

        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
        }

        // ---------- Chuẩn hóa tên ----------

        [TestCase("Long", "Long")]
        [TestCase("  Bé   Na  ", "Bé Na")]
        [TestCase("Tèo\tEm", "Tèo Em")]
        [TestCase("A", "A")]
        [TestCase("1234567890123456", "1234567890123456")]
        public void TryNormalizeName_Valid_ReturnsCleanName(string raw, string expected)
        {
            Assert.IsTrue(CharacterLook.TryNormalizeName(raw, out string name));
            Assert.AreEqual(expected, name);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("12345678901234567")]
        [TestCase("\n\t")]
        public void TryNormalizeName_Invalid_False(string raw)
        {
            Assert.IsFalse(CharacterLook.TryNormalizeName(raw, out string name));
            Assert.IsNull(name);
        }

        [Test]
        public void TryNormalizeName_ControlCharacters_Removed()
        {
            Assert.IsTrue(CharacterLook.TryNormalizeName("Na\u0000m\u0007", out string name));
            Assert.AreEqual("Nam", name);
        }

        [Test]
        public void TryNormalizeName_DecomposedVietnamese_CountsAsPrecomposed()
        {
            // "Hoàng" gõ kiểu tổ hợp: a + dấu huyền rời (6 ký tự) → dạng dựng sẵn 5 ký tự.
            string decomposed = "Hoàng";
            Assert.IsTrue(CharacterLook.TryNormalizeName(decomposed, out string name));
            Assert.AreEqual("Hoàng", name);
        }

        // ---------- Farm.SetCharacter ----------

        [Test]
        public void NewFarm_CharacterNotCreated_DefaultLook()
        {
            Farm farm = CreateFarm();

            CharacterLook look = farm.State.Character;
            Assert.IsFalse(look.Created);
            Assert.AreEqual(CharacterLook.DefaultHairStyle, look.HairStyle);
            Assert.AreEqual(CharacterLook.DefaultHairColor, look.HairColor);
        }

        [Test]
        public void SetCharacter_Valid_StoresAndRaisesChanged()
        {
            Farm farm = CreateFarm();
            int changed = 0;
            farm.Changed += () => changed++;

            Assert.AreEqual(FarmResult.Ok, farm.SetCharacter("  Bé Na ", "longhair", "vang"));

            CharacterLook look = farm.State.Character;
            Assert.IsTrue(look.Created);
            Assert.AreEqual("Bé Na", look.Name);
            Assert.AreEqual("longhair", look.HairStyle);
            Assert.AreEqual("vang", look.HairColor);
            Assert.AreEqual(1, changed);
        }

        [Test]
        public void SetCharacter_EmptyName_InvalidNameAndUnchanged()
        {
            Farm farm = CreateFarm();
            int changed = 0;
            farm.Changed += () => changed++;

            Assert.AreEqual(FarmResult.InvalidName, farm.SetCharacter("   ", "longhair", "vang"));

            Assert.IsFalse(farm.State.Character.Created);
            Assert.AreEqual(CharacterLook.DefaultHairStyle, farm.State.Character.HairStyle);
            Assert.AreEqual(0, changed);
        }

        [Test]
        public void SetCharacter_EmptyStyleAndColor_UseDefaults()
        {
            Farm farm = CreateFarm();

            Assert.AreEqual(FarmResult.Ok, farm.SetCharacter("Nam", null, ""));

            Assert.AreEqual(CharacterLook.DefaultHairStyle, farm.State.Character.HairStyle);
            Assert.AreEqual(CharacterLook.DefaultHairColor, farm.State.Character.HairColor);
        }

        // ---------- Save ----------

        [Test]
        public void Save_RoundTrip_KeepsCharacter()
        {
            Farm farm = CreateFarm();
            farm.SetCharacter("Tí", "curlyhair", "do");

            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            Assert.AreEqual(SaveData.CurrentVersion, data.version);
            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState loaded));

            CharacterLook look = loaded.Character;
            Assert.IsTrue(look.Created);
            Assert.AreEqual("Tí", look.Name);
            Assert.AreEqual("curlyhair", look.HairStyle);
            Assert.AreEqual("do", look.HairColor);
        }

        [Test]
        public void Save_EmptyCharacterObject_LikeJsonUtilityDefault_NotCreated()
        {
            // JsonUtility tạo sẵn object rỗng khi save cũ thiếu trường character.
            Farm farm = CreateFarm();
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            data.character = new SaveCharacter();

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState loaded));

            Assert.IsFalse(loaded.Character.Created);
            Assert.AreEqual(CharacterLook.DefaultHairStyle, loaded.Character.HairStyle);
        }

        [Test]
        public void Save_CreatedButBrokenName_NotCreated()
        {
            Farm farm = CreateFarm();
            farm.SetCharacter("Tí", "curlyhair", "do");
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            data.character.name = "   ";

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState loaded));

            Assert.IsFalse(loaded.Character.Created);
            // Vẫn giữ tóc đã chọn để màn tạo nhân vật hiện sẵn.
            Assert.AreEqual("curlyhair", loaded.Character.HairStyle);
        }

        // ---------- Helpers ----------

        private Farm CreateFarm()
        {
            var zone = new ZoneDef(Zone1, new[] { new BedDef(6, 2) }, 6, new int[6]);
            var crops = new[] { new CropDef(RauMuong, 120, 5, 12, 2) };
            var def = new FarmDef(new[] { zone }, crops, 100, new Dictionary<string, int> { { RauMuong, 3 } }, 20);
            return new Farm(def, FarmState.CreateNew(def), _clock);
        }
    }
}
