using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Fractalnitializer : MonoBehaviour {
  public Sprite[] sprites;
  public static EntityArchetype archetype;
  public static QuadTree qt;

  public class GetPrefabBaker : Baker<Fractalnitializer>
  {
      public override void Bake(Fractalnitializer authoring)
      {
          SpriteSheetManager.RecordSpriteSheet(authoring.sprites, "emoji");
          qt = new QuadTree(new float3(0, 0, 20), Entity.Null);
      }
  }
}
