using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PeopleManager))]
public class PeopleManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            var pm = (PeopleManager)target;
            if (GUILayout.Button("Construct House"))
            {
                pm.ConstructHouse();
            }
            if (GUILayout.Button("Construct Large House"))
            {
                pm.ConstructLargeHouse();
            }
        }
    }
}
