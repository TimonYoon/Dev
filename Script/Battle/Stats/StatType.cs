public enum StatType
{
    NotDefined,
    HP,
    CurHP,
    MaxHP,
    HPRegen,
    CurShield,
    MaxShield,
    AttackPower,
    AttackPowerMelee,
    AttackPowerRange,
    AttackPowerPhysical,
    AttackPowerMagical,

    /// <summary> 보스 상대로 데미지 증가율 </summary>
    IncreaseDamageRateBoss,

    /// <summary> 힐량 증가 </summary>
    IncreaseHealRatio,

    /// <summary> 사거리 증가 (원거리 스킬만) </summary>
    IncreaseAttackRange,

    /// <summary> 회피 성공률 </summary>
    Dodge,

    /// <summary> 관통 비율 (대상 방어력 무시 비율) </summary>
    PenetrateRatio,

    AttackSpeed,

    /// <summary> 관통 </summary>
    LifeDrainRate,

    ReflectDamageRate,

  

    /// <summary> 방어력 </summary>
    DefensePower,

    /// <summary> 방어력에 의한 피해감소율로 감소한 피해 이후 또 추가 피해 감소 </summary>
    ReduceDamageRate,
    ReducePhysicalDamageRate,
    ReduceMagicalDamageRate,
    MoveSpeed,



    //####################### 내정 영웅 용
    ProductionPower,
    CollectPower,
    TaxPower
}
