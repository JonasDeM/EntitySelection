using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.Editor;
using Unity.Rendering;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("EntitySelection Tool", typeof(EntitySelectionProxy))]
public class EntitySelectionEditorTool : EditorTool, IDrawSelectedHandles
{
    public void OnDrawHandles()
    {
        EntitySelectionProxy entityProxy = target as EntitySelectionProxy;
        if (entityProxy == null || !entityProxy.Exists)
            return;

        var world = entityProxy.World;
        if (!world.EntityManager.HasComponent<WorldRenderBounds>(entityProxy.Entity))
            return;
        var bounds = world.EntityManager.GetComponentData<WorldRenderBounds>(entityProxy.Entity).Value;
        
        Handles.color = Color.green;
        Handles.DrawWireCube(bounds.Center, bounds.Size);
    }

    public override void OnToolGUI(EditorWindow window)
    {
        // Nothing yet
    }
}
