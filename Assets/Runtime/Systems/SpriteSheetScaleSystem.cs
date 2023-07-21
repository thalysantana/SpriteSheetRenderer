using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;

public partial class SpriteSheetScaleSystem : SystemBase {
  protected override void OnUpdate(){
    Entities.WithName("SpriteSheetScaleSystem").WithChangeFilter<LocalTransform>().ForEach(
      (ref SpriteMatrix renderData, in LocalTransform transform) => {
        renderData.matrix.w = transform.Scale;
      }
    ).ScheduleParallel();
  }
}
