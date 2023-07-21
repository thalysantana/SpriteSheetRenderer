using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SingleSpriteSheetSpawner : MonoBehaviour {
  public Sprite[] sprites;

  public class GetPrefabBaker : Baker<SingleSpriteSheetSpawner>
  {
    public override void Bake(SingleSpriteSheetSpawner authoring)
    {
      // 2) Record and bake this spritesheet(only once)
      SpriteSheetManager.RecordSpriteSheet(authoring.sprites, "emoji");

      int maxSprites = SpriteSheetCache.GetLength("emoji");
      var color = UnityEngine.Random.ColorHSV(.35f, .85f);

      // 3) Populate components
      List<IComponentData> components = new List<IComponentData> {
        new Position2D { Value = float2.zero },
        new LocalTransform() { Scale = 15 },
        new SpriteIndex { Value = UnityEngine.Random.Range(0, maxSprites) },
        new SpriteSheetAnimation { maxSprites = maxSprites, play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 },
        new SpriteSheetColor { color = new float4(color.r, color.g, color.b, color.a) }
      };
      // 4) Instantiate the entity
      Entity e = SpriteSheetManager.Instantiate(components, "emoji");
    }
  }
}
