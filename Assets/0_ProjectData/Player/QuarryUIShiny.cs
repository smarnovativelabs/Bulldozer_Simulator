using UnityEngine;
using UnityEngine.UI;
using Coffee.UIEffects;

namespace Bulldozer.Player
{
    // Keep sprite UVs untouched. The bundled legacy UIShiny packs 24-bit numbers
    // into UV0, which renders the wrong atlas region in this project's UI pipeline.
    public sealed class QuarryUIShiny : UIShiny
    {
        public Shader compatibleShader;
        protected override void OnEnable()
        {
            if(graphic&&graphic.canvas)graphic.canvas.additionalShaderChannels|=AdditionalCanvasShaderChannels.TexCoord1;
            base.OnEnable();
        }
        public override Hash128 GetMaterialHash(Material material)
        {
            return isActiveAndEnabled&&material?new Hash128((uint)material.GetInstanceID(),0x51534849,0,0):default;
        }
        public override void ModifyMaterial(Material material,Graphic target)
        {
            material.shader=compatibleShader;paramTex.RegisterMaterial(material);
        }
        public override void ModifyMesh(VertexHelper vh,Graphic target)
        {
            if(!isActiveAndEnabled)return;
            Rect rect=rectTransform.rect;float angle=rotation*Mathf.Deg2Rad;
            Vector2 direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
            float extent=Mathf.Abs(direction.x)*rect.width+Mathf.Abs(direction.y)*rect.height;
            float row=paramTex.GetNormalizedIndex(this);
            var vertex=default(UIVertex);
            for(int i=0;i<vh.currentVertCount;i++)
            {
                vh.PopulateUIVertex(ref vertex,i);
                float position=.5f+Vector2.Dot((Vector2)vertex.position-rect.center,direction)/Mathf.Max(.001f,extent);
                vertex.uv1=new Vector4(position,row,0,0);vh.SetUIVertex(vertex,i);
            }
        }
    }
}
