using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;
using VeQue.View.Data;
using VeQue.View.UI;

namespace VeQue.View.Fishing
{
    // Gán / gỡ hiệu ứng cá shiny (material + đốm lấp lánh) cho icon UI và SpriteRenderer (Đợt 2 Phần 18).
    public static class ShinyLook
    {
        private static readonly int SpriteRectId = Shader.PropertyToID("_SpriteRect");
        private static readonly int UseSpriteRectId = Shader.PropertyToID("_UseSpriteRect");

        // shiny null = icon thường. seed: cố định cho mỗi con (vị trí đốm). Gọi sau khi đã gán sprite.
        public static void Apply(Image image, ShinyPalette.Entry shiny, int seed)
        {
            if (image == null)
                return;
            Material material = shiny != null ? shiny.Material : null;
            ShinyIconMesh mesh = image.GetComponent<ShinyIconMesh>();
            if (material != null && mesh == null)
                mesh = image.gameObject.AddComponent<ShinyIconMesh>();
            if (mesh != null)
                mesh.enabled = material != null;
            // Graphic.material bỏ qua nếu không đổi; null = material mặc định.
            image.material = material;
            ApplySparkles(image.gameObject, shiny, seed);
        }

        // shiny null = về normalMaterial. Khung sprite truyền qua MaterialPropertyBlock (block dùng lại để không cấp phát).
        public static void Apply(SpriteRenderer renderer, ShinyPalette.Entry shiny, int seed, Material normalMaterial,
            MaterialPropertyBlock block)
        {
            if (renderer == null)
                return;
            Material material = shiny != null ? shiny.Material : null;
            renderer.sharedMaterial = material != null ? material : normalMaterial;
            renderer.GetPropertyBlock(block);
            if (material != null && renderer.sprite != null)
            {
                block.SetVector(SpriteRectId, DataUtility.GetOuterUV(renderer.sprite));
                block.SetFloat(UseSpriteRectId, 1f);
            }
            else
            {
                block.SetFloat(UseSpriteRectId, 0f);
            }
            renderer.SetPropertyBlock(block);
            ApplySparkles(renderer.gameObject, shiny, seed);
        }

        private static void ApplySparkles(GameObject target, ShinyPalette.Entry shiny, int seed)
        {
            ShinySparkles sparkles = target.GetComponent<ShinySparkles>();
            if (shiny == null)
            {
                if (sparkles != null)
                    sparkles.Hide();
                return;
            }
            if (sparkles == null)
                sparkles = target.AddComponent<ShinySparkles>();
            sparkles.Show(shiny.SparkleColor, seed);
        }
    }
}
