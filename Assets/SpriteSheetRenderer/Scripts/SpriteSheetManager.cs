﻿using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public abstract class SpriteSheetManager {
  private static EntityManager entityManager;
  public static List<RenderInformation> renderInformation = new List<RenderInformation>();

  public static EntityManager EntityManager {
    get {
      if(entityManager == default)
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
      return entityManager;
    }
  }

  public static Entity Instantiate(List<IComponentData> componentData, string spriteSheetName)
  {
    EntityArchetype archetype = EntityManager.CreateArchetype(
      typeof(Position2D),
      typeof(Rotation2D),
      typeof(LocalTransform),
      typeof(LifeTime),
      //required params
      typeof(SpriteIndex),
      typeof(SpriteSheetAnimation),
      typeof(SpriteSheetMaterial),
      typeof(SpriteSheetColor),
      typeof(SpriteMatrix),
      typeof(BufferHook)
    );

    return Instantiate(archetype, componentData, spriteSheetName);
  }
  
  public static Entity Instantiate(List<IComponentData> componentData, SpriteSheetAnimator animator)
  {
    EntityArchetype archetype = EntityManager.CreateArchetype(
      typeof(Position2D),
      typeof(Rotation2D),
      typeof(LocalTransform),
      typeof(LifeTime),
      //required params
      typeof(SpriteIndex),
      typeof(SpriteSheetAnimation),
      typeof(SpriteSheetMaterial),
      typeof(SpriteSheetColor),
      typeof(SpriteMatrix),
      typeof(BufferHook)
    );

    return Instantiate(archetype, componentData, animator);
  }
  
  public static Entity Instantiate(EntityArchetype archetype, List<IComponentData> componentDatas, string spriteSheetName) {
    Entity e = EntityManager.CreateEntity(archetype);
    Material material = SpriteSheetCache.GetMaterial(spriteSheetName);
    int bufferID = DynamicBufferManager.AddDynamicBuffers(DynamicBufferManager.GetEntityBuffer(material), material);
    foreach(IComponentData Idata in componentDatas)
      EntityManager.SetComponentData(e, (dynamic)Idata);

    var spriteSheetMaterial = new SpriteSheetMaterial { material = material };
    BufferHook bh = new BufferHook { bufferID = bufferID, bufferEnityID = DynamicBufferManager.GetEntityBufferID(spriteSheetMaterial) };
    EntityManager.SetComponentData(e, bh);
    EntityManager.SetSharedComponentManaged(e, spriteSheetMaterial);
    return e;
  }

  public static Entity Instantiate(EntityArchetype archetype, List<IComponentData> componentDatas, SpriteSheetAnimator animator){
    Entity e = EntityManager.CreateEntity(archetype);
    animator.currentAnimationIndex = animator.defaultAnimationIndex;
    SpriteSheetAnimationData startAnim = animator.animations[animator.defaultAnimationIndex];
    int maxSprites = startAnim.sprites.Length;
    Material material = SpriteSheetCache.GetMaterial(animator.animations[animator.defaultAnimationIndex].animationName);
    int bufferID = DynamicBufferManager.AddDynamicBuffers(DynamicBufferManager.GetEntityBuffer(material), material);
    foreach(IComponentData Idata in componentDatas)
      EntityManager.SetComponentData(e, (dynamic)Idata);

    var spriteSheetMaterial = new SpriteSheetMaterial { material = material };
    BufferHook bh = new BufferHook { bufferID = bufferID, bufferEnityID = DynamicBufferManager.GetEntityBufferID(spriteSheetMaterial) };
    EntityManager.SetComponentData(e, bh);
    EntityManager.SetComponentData(e, new SpriteSheetAnimation {  maxSprites = maxSprites , play = startAnim.playOnStart, samples = startAnim.samples, repetition = startAnim.repetition});
    EntityManager.SetComponentData(e, new SpriteIndex { Value = startAnim.startIndex });
    EntityManager.SetSharedComponentManaged(e, spriteSheetMaterial);
    animator.managedEntity = e;
    SpriteSheetCache.entityAnimator.Add(e, animator);
    return e;
  }

  public static void SetAnimation(Entity e, SpriteSheetAnimationData animation){
    int bufferEnityID = EntityManager.GetComponentData<BufferHook>(e).bufferEnityID;
    int bufferID = EntityManager.GetComponentData<BufferHook>(e).bufferID;
    Material oldMaterial = DynamicBufferManager.GetMaterial(bufferEnityID);
    string oldAnimation = SpriteSheetCache.GetMaterialName(oldMaterial);
    if(animation.animationName != oldAnimation) {
      Material material = SpriteSheetCache.GetMaterial(animation.animationName);
      var spriteSheetMaterial = new SpriteSheetMaterial { material = material };

      DynamicBufferManager.RemoveBuffer(oldMaterial, bufferID);

      //use new buffer
      bufferID = DynamicBufferManager.AddDynamicBuffers(DynamicBufferManager.GetEntityBuffer(material), material);
      BufferHook bh = new BufferHook { bufferID = bufferID, bufferEnityID = DynamicBufferManager.GetEntityBufferID(spriteSheetMaterial) };

      EntityManager.SetSharedComponentManaged(e, spriteSheetMaterial);
      EntityManager.SetComponentData(e, bh);
    }
    EntityManager.SetComponentData(e, new SpriteSheetAnimation { maxSprites = animation.sprites.Length, play = animation.playOnStart, samples = animation.samples, repetition = animation.repetition, elapsedFrames = 0 });
    EntityManager.SetComponentData(e, new SpriteIndex { Value = animation.startIndex });
    MarkDirty<SpriteSheetColor>(e);
    MarkDirty<SpriteIndex>(e);
    MarkDirty<SpriteMatrix>(e);
  }

  public static void SetAnimation(EntityCommandBuffer commandBuffer, Entity e, SpriteSheetAnimationData animation, BufferHook hook) {
    Material oldMaterial = DynamicBufferManager.GetMaterial(hook.bufferEnityID);
    string oldAnimation = SpriteSheetCache.GetMaterialName(oldMaterial);
    if(animation.animationName != oldAnimation) {
      Material material = SpriteSheetCache.GetMaterial(animation.animationName);
      var spriteSheetMaterial = new SpriteSheetMaterial { material = material };

      //clean old buffer
      DynamicBufferManager.RemoveBuffer(oldMaterial, hook.bufferID);

      //use new buffer
      int bufferID = DynamicBufferManager.AddDynamicBuffers(DynamicBufferManager.GetEntityBuffer(material), material);
      BufferHook bh = new BufferHook { bufferID = bufferID, bufferEnityID = DynamicBufferManager.GetEntityBufferID(spriteSheetMaterial) };

      commandBuffer.SetSharedComponentManaged(e, spriteSheetMaterial);
      commandBuffer.SetComponent(e, bh);
    }
    commandBuffer.SetComponent(e, new SpriteSheetAnimation { maxSprites = animation.sprites.Length, play = animation.playOnStart, samples = animation.samples, repetition = animation.repetition, elapsedFrames = 0 });
    commandBuffer.SetComponent(e, new SpriteIndex { Value = animation.startIndex });
    MarkDirty<SpriteSheetColor>(e, commandBuffer);
    MarkDirty<SpriteIndex>(e, commandBuffer);
    MarkDirty<SpriteMatrix>(e, commandBuffer);
  }

  public static void MarkDirty<T>(Entity e) where T : unmanaged , IComponentData {
    EntityManager.SetComponentData(e, entityManager.GetComponentData<T>(e));
  }
  public static void MarkDirty<T>(Entity e,EntityCommandBuffer ecb) where T : unmanaged, IComponentData {
    ecb.SetComponent(e, entityManager.GetComponentData<T>(e));
  }

  public static void UpdateEntity(Entity entity, IComponentData componentData) {
    EntityManager.SetComponentData(entity, (dynamic)componentData);
  }

  public static void UpdateEntity(EntityCommandBuffer commandBuffer,Entity entity, IComponentData componentData){
    commandBuffer.SetComponent(entity, (dynamic)componentData);
  }

  public static void DestroyEntity(Entity e, string materialName) {
    Material material = SpriteSheetCache.GetMaterial(materialName);
    int bufferID = EntityManager.GetComponentData<BufferHook>(e).bufferID;
    DynamicBufferManager.RemoveBuffer(material, bufferID);
    EntityManager.DestroyEntity(e);
  }

  public static void DestroyEntity(EntityCommandBuffer commandBuffer, Entity e, BufferHook hook) {
    commandBuffer.DestroyEntity(e);
    Material material = DynamicBufferManager.GetMaterial(hook.bufferEnityID);
    DynamicBufferManager.RemoveBuffer(material, hook.bufferID);
  }

  public static void RecordSpriteSheet(Sprite[] sprites, string spriteSheetName, int spriteCount = 0) {
    KeyValuePair<Material, float4[]> atlasData = SpriteSheetCache.BakeSprites(sprites, spriteSheetName);
    SpriteSheetMaterial material = new SpriteSheetMaterial { material = atlasData.Key };
    DynamicBufferManager.GenerateBuffers(material, spriteCount);
    DynamicBufferManager.BakeUvBuffer(material, atlasData);
    renderInformation.Add(new RenderInformation(material.material, DynamicBufferManager.GetEntityBuffer(material.material)));
  }

  public static void RecordAnimator(SpriteSheetAnimator animator){
    foreach(SpriteSheetAnimationData animation in animator.animations)
      RecordSpriteSheet(animation.sprites, animation.name);
  }

  public static void CleanBuffers() {
    for(int i = 0; i < renderInformation.Count; i++)
      renderInformation[i].DestroyBuffers();
    renderInformation.Clear();
  }

  public static void ReleaseUvBuffer(int bufferID) {
    if(renderInformation[bufferID].uvBuffer != null)
      renderInformation[bufferID].uvBuffer.Release();
  }

  public static void ReleaseBuffer(int bufferID) {
    if(renderInformation[bufferID].matrixBuffer != null)
      renderInformation[bufferID].matrixBuffer.Release();
    if(renderInformation[bufferID].colorsBuffer != null)
      renderInformation[bufferID].colorsBuffer.Release();
    //if(renderInformation[bufferID].uvBuffer != null)
      //renderInformation[bufferID].uvBuffer.Release();
    if(renderInformation[bufferID].indexBuffer != null)
      renderInformation[bufferID].indexBuffer.Release();
  }

}
