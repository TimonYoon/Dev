using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSetting : MonoBehaviour {

    private void Start()
    {
        Canvas can = GetComponent<Canvas>();
        can.renderMode = RenderMode.ScreenSpaceCamera;
        can.worldCamera = Camera.main;
    }
}
