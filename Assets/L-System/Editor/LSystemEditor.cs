using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LSystem))]
public class LSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LSystem script = (LSystem)target;

        if(DrawDefaultInspector())
        {
            script.Initiate();
        }
    }
}
