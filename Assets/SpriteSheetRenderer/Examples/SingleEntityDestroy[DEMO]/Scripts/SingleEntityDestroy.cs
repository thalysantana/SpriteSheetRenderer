using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SingleEntityDestroy : MonoBehaviour {
  public Sprite[] sprites;

  public class GetPrefabBaker : Baker<SingleEntityDestroy>
  {
    public override void Bake(SingleEntityDestroy authoring)
    {
      SpriteSheetManager.RecordSpriteSheet(authoring.sprites, "emoji");
    }
  }
    
  void Update() {
    if(Input.GetKeyDown(KeyCode.Space)) {
      int maxSprites = SpriteSheetCache.GetLength("emoji");
      var color = UnityEngine.Random.ColorHSV(.35f, .85f);

      // 3) Populate components
      List<IComponentData> components = new List<IComponentData> {
        new Position2D { Value = UnityEngine.Random.insideUnitCircle * 7 },
        new LocalTransform() { Scale = UnityEngine.Random.Range(0,3f) },
        new SpriteIndex { Value = UnityEngine.Random.Range(0, maxSprites) },
        new SpriteSheetAnimation { maxSprites = maxSprites, play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 },
        new SpriteSheetColor { color = new float4(color.r, color.g, color.b, color.a) },
        new LifeTime{ Value = UnityEngine.Random.Range(5,15)}
      };

      SpriteSheetManager.Instantiate(components, "emoji");
    }
  }
}
