﻿﻿// Author: Jonas De Maeseneer

using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using Unity.Entities.Editor;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
  
[ExecuteAlways]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public class EntitySelectionSystem : ComponentSystem
{
    // Instance members
    public EntitySelectionProxy CurrentSelectedEntityProxy;
    private RenderTexture _objectIDRenderTarget;
    private Shader _colorIDShader;
    private Texture2D _objectID1x1Texture;

    // Material
    private readonly Dictionary<int, int> _entityIndexToVersion = new Dictionary<int, int>();
    private static readonly int ColorPropertyID = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock _idMaterialPropertyBlock;
    private Material _idMaterial;
    
    // cached reflection variable to find actual scene view camera rect
    private static readonly PropertyInfo _sceneViewCameraRectProp = typeof(SceneView).GetProperty("cameraRect", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    protected override void OnCreate()
    {
        _colorIDShader = Shader.Find("Unlit/EntityIdShader");
        _objectID1x1Texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        CurrentSelectedEntityProxy = ScriptableObject.CreateInstance<EntitySelectionProxy>();
        SceneView.duringSceneGui += UpdateView;
        _idMaterialPropertyBlock = new MaterialPropertyBlock();
        _idMaterial = new Material(_colorIDShader);
    }

    private void OnClicked(Vector2 mousePos, Camera camera, int renderTextureWidth, int renderTextureHeight)
    {
        // Needs to happen when the scene changed
        if (_idMaterial == null)
        {
            OnCreate();
        }
        
        // Initial creation + on window resize
        if (_objectIDRenderTarget == null ||
            renderTextureWidth  != _objectIDRenderTarget.width ||
            renderTextureHeight != _objectIDRenderTarget.height)
        {
            _objectIDRenderTarget = new RenderTexture(renderTextureWidth, renderTextureHeight, 0)
            {
                antiAliasing = 1,
                filterMode = FilterMode.Point,
                autoGenerateMips = false,
                depth = 24
            };
        }

        // Rendering Unique color per entity
        RenderEntityIDs(camera);
        // Getting the pixel at the mouse position and converting the color to an entity
        SelectEntity(mousePos);
    }

    private void RenderEntityIDs(Camera camera)
    {
        var cmd = new CommandBuffer();
        cmd.SetRenderTarget(_objectIDRenderTarget);
        cmd.ClearRenderTarget(true, true, new Color(0,0,0,0));
        cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
        Entities.ForEach((Entity e, RenderMesh mesh, ref LocalToWorld localToWorld) =>
        {
            if (mesh.mesh == null)
            {
                return;
            }
            _entityIndexToVersion[e.Index] = e.Version;
            _idMaterialPropertyBlock.SetColor(ColorPropertyID, IndexToColor(e.Index));
            cmd.DrawMesh(mesh.mesh, localToWorld.Value, _idMaterial, mesh.subMesh, 0, _idMaterialPropertyBlock);
        });
        Graphics.ExecuteCommandBuffer(cmd);
    }

    private void SelectEntity(Vector2 mousePos)
    {
        var selectedEntity = new Entity
        {
            Index = ColorToIndex(GetColorAtMousePos(mousePos, _objectIDRenderTarget))
        };
        if (_entityIndexToVersion.ContainsKey(selectedEntity.Index))
        {
            selectedEntity.Version = _entityIndexToVersion[selectedEntity.Index];
            CurrentSelectedEntityProxy.SetEntity(World, selectedEntity);

            Selection.activeObject = CurrentSelectedEntityProxy;
        }
        else
        {
            Selection.activeObject = null;
        }
    }

    private Color GetColorAtMousePos(Vector2 posLocalToSceneView, RenderTexture objectIdTex)
    {
        RenderTexture.active = objectIdTex;

        // clicked outside of scene view
        if (posLocalToSceneView.x < 0 || posLocalToSceneView.x > objectIdTex.width
            || posLocalToSceneView.y < 0 || posLocalToSceneView.y > objectIdTex.height)
        {
            return new Color(0,0,0,0); // results in Entity.Null
        }

        // handles when the edges of the screen are clicked
        posLocalToSceneView.x = Mathf.Clamp(posLocalToSceneView.x, 0, objectIdTex.width - 1);
        posLocalToSceneView.y = Mathf.Clamp(posLocalToSceneView.y, 0, objectIdTex.height - 1);
        
        _objectID1x1Texture.ReadPixels(new Rect(posLocalToSceneView.x, posLocalToSceneView.y, 1, 1), 0, 0, false);
        _objectID1x1Texture.Apply();
        RenderTexture.active = null;
        
        return _objectID1x1Texture.GetPixel(0, 0);
    }

    private static Color32 IndexToColor(int index)
    {
        var bytes = BitConverter.GetBytes(index);
        return new Color32(bytes[0], bytes[1], bytes[2], bytes[3]);
    }

    private static int ColorToIndex(Color32 color)
    {
        var bytes = new byte[] { color.r, color.g, color.b, color.a };
        return BitConverter.ToInt32(bytes, 0);
    }

    // Get input from the SceneView
    private static void UpdateView(SceneView sceneView)
    {
        if (Event.current != null)
        {
            if (Event.current.keyCode == KeyCode.Alpha1 && Event.current.type == EventType.KeyDown)
            {
                foreach (var world in World.All)
                {
                    var system = world.GetExistingSystem<EntitySelectionSystem>();

                    Rect cameraRect;
                    try
                    {
                        cameraRect = (Rect) _sceneViewCameraRectProp.GetValue(sceneView);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"EntitySelectionSystem couldn't determine camera rect of scene view. Using fallback rect. \n {e}");
                        cameraRect = sceneView.position;
                    }
                    
                    system?.OnClicked(Event.current.mousePosition, sceneView.camera, (int) cameraRect.width, (int) cameraRect.height);
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        if (Selection.activeObject == CurrentSelectedEntityProxy)
        {
            Selection.activeObject = null;
        }

        Object.DestroyImmediate(CurrentSelectedEntityProxy);
        Object.DestroyImmediate(_idMaterial);
        Object.DestroyImmediate(_objectID1x1Texture);
    }

    protected override void OnUpdate()
    {
        // Everything happens in OnClicked which is called on an editor event
    }
}
