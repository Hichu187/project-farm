using NUnit.Framework;
using VeQue.View.Anim;

namespace VeQue.Tests
{
    public class EaseTests
    {
        private const float Tolerance = 1e-4f;

        [TestCase(-1f, 0f)]
        [TestCase(0f, 0f)]
        [TestCase(1f, 1f)]
        [TestCase(2f, 1f)]
        public void AllCurves_StartAt0EndAt1_AndClamp(float t, float expected)
        {
            Assert.AreEqual(expected, Ease.OutQuad(t), Tolerance);
            Assert.AreEqual(expected, Ease.InQuad(t), Tolerance);
            Assert.AreEqual(expected, Ease.OutBack(t), Tolerance);
        }

        [Test]
        public void OutQuad_FasterThanLinearEarly_InQuadSlower()
        {
            Assert.Greater(Ease.OutQuad(0.25f), 0.25f);
            Assert.Less(Ease.InQuad(0.25f), 0.25f);
        }

        [Test]
        public void OutBack_OvershootsAboutTenPercent()
        {
            float peak = 0f;
            for (int i = 0; i <= 100; i++)
                peak = System.Math.Max(peak, Ease.OutBack(i / 100f));

            Assert.That(peak, Is.InRange(1.05f, 1.15f));
        }

        [Test]
        public void DampedWave_SquashFirstThenStretch_EndsAtZero()
        {
            Assert.AreEqual(0f, Ease.DampedWave(0f), Tolerance);
            Assert.Less(Ease.DampedWave(0.25f), 0f);
            Assert.Greater(Ease.DampedWave(0.75f), 0f);
            Assert.AreEqual(0f, Ease.DampedWave(1f), Tolerance);
            Assert.Greater(System.Math.Abs(Ease.DampedWave(0.25f)), Ease.DampedWave(0.75f));
        }

        [TestCase(0.03f, 0f)]
        [TestCase(0.04f, 0.0625f)]
        [TestCase(0.25f, 0.25f)]
        [TestCase(-0.05f, -0.0625f)]
        public void SnapToPixel_RoundsToWholePixels(float value, float expected)
        {
            Assert.AreEqual(expected, Ease.SnapToPixel(value, 16f), Tolerance);
        }
    }
}
