using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    None = 0,
    WarlockCharge = 1,
    WarlockEffect = 2,
    WarlockProject = 3,
    Hit = 4,
    KnightExplo = 5,
    AssassinExplo = 6,
    ExecExplo = 7,
    SquireExplo = 8,
    DogExplo = 9,
    SkeletonExplo = 10,
    WolfSpawnCharge = 11,
    WolfSpawnEffect = 12,
    SkelSpawnCharge = 13,
    SkelSpawnEffect = 14,
    ThunderCharge = 15,
    ThunderEffect = 16,
    HealEffect = 17,
    HealProject = 18,
    SongCharge = 19,
    SongEffect = 20,
    HolyCharge = 21,
    HolyEffect = 22,
    TimeCharge = 23,
    TimeEffect = 24,
    FireCharge = 25,
    FireEffect = 26,
    FireProject = 27,
    GroundSlamCharge = 28,
    GroudSlamEffect = 29,
    VinesCharge = 30,
    FireVinesEffect = 31,
    WaterVinesEffect = 32,
    StoneVinesEffect = 33,
    VinesEffect = 34,
    ArrowProject = 35,
    PoisonEffect = 36,
    AtkEffect = 37,
    SpeedEffect = 38,
    SlowEffect = 39,
    PuffEffect = 40
}
[CreateAssetMenu(fileName = "New Effect", menuName = "Scriptable/Effect")]
public class ScriptableEffect : ScriptableObject
{
    public EffectType effectType;
    public GameObject effectPrefab;
}