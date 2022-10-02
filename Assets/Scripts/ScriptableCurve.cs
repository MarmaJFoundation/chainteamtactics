using UnityEngine;

public enum CurveType
{
    None = 0,
    SmoothParabol = 1,
    PeakParabol = 2,
    AttackCurve = 3,
    DamagedCurve = 4,
    ShakeCurve = 5,
    DexAttackRot = 6,
    MagicAttackRot = 7,
    MagicAttackScale = 8,
    EaseOut = 9,
    DeathCurve = 10,
    EaseIn = 11,
    JumpCurve = 12,
    MeleeAttackCurve = 13,
    ChestOpenCurve = 14,
    ChestEaseInCurve = 15,
    InverseEaseOut = 16,
    ChestEaseOut = 17,
    CameraEaseOut = 18,
    ShakeCurve2 = 19,
    SmoothParabolInverted = 20,
    BossShakeCurve = 21
}
[CreateAssetMenu(fileName = "New Curve", menuName = "Scriptable/Curve")]
public class ScriptableCurve : ScriptableObject
{
    public CurveType curveType;
    public AnimationCurve animationCurve;
}