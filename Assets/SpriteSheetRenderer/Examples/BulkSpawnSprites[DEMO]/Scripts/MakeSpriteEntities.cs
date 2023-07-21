using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ECSSpriteSheetAnimation.Examples {
  public class MakeSpriteEntities : MonoBehaviour
  {
    public int spriteCount = 5000;
    public Sprite[] sprites;
    public float2 spawnArea = new float2(100, 100);

    public class GetPrefabBaker : Baker<MakeSpriteEntities>
    {
      public override void Bake(MakeSpriteEntities authoring)
      {
        /*EntityArchetype archetype = eManager.CreateArchetype(
           typeof(Position2D),
           typeof(Rotation2D),
           typeof(LocalTransform),
           //required params
           typeof(SpriteIndex),
           typeof(SpriteSheetAnimation),
           typeof(SpriteSheetMaterial),
           typeof(SpriteSheetColor),
           typeof(SpriteMatrix),
           typeof(BufferHook)
        );*/

        NativeArray<Entity> entities = new NativeArray<Entity>(authoring.spriteCount, Allocator.Temp);
        //eManager.CreateEntity(archetype, entities);

        //only needed for the first time to bake the material and create the uv map
        SpriteSheetManager.RecordSpriteSheet(authoring.sprites, "emoji", entities.Length);


        Rect area = GetSpawnArea(authoring.spawnArea);
        Random rand = new Random((uint)UnityEngine.Random.Range(0, int.MaxValue));
        int cellCount = SpriteSheetCache.GetLength("emoji");
        SpriteSheetMaterial material = new SpriteSheetMaterial { material = SpriteSheetCache.GetMaterial("emoji") };

        for (int i = 0; i < entities.Length; i++)
        {
          Entity e = entities[i];
          var color = UnityEngine.Random.ColorHSV(.15f, .75f);

          AddComponent(e, new LocalTransform() { Scale = 10 });
          AddComponent(e, new Position2D { Value = rand.NextFloat2(area.min, area.max) });

          AddComponent(e, new SpriteIndex { Value = rand.NextInt(0, cellCount) });
          AddComponent(e,
            new SpriteSheetAnimation
            {
              maxSprites = cellCount, play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10
            });
          AddComponent(e, new SpriteSheetColor { color = new float4(color.r, color.g, color.b, color.a) });
          AddComponent<SpriteMatrix>(e, default);
          AddComponent(e,
            new BufferHook { bufferID = i, bufferEnityID = DynamicBufferManager.GetEntityBufferID(material) });
          AddSharedComponentManaged(e, material);
        }
      }

      Rect GetSpawnArea(float2 spawnArea)
      {
        Rect r = new Rect(0, 0, spawnArea.x, spawnArea.y);
        r.center = new Vector2(spawnArea.x / 2, spawnArea.y / 2);
        return r;
      }
    }
  }
}