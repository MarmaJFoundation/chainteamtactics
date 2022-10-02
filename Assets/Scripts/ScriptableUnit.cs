using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    None = 0,
    Squire = 1,
    Knight = 2,
    Mage = 3,
    Chemist = 4,
    Executioner = 5,
    Marksman = 6,
    Priest = 7,
    Warlock = 8,
    Druid = 9,
    Bard = 10,
    Assassin = 11,
    Elementalist = 12,
    Necromancer = 13,
    Paladin = 14,
    TimeBender = 15,
    Skeleton = 16,
    Wolf = 17
}
[CreateAssetMenu(fileName = "New Unit", menuName = "Scriptable/Unit")]
public class ScriptableUnit : ScriptableObject
{
    public UnitType unitType;
    public EffectType chargeEffect;
    public EffectType exploEffect;
    public EffectType projectileEffect;
    public EffectType nodeEffect;
    public string abrevName;
    public string pronoun;
    public string[] description;
    public Sprite defaultImage;
    public float effectOffset;
    public GameObject unitPrefab;
    public bool supports;
    public bool dontRun;
    public bool summon;
    public bool selfCast;
    public int attackRange;
    public int maxHealth;
    public int damage;
    public int speed;
    public int areaOfEffect;
    public int unitTier;
    public int attackSpeed;
}
