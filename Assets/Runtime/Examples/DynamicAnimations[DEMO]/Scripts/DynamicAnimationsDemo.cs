using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class DynamicAnimationsDemo: MonoBehaviour {
  public SpriteSheetAnimator animator;
  public static Entity character;

    public class GetPrefabBaker : Baker<DynamicAnimationsDemo>
    {
      public override void Bake(DynamicAnimationsDemo authoring)
      {
        SpriteSheetManager.RecordAnimator(authoring.animator);

        var color = Color.white;

        // 3) Populate components
        List<IComponentData> components = new List<IComponentData> {
          new Position2D { Value = float2.zero },
          new LocalTransform() { Scale = 5 },
          new SpriteSheetColor { color = new float4(color.r, color.g, color.b, color.a) }
        };
        // 4) Instantiate the entity
        character = SpriteSheetManager.Instantiate(components, authoring.animator);
      }
    }
}
