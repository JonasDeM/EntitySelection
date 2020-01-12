using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySelectionWireCube : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
    }
}
