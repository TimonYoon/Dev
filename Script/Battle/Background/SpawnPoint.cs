using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
    public enum UnitType
    {
        Blue,
        Red
    }

    public enum RangeType
    {
        Melee,
        Mid,
        Range
    }

    public UnitType unitType = UnitType.Red;
    public RangeType rangeType = RangeType.Melee;

    [System.NonSerialized]
    public bool isAssigned = false;
}
