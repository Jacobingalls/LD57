using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cathedral))]
public class CathedralEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            var c = (Cathedral)target;
            if (GUILayout.Button("Yeet Mortal Soul"))
            {
                c.YeetMortalSoul();
            }
        }
    }
}
