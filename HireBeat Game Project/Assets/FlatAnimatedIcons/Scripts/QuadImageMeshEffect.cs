using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowArt
{
    public class QuadImageMeshEffect : BaseMeshEffect
    {
        public Vector2[] from;
        public Vector2[] to;
        public float curValue = 0;
        protected QuadImageMeshEffect()
        {
            from = new Vector2[] { new Vector2(1,1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1)};
            to = new Vector2[] { new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f) };
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if(vh.currentVertCount != 4)
            {
                return;
            }
            UIVertex vert = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vert, i);
                Vector3 pos = vert.position;
                Vector2 val = Vector2.Lerp(from[i], to[i], curValue);
                vert.position = new Vector3(pos.x * val.x, pos.y * val.y, pos.z);
                vh.SetUIVertex(vert, i);
            }
        }
    }
}
