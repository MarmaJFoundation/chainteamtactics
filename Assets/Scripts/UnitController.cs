using Anonym.Isometric;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public SpriteAnimator spriteAnimator;
    public SpriteRenderer spriteRenderer;
    public GameObject spritePivot;

    public ScriptableUnit scriptableUnit;
    public NodeInfo currentNode;
    public NodeInfo targetNode;
    public int unitID;
    public bool isPurple;
    public int currentHealth;
    public bool onAction;
    private bool onWater;
    private bool shaking;
    private int attackBuffDelay;
    private int speedBuffDelay;
    //private float poisonDelay;
    private float particleDelay;
    //private int poison;
    private int attackBuff;
    private int speedBuff;
    private int cooldown;

    private Vector3 spriteStartPos;
    private MapController mapController;
    [HideInInspector]
    public Transform shadowTransform;
    private IsoTile shadowTile;
    private SpriteRenderer shadowRenderer;
    private UnitColorInfo unitColorInfo;
    [HideInInspector]
    public HealthbarController healthbarController;
    [HideInInspector]
    public HealthbarController staticBarController;
    public UnitInfo unitInfo;
    [HideInInspector]
    public IsoTile isoTile;
    [HideInInspector]
    public string colorString;
    private float debugTimer1;
    private float debugTimer2;
    public int CurrentSpeed
    {
        get
        {
            return speedBuff > 0 ? unitInfo.speed / 10 * (speedBuff + 1) : unitInfo.speed / 10 / (speedBuff * -1 + 1);
        }
    }
    private bool Floats
    {
        get
        {
            return unitInfo.unitType == UnitType.Warlock;
        }
    }
    private bool Reviver
    {
        get
        {
            return unitInfo.unitType == UnitType.Necromancer || unitInfo.unitType == UnitType.Priest;
        }
    }
    public void Setup(MapController mapController, NodeInfo currentNode, UnitInfo unitInfo, bool isPurple, int currentCooldown)
    {
        this.mapController = mapController;
        this.isPurple = isPurple;
        this.currentNode = currentNode;
        this.unitInfo = unitInfo;
        colorString = isPurple ? "purple" : "blue";
        unitColorInfo = unitInfo.unitColorInfo;
        unitID = unitInfo.unitID;
        onAction = false;
        cooldown = currentCooldown;
        isoTile = GetComponent<IsoTile>();
        spriteStartPos = spriteRenderer.transform.localPosition;
        scriptableUnit = BaseUtils.unitDict[unitInfo.unitType];
        currentHealth = unitInfo.health;
        spriteRenderer.material = isPurple ? BaseUtils.purpleMaterial : BaseUtils.yellowMaterial;
        GameObject shadowObj = Instantiate(mapController.shadowObj, transform.parent);
        shadowTransform = shadowObj.transform;
        shadowTransform.position = transform.position;
        shadowTile = shadowTransform.GetComponent<IsoTile>();
        shadowRenderer = shadowTransform.GetComponentInChildren<SpriteRenderer>();
        SetCurrentPosOnNode();
        SetCurrentNode(currentNode);
        if (!BaseUtils.onTavern)
        {
            AddStaticBar();
        }
        AddHealthBar();
        AdjustSpeed();
        spriteRenderer.flipX = isPurple;
        spriteAnimator.Animate("Idle");
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor("_HairColor", unitColorInfo.hairColor);
        materialPropertyBlock.SetColor("_SkinColor", unitColorInfo.skinColor);
        //materialPropertyBlock.SetColor("_OutlineColor", new Color(1, 1, 1, .3f));
        materialPropertyBlock.SetColor("_RarityOneColor", unitColorInfo.rarityColors[0]);
        materialPropertyBlock.SetColor("_RarityTwoColor", unitColorInfo.rarityColors[1]);
        materialPropertyBlock.SetColor("_RarityThreeColor", unitColorInfo.rarityColors[2]);
        materialPropertyBlock.SetColor("_RarityFourColor", unitColorInfo.rarityColors[3]);
        materialPropertyBlock.SetFloat("_HasRarity", unitColorInfo.hasRarity ? 1f : 0f);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    private void AddHealthBar()
    {
        GameObject healthBarObj = Instantiate(mapController.healthbarPrefab, BaseUtils.mainCanvas.GetChild(0));
        healthbarController = healthBarObj.GetComponent<HealthbarController>();
        healthbarController.Setup(transform, currentHealth);
    }
    private void AddStaticBar()
    {
        GameObject staticBarObj = Instantiate(mapController.staticbarPrefab, isPurple ? mapController.battleRosterController.rightBars : mapController.battleRosterController.leftBars);
        HealthbarController healthbarController = staticBarObj.GetComponent<HealthbarController>();
        healthbarController.Setup(this);
        staticBarController = healthbarController;
        RosterButton rosterButton = staticBarObj.GetComponentInChildren<RosterButton>();
        rosterButton.Setup(mapController.mainMenuController, this);
    }
    private void AdjustSpeed()
    {
        spriteAnimator.animations[0].speed = 1.5f / CurrentSpeed;
        spriteAnimator.animations[1].speed = 1f / CurrentSpeed;
        spriteAnimator.animations[2].speed = 1f / CurrentSpeed;
    }
    private void Update()
    {
        if (mapController.placingUnits || BaseUtils.onTavern)
        {
            return;
        }
        if (particleDelay > 0)
        {
            particleDelay -= Time.deltaTime;
        }
        else
        {
            particleDelay = .5f;
            spriteRenderer.color = Color.white;
            /*if (poison > 0)
            {
                BaseUtils.InstantiateEffect(EffectType.PoisonEffect, transform.position + Vector3.up * scriptableUnit.effectOffset, true);
                spriteRenderer.color = BaseUtils.poisonColor;
            }*/
            if (speedBuff > 0)
            {
                BaseUtils.InstantiateEffect(EffectType.SpeedEffect, transform.position + Vector3.up * scriptableUnit.effectOffset, true);
                spriteRenderer.color = BaseUtils.speedColor;
            }
            if (speedBuff < 0)
            {
                BaseUtils.InstantiateEffect(EffectType.SlowEffect, transform.position + Vector3.up * scriptableUnit.effectOffset, true);
                spriteRenderer.color = BaseUtils.slowColor;
            }
            if (attackBuff > 0)
            {
                BaseUtils.InstantiateEffect(EffectType.AtkEffect, transform.position + Vector3.up * scriptableUnit.effectOffset, true);
                spriteRenderer.color = BaseUtils.atkColor;
            }
        }
    }
    public void ProcessBuffDelays()
    {
        if (attackBuffDelay > 0)
        {
            attackBuffDelay--;
        }
        else
        {
            attackBuff = 0;
        }
        if (speedBuffDelay > 0)
        {
            speedBuffDelay--;
        }
        else
        {
            speedBuff = 0;
            AdjustSpeed();
        }
        /*if (poisonDelay > 0)
        {
            poisonDelay -= Time.deltaTime;
        }
        else if (poison > 0)
        {
            poison--;
            poisonDelay = 1;
            //mapController.logController.AddLog($"{colorString} {unitInfo.unitType} receives 10 poison damage");
            Damage(10);
        }*/
    }
    private void LateUpdate()
    {
        if (currentHealth <= 0)
        {
            return;
        }
        shadowTransform.position = transform.position;
        //isoTile.sortingOrder.Update_SortingOrder();
        //shadowTile.Update_SortingOrder();
        //shadowTransform
        shadowRenderer.sortingOrder = shadowTile.sortingOrder.CalcSortingOrder();
        spriteRenderer.sortingOrder = isoTile.sortingOrder.CalcSortingOrder();
        //debugTimer1 += Time.deltaTime;
        //Debug.Log(isoTile.sortingOrder.IsCorrupted_LastSortingOrder());
        /*if (debugTimer1 > 2)
        {
            BaseUtils.InstantiateText(transform, isoTile.sortingOrder.CalcSortingOrder().ToString(), 0, BaseUtils.healingColor, false);
            debugTimer1 = 0;
        }
        debugTimer2 += Time.deltaTime;
        if (debugTimer2 > 1.5f)
        {
            BaseUtils.InstantiateText(transform, spriteRenderer.sortingOrder.ToString(), 0, BaseUtils.damageColor, false);
            debugTimer2 = 0;
        }*/
    }
    private IEnumerator ActionCoroutine(NodeInfo targetNode, UnitController revivedUnit)
    {
        onAction = true;
        float timer = 0;
        bool actioned = false;
        if (currentNode.position.x != targetNode.position.x)
        {
            spriteRenderer.flipX = currentNode.position.x > targetNode.position.x;
        }
        if (scriptableUnit.chargeEffect != EffectType.None)
        {
            BaseUtils.InstantiateEffect(scriptableUnit.chargeEffect, transform.position + Vector3.up * scriptableUnit.effectOffset, true);
        }
        spriteAnimator.StopAnimation("Attack", 0);
        while (timer <= 1)
        {
            if (scriptableUnit.attackRange == 1)
            {
                AnimateMeleeAttack(targetNode, timer);
            }
            timer += Time.deltaTime * CurrentSpeed * 1.5f;
            if (timer > .5f && !actioned)
            {
                actioned = true;
                spriteAnimator.StopAnimation("Attack", 1);
                if (scriptableUnit.exploEffect != EffectType.None)
                {
                    BaseUtils.InstantiateEffect(scriptableUnit.exploEffect, transform.position + Vector3.up * scriptableUnit.effectOffset, true, 1, spriteRenderer.flipX ? -1 : 1);
                }
                if (scriptableUnit.attackRange > 1)
                {
                    actioned = true;
                    if (scriptableUnit.projectileEffect != EffectType.None)
                    {
                        BaseUtils.InstantiateEffect(scriptableUnit.projectileEffect, transform.position + Vector3.up * scriptableUnit.effectOffset, targetNode.worldPos + Vector3.up, true);
                    }
                    if (scriptableUnit.unitType == UnitType.Elementalist)
                    {
                        ActionOnNode(targetNode, revivedUnit);
                    }
                    else
                    {
                        StartCoroutine(SimulateProjectile(targetNode, revivedUnit));
                    }
                }
                else
                {
                    ActionOnNode(targetNode, revivedUnit);
                }
            }
            yield return null;
        }
        if (scriptableUnit.attackRange == 1)
        {
            AnimateMeleeAttack(targetNode, 1);
        }
        spritePivot.transform.localScale = Vector3.one;
        spritePivot.transform.localRotation = Quaternion.identity;
        SetCurrentPosOnNode();
        spriteAnimator.Animate("Idle");
        if (scriptableUnit.attackRange <= 1)
        {
            onAction = false;
        }
    }
    public void ActionOnNode(NodeInfo targetNode, UnitController revivedUnit)
    {
        if (scriptableUnit.areaOfEffect > 0)
        {
            PathfindController.GetAreaOfEffectNodes(targetNode, scriptableUnit.areaOfEffect);
            for (int i = 0; i < PathfindController.neighbourNodes.Count; i++)
            {
                ResolveActionOnNode(PathfindController.neighbourNodes[i], revivedUnit);
            }
        }
        else
        {
            ResolveActionOnNode(targetNode, revivedUnit);
        }
        onAction = false;
    }
    private IEnumerator SimulateProjectile(NodeInfo targetNode, UnitController revivedUnit)
    {
        yield return new WaitForSeconds(.2f);
        ActionOnNode(targetNode, revivedUnit);
    }
    private void ResolveActionOnNode(NodeInfo targetNode, UnitController revivedUnit)
    {
        if (scriptableUnit.nodeEffect != EffectType.None)
        {
            EffectType effectType = scriptableUnit.nodeEffect;
            if (scriptableUnit.unitType == UnitType.Elementalist)
            {
                effectType = PathfindController.GetNodeElement(targetNode);
                switch (effectType)
                {
                    case EffectType.StoneVinesEffect:
                        BaseUtils.InstantiateEffect(EffectType.GroundSlamCharge, currentNode.worldPos + Vector3.up * .5f, true);
                        SpeedBuffNode(targetNode);
                        break;
                    case EffectType.FireVinesEffect:
                        BaseUtils.InstantiateEffect(EffectType.FireCharge, currentNode.worldPos + Vector3.up * .5f, true);
                        AttackBuffNode(currentNode);
                        break;
                    case EffectType.WaterVinesEffect:
                        BaseUtils.InstantiateEffect(EffectType.HealEffect, currentNode.worldPos + Vector3.up * .5f, true);
                        //HealNode(currentNode);
                        break;
                }
            }
            BaseUtils.InstantiateEffect(effectType, targetNode.worldPos + Vector3.up * .5f, true);
        }
        if (scriptableUnit.unitType == UnitType.Druid)
        {
            SummonUnit(targetNode, UnitType.Wolf, unitColorInfo);
        }
        else if (scriptableUnit.unitType == UnitType.TimeBender)
        {
            SpeedBuffNode(targetNode);
        }
        else if (scriptableUnit.unitType == UnitType.Bard)
        {
            AttackBuffNode(targetNode);
        }
        else if (Reviver)
        {
            ReviveNode(targetNode, revivedUnit);
        }
    }
    private void AttackBuffNode(NodeInfo targetNode)
    {
        if (targetNode.unit != null)
        {
            if (!targetNode.unit.isPurple ^ isPurple)
            {
                targetNode.unit.attackBuff = 3;
            }
            else
            {
                targetNode.unit.attackBuff = -3;
            }
            targetNode.unit.attackBuffDelay = 6;
        }
    }
    private void SpeedBuffNode(NodeInfo targetNode)
    {
        if (targetNode.unit != null)
        {
            if (!targetNode.unit.isPurple ^ isPurple)
            {
                targetNode.unit.speedBuff = 2;
            }
            else
            {
                targetNode.unit.speedBuff = -2;
            }
            targetNode.unit.speedBuffDelay = 4;
            targetNode.unit.AdjustSpeed();
        }
    }
    private void ReviveNode(NodeInfo targetNode, UnitController revivedUnit)
    {
        if (revivedUnit == null)
        {
            return;
        }
        if (scriptableUnit.unitType == UnitType.Priest)
        {
            SummonUnit(targetNode, revivedUnit.unitInfo.unitType, revivedUnit.unitColorInfo, revivedUnit.cooldown + 1);
        }
        else if (scriptableUnit.unitType == UnitType.Necromancer)
        {
            SummonUnit(targetNode, UnitType.Skeleton, unitColorInfo);
        }
        mapController.RemoveUnit(revivedUnit);
    }
    private void SummonUnit(NodeInfo targetNode, UnitType unitType, UnitColorInfo unitColorInfo, int newCooldown = 0)
    {
        int newID = unitID * -1 - 1000000 - cooldown * 1000000;
        ScriptableUnit unitScriptable = BaseUtils.unitDict[unitType];
        UnitInfo unitInfo = new UnitInfo(unitType, unitColorInfo, newID, unitScriptable.speed, unitScriptable.damage, unitScriptable.maxHealth, 0);
        mapController.PlaceUnit(targetNode, unitInfo, isPurple, newCooldown);
        cooldown++;
    }
    public void Heal(int healing)
    {
        //poison = 0;
        //poisonDelay = 0;
        int unitMaxHeal = unitInfo.health - currentHealth;
        currentHealth += healing;
        if (currentHealth > unitInfo.health)
        {
            currentHealth = unitInfo.health;
        }
        healing = Mathf.Clamp(healing, 0, unitMaxHeal);
        if (healing > 0)
        {
            BaseUtils.InstantiateText(transform, healing.ToString(), healing, BaseUtils.healingColor, false);
        }
        if (healthbarController != null)
        {
            healthbarController.UpdateHealth(currentHealth);
        }
        if (staticBarController != null)
        {
            staticBarController.UpdateHealth(currentHealth);
        }
    }
    public void Damage(int damage)
    {
        if (currentHealth <= 0)
        {
            return;
        }
        currentHealth -= damage;
        BaseUtils.InstantiateText(transform, damage.ToString(), damage, BaseUtils.damageColor, false);
        BaseUtils.InstantiateEffect(EffectType.Hit, transform.position + Vector3.up * (Floats ? 1 : .5f), true);
        if (healthbarController != null)
        {
            healthbarController.UpdateHealth(currentHealth);
        }
        if (staticBarController != null)
        {
            staticBarController.UpdateHealth(currentHealth);
        }
        if (!shaking)
        {
            StartCoroutine(ShakeCoroutine());
        }
        if (currentHealth <= 0)
        {
            if (healthbarController != null && healthbarController.gameObject != null)
            {
                Destroy(healthbarController.gameObject);
            }
            currentNode.unit = null;
            onAction = false;
            attackBuff = 0;
            speedBuff = 0;
            StopAllCoroutines();
            StartCoroutine(DeathCoroutine());
        }
    }
    private IEnumerator ShakeCoroutine()
    {
        shaking = true;
        Vector3 fromPos = spriteRenderer.transform.localPosition;
        float timer = 0;
        while (timer <= 1)
        {
            spriteRenderer.color = Color.Lerp(Color.white, Color.red, timer.Evaluate(CurveType.DamagedCurve));
            spriteRenderer.transform.localPosition = spriteStartPos + .2f * timer.Evaluate(CurveType.DamagedCurve) * Vector3.right;
            timer += Time.deltaTime * 5;
            yield return null;
        }
        spriteRenderer.color = Color.white;
        spriteRenderer.transform.localPosition = fromPos;
        shaking = false;
    }
    private IEnumerator DeathCoroutine()
    {
        spriteAnimator.Animate("Death");
        float timer = 0;
        while (timer <= 1)
        {
            spriteRenderer.color = Color.Lerp(Color.white, Color.red, timer.Evaluate(CurveType.DamagedCurve));
            spriteRenderer.transform.localPosition = spriteStartPos + .5f * timer.Evaluate(CurveType.PeakParabol) * Vector3.up;
            timer += Time.deltaTime * 5;
            yield return null;
        }
        spriteRenderer.color = Color.white;
        if (!onWater)
        {
            spriteRenderer.transform.localPosition = Vector3.up * .5f;
        }
    }
    private void AnimateMeleeAttack(NodeInfo gotoNode, float timer)
    {
        if (Floats)
        {
            transform.position = Vector3.Lerp(currentNode.WorldPosFloat, gotoNode.WorldPosFloat, timer.Evaluate(CurveType.AttackCurve));
        }
        else
        {
            transform.position = Vector3.Lerp(currentNode.worldPos, gotoNode.worldPos, timer.Evaluate(CurveType.AttackCurve));
        }
    }
    public void Idle()
    {
        if (currentHealth <= 0)
        {
            return;
        }
        spriteAnimator.Animate("Idle");
    }
    public void Move(Vector2Int moveToNode)
    {
        NodeInfo gotoNode = MapController.nodeGrid[moveToNode.x, moveToNode.y];
        if (gotoNode == currentNode)
        {
            SetCurrentPosOnNode();
            spriteAnimator.Animate("Idle");
            return;
        }
        currentNode.unit = null;
        SetCurrentNode(gotoNode);
        NodeInfo fromNode = currentNode;
        currentNode = gotoNode;
        spriteAnimator.Animate("Walk");
        StartCoroutine(OnActionCoroutine());
        StartCoroutine(MoveCoroutine(fromNode, gotoNode));
    }
    private IEnumerator OnActionCoroutine()
    {
        onAction = true;
        yield return new WaitForSeconds(.05f);
        onAction = false;
    }
    public void Action(Vector2Int actionNode, UnitController revivedUnit)
    {
        NodeInfo gotoNode = MapController.nodeGrid[actionNode.x, actionNode.y];
        StartCoroutine(ActionCoroutine(gotoNode, revivedUnit));
    }
    private IEnumerator MoveCoroutine(NodeInfo fromNode, NodeInfo gotoNode)
    {
        float nodeDistance = Vector2Int.Distance(fromNode.position, gotoNode.position);
        if (fromNode.position.x != gotoNode.position.x)
        {
            spriteRenderer.flipX = fromNode.position.x > gotoNode.position.x;
        }
        float timer = 0;
        while (timer <= 1)
        {
            float weight = 1;
            if (Floats)
            {
                transform.position = Vector3.Lerp(fromNode.WorldPosFloat, gotoNode.WorldPosFloat, timer);
            }
            else
            {
                weight = 1 + BaseUtils.tileDict[gotoNode.tileType].weight * .2f;
                transform.position = Vector3.Lerp(fromNode.worldPos, gotoNode.worldPos, timer);
            }
            timer += Time.deltaTime * CurrentSpeed / weight / nodeDistance * 2f;
            yield return null;
        }
        if (Floats)
        {
            transform.position = gotoNode.WorldPosFloat;
        }
        else
        {
            transform.position = gotoNode.worldPos;
        }
        SetCurrentPosOnNode();
        spriteAnimator.Animate("Idle");
    }
    private void SetCurrentNode(NodeInfo gotoNode)
    {
        if (OnWater(gotoNode))
        {
            shadowTransform.gameObject.SetActive(false);
            spriteRenderer.material = isPurple ? BaseUtils.purpleWaterMaterial : BaseUtils.yellowWaterMaterial;
            onWater = true;
        }
        else
        {
            shadowTransform.gameObject.SetActive(true);
            spriteRenderer.material = isPurple ? BaseUtils.purpleMaterial : BaseUtils.yellowMaterial;
            onWater = false;
        }
        gotoNode.unit = this;
    }
    private void SetCurrentPosOnNode()
    {
        if (Floats)
        {
            transform.position = currentNode.WorldPosFloat;
        }
        else
        {
            transform.position = currentNode.worldPos;
        }
    }
    private bool OnWater(NodeInfo checkNode)
    {
        return BaseUtils.tileDict[checkNode.tileType].hasMask && !Floats;
    }
    private void OnDestroy()
    {
        if (healthbarController != null && healthbarController.gameObject != null)
        {
            healthbarController.StopAllCoroutines();
            Destroy(healthbarController.gameObject);
        }
        if (staticBarController != null && staticBarController.gameObject != null)
        {
            staticBarController.StopAllCoroutines();
            Destroy(staticBarController.gameObject);
        }
        if (shadowTransform != null && shadowTransform.gameObject != null)
        {
            Destroy(shadowTransform.gameObject);
        }
    }
}
