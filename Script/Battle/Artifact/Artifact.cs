using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact
{
    public Artifact(ArtifactBaseData data)
    {
        baseData = data;
    }

    public ArtifactBaseData baseData { get; private set; }

    /// <summary> 해당 유물 중첩 소유 갯수 </summary>
    public int stack { get; set; }
}