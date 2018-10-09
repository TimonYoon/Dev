using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialAnimation : MonoBehaviour {

    new public Renderer renderer;

    Material[] materials;

    public string propertyName;

    public AnimationCurve animCurve;

    public float speed;
        
    void Start ()
    {
        if (!renderer)
            return;

        p = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(p);

        startTime = Time.time;
	}

    float startTime;

    MaterialPropertyBlock p;

    void Update ()
    {
        float t = (Time.time * speed) % (int)(Time.time * speed);

        float a = animCurve.Evaluate(t);

        p.SetFloat(propertyName, a);

        renderer.SetPropertyBlock(p);
	}
}
