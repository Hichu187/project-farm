using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace VeQue.View.UI
{
    // Ghi khung sprite trong texture (uMin, vMin, uMax, vMax) vào uv1 của Image để shader VeQue/ShinyIcon tính được
    // tọa độ pixel nguyên của sprite, kể cả khi sprite nằm trong Sprite Atlas. Tự bật kênh TexCoord1 của Canvas.
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public sealed class ShinyIconMesh : BaseMeshEffect
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            EnableChannel();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;
            EnableChannel();
            Sprite sprite = graphic is Image image ? image.overrideSprite : null;
            Vector4 rect = sprite != null ? DataUtility.GetOuterUV(sprite) : new Vector4(0f, 0f, 1f, 1f);
            UIVertex vertex = default;
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                vertex.uv1 = rect;
                vh.SetUIVertex(vertex, i);
            }
        }

        private void EnableChannel()
        {
            Canvas canvas = graphic != null ? graphic.canvas : null;
            if (canvas != null)
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
        }
    }
}
