using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerate))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerate map = target as MapGenerate;
        if (DrawDefaultInspector ()) {
            map.GenerateMap ();
        }

        if (GUILayout.Button("Generate Map")) {
            map.GenerateMap ();
        }
        
    }
}
