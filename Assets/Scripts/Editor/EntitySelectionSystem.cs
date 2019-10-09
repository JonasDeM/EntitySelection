// Author: Jonas De Maeseneer

using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.Editor;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

[AlwaysUpdateSystem]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public class EntitySelectionSystem : ComponentSystem
{
    // static sceneview data
    private static Camera _sceneViewCam;
    private static bool _clicked;
    private static Vector2 _mousePos;
    
    // instance members
    public EntitySelectionProxy CurrentSelectedEntityProxy;
    private RenderTexture _objectIdTexture;
    private Shader _colorIDShader;
    private Texture2D _selectedPixel;

    // would be great if we didn't have to store the version or could put it in the rendered color
    // another nice thing could be that we only have 1 material just change per instance color
    readonly Dictionary<int, VersionAndMaterial> _entityIndexToMat = new Dictionary<int, VersionAndMaterial>();
    struct VersionAndMaterial
    {
        public Material Material;
        public int Version;
    }

    protected override void OnCreate()
    {
        _colorIDShader = Shader.Find("Unlit/EntityIdShader");
        _selectedPixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        CurrentSelectedEntityProxy = ScriptableObject.CreateInstance<EntitySelectionProxy>();
        SceneView.duringSceneGui += UpdateView;
    }

    protected override void OnUpdate()
    {
        if (_sceneViewCam == null)
            return;
        
        // initial creation + on window resize
        if (_objectIdTexture == null || _sceneViewCam.targetTexture.width != _objectIdTexture.width
            || _sceneViewCam.targetTexture.height != _objectIdTexture.height)
        {
            _objectIdTexture = new RenderTexture(_sceneViewCam.targetTexture.width, _sceneViewCam.targetTexture.height, 0)
            {
                antiAliasing = 1,
                filterMode = FilterMode.Point,
                autoGenerateMips = false,
                depth = 24
            };
        }

        if (_clicked)
        {
            _clicked = false;
            // Rendering Unique color per entity
            RenderEntityIDs();
            // Getting the pixel at the mouse position and converting the color to an entity
            SelectEntity();
        }

    }

    private void RenderEntityIDs()
    {
        CommandBuffer cmd = new CommandBuffer();
        cmd.SetRenderTarget(_objectIdTexture);
        cmd.ClearRenderTarget(true, true, Color.white);
        cmd.SetViewProjectionMatrices(_sceneViewCam.worldToCameraMatrix, _sceneViewCam.projectionMatrix);
        Entities.ForEach((Entity e, RenderMesh mesh, ref LocalToWorld localToWorld) =>
        {
            if (!_entityIndexToMat.ContainsKey(e.Index))
            {
                Material m = new Material(_colorIDShader);
                m.color = IndexToColor(e.Index);
                _entityIndexToMat[e.Index] = new VersionAndMaterial()
                {
                    Version = e.Version,
                    Material = m
                };
            }

            var matAndVersion = _entityIndexToMat[e.Index];
            matAndVersion.Version = e.Version;
            _entityIndexToMat[e.Index] = matAndVersion;

            cmd.DrawMesh(mesh.mesh, localToWorld.Value, matAndVersion.Material);
        });
        Graphics.ExecuteCommandBuffer(cmd);
    }
    private void SelectEntity()
    {
        Entity selectedEntity = new Entity()
        {
            Index = ColorToIndex(GetColorAtMousePos(_mousePos, _objectIdTexture))
        };
        if (_entityIndexToMat.ContainsKey(selectedEntity.Index))
        {
            selectedEntity.Version = _entityIndexToMat[selectedEntity.Index].Version;
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
        _selectedPixel.ReadPixels(new Rect(posLocalToSceneView.x, posLocalToSceneView.y, 1, 1), 0, 0, false);
        _selectedPixel.Apply();
        RenderTexture.active = null;
    
        return _selectedPixel.GetPixel(0, 0);
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
            if (!_sceneViewCam == sceneView.camera)
            {
                _sceneViewCam = sceneView.camera;
            }
            
            if(Event.current.keyCode == KeyCode.Alpha1)
            {
                _clicked = true;
                _mousePos = Event.current.mousePosition;
            }
        }
    }

    protected override void OnDestroy()
    {
        if(Selection.activeObject == CurrentSelectedEntityProxy)
        {
            Selection.activeObject = null;
        }
        Object.Destroy(CurrentSelectedEntityProxy);
        
        // clear static variables
        _sceneViewCam = null;
        _clicked = false;
    }
}
