using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlantGenerator))]

public class PlantGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlantGenerator script = (PlantGenerator) target;

        if (DrawDefaultInspector())
        {
            script.Init();
        }
    }
}
