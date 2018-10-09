using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Place))]
class ConnectLineHandleExampleScript : Editor
{
    void OnSceneGUI()
    {
        Place connectedObjects = target as Place;
        if (connectedObjects.connectedPlaceList == null)
            return;

        Vector3 center = connectedObjects.transform.position;
        for (int i = 0; i < connectedObjects.connectedPlaceList.Count; i++)
        {
            GameObject connectedObject = connectedObjects.connectedPlaceList[i].gameObject;
            if (connectedObject)
            {
                Handles.color = Color.magenta;
                Handles.DrawLine(center, connectedObject.transform.position);
            }
            else
            {
                Handles.DrawLine(center, Vector3.zero);
            }
        }
    }
}

