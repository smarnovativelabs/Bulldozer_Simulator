using UnityEngine;

namespace Bulldozer.UI
{
    /// <summary>A flat, feathered UI disc without a texture or material allocation.</summary>
    public sealed class LoadingDiscGraphic : UnityEngine.UI.MaskableGraphic
    {
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
        {
            vh.Clear();
            Rect rect = rectTransform.rect;
            Vector2 center = rect.center;
            Vector2 radius = rect.size * .5f;
            const int segments = 96;
            vh.AddVert(center, color, Vector2.zero);
            Color transparent = color;
            transparent.a = 0f;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vh.AddVert(center + Vector2.Scale(direction, radius - Vector2.one), color, Vector2.zero);
                vh.AddVert(center + Vector2.Scale(direction, radius), transparent, Vector2.zero);
                if (i == 0) continue;
                int current = 1 + i * 2;
                int previous = current - 2;
                vh.AddTriangle(0, previous, current);
                vh.AddTriangle(previous, previous + 1, current);
                vh.AddTriangle(current, previous + 1, current + 1);
            }
        }
    }
}
