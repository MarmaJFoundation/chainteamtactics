using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FightNode
{
    public Vector2Int position;
    public TileType tileType;
    public bool blocked;
    public UnitFightInfo unit = null;
    public List<UnitFightInfo> bodies = new List<UnitFightInfo>();
}
public class UnitFightInfo
{
    public ScriptableUnit scriptableUnit;
    public FightNode currentNode;
    public UnitInfo unitInfo;
    public bool isPurple;
    public int currentHealth;

    public FightNode targetNode;
    public int attackBuff;
    public int speedBuff;
    //#bubuSpeed
    public int turnDelay;
    public int turnStep;
    //public int poison;
    public bool justAttacked;
    public int attackDelay;
    public int attackBuffDelay;
    public int speedBuffDelay;
    //public int poisonDelay;
    public int attentionDelay;
    public int deathDelay;
    public int cooldown;
    public int UnitID
    {
        get
        {
            return unitInfo.unitID;
        }
    }
    public bool Floats
    {
        get
        {
            return unitInfo.unitType == UnitType.Warlock;
        }
    }
    public bool Reviver
    {
        get
        {
            return unitInfo.unitType == UnitType.Necromancer || unitInfo.unitType == UnitType.Priest;
        }
    }
    public int MaxHealth
    {
        get
        {
            return unitInfo.health;
        }
    }
    public int CurrentDamage
    {
        get
        {
            return unitInfo.damage + attackBuff;
        }
    }
    //#bubuSpeed
    public int CurrentSpeed
    {
        get
        {
            return speedBuff > 0 ? unitInfo.speed * (speedBuff + 1) : unitInfo.speed / (speedBuff * -1 + 1);
        }
    }
    public UnitFightInfo(UnitInfo unitInfo, bool isPurple, FightNode currentNode, int cooldown)
    {
        this.unitInfo = unitInfo;
        this.isPurple = isPurple;
        this.currentNode = currentNode;
        this.cooldown = cooldown;
        currentHealth = unitInfo.health;
        scriptableUnit = BaseUtils.unitDict[unitInfo.unitType];
    }
}
public class Vector2CTT
{
    public int x;
    public int y;
    public Vector2CTT(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public static implicit operator Vector2CTT(Vector2Int vector)
    {
        return new Vector2CTT(vector.x, vector.y);
    }
}
public struct ActionStruct
{
    public int unitID;
    public Vector2CTT moveToNode;
    public Vector2CTT targetNode;
    public List<int> targets;
    public int damage;

    public ActionStruct(int unitID, Vector2Int moveToNode)
    {
        this.unitID = unitID;
        this.moveToNode = moveToNode;
        targetNode = null;
        targets = null;
        damage = 0;
    }
    public ActionStruct(int unitID, Vector2Int targetNode, List<int> targets, int damage)
    {
        this.unitID = unitID;
        moveToNode = null;
        this.targetNode = targetNode;
        this.targets = targets;
        this.damage = damage;
    }
    public ActionStruct(int unitID)
    {
        this.unitID = unitID;
        moveToNode = null;
        targetNode = null;
        targets = null;
        damage = 0;
    }
}
[System.Serializable]
public class FightStruct
{
    public bool purpleWins;
    public List<ActionStruct> actionStructs;
    public List<FullPlayerLoadoutData[]> playerLoadouts;

    public FightStruct(bool purpleWins, List<ActionStruct> actionStructs)
    {
        this.purpleWins = purpleWins;
        this.actionStructs = actionStructs;
        playerLoadouts = new List<FullPlayerLoadoutData[]>();
    }
}
public class BattleController : MonoBehaviour
{
    private readonly List<UnitFightInfo> yellowUnits = new List<UnitFightInfo>();
    private readonly List<UnitFightInfo> purpleUnits = new List<UnitFightInfo>();
    private readonly List<FightNode> neighbourNodes = new List<FightNode>();
    private FightNode[,] nodeGrid;
    public TextAsset mapInfo;
    private List<ActionStruct> actionStructs;
    public FightStruct GenerateFightStruct(int mapIndex, List<UnitInfo> purpleUnitInfos, List<Vector2Int> purplePositions, List<UnitInfo> yellowUnitInfos, List<Vector2Int> yellowPositions)
    {
        nodeGrid = new FightNode[29, 13];
        List<MapData> mapDatas = XMLGenerator.DeserializeObject(mapInfo.text, typeof(List<MapData>)) as List<MapData>;
        for (int i = 0; i < mapDatas[mapIndex].tileInfos.Count; i++)
        {
            Vector2Int rawPos = mapDatas[mapIndex].tileInfos[i].position;
            FightNode nodeInfo = new FightNode
            {
                position = new Vector2Int(rawPos.x + 14, rawPos.y + 7),
                blocked = mapDatas[mapIndex].tileInfos[i].blocked,
                tileType = mapDatas[mapIndex].tileInfos[i].tileType
            };
            nodeGrid[rawPos.x + 14, rawPos.y + 7] = nodeInfo;
        }
        for (int i = 0; i < purpleUnitInfos.Count; i++)
        {
            AddUnit(true, nodeGrid[purplePositions[i].x, purplePositions[i].y], purpleUnitInfos[i]);
        }
        for (int i = 0; i < yellowUnitInfos.Count; i++)
        {
            AddUnit(false, nodeGrid[yellowPositions[i].x, yellowPositions[i].y], yellowUnitInfos[i]);
        }
        actionStructs = new List<ActionStruct>();
        int breaker = 1000;
        while (HasUnits(purpleUnits) && HasUnits(yellowUnits))
        {
            breaker--;
            if (breaker < 0)
            {
                Debug.Log("broke");
                break;
            }
            for (int i = 0; i < purpleUnits.Count; i++)
            {
                if (ProcessUnitBehavior(purpleUnits[i], out ActionStruct actionStruct))
                {
                    actionStructs.Add(actionStruct);
                }
            }
            for (int i = 0; i < yellowUnits.Count; i++)
            {
                if (ProcessUnitBehavior(yellowUnits[i], out ActionStruct actionStruct))
                {
                    actionStructs.Add(actionStruct);
                }
            }
        }
        return new FightStruct(HasUnits(purpleUnits), actionStructs);
    }
    private bool ProcessUnitBehavior(UnitFightInfo unitFightInfo, out ActionStruct actionStruct)
    {
        if (unitFightInfo.currentHealth <= 0)
        {
            unitFightInfo.deathDelay++;
            if (unitFightInfo.deathDelay > 20)
            {
                RemoveUnit(unitFightInfo);
            }
            actionStruct = new ActionStruct(unitFightInfo.UnitID);
            return false;
        }
        bool processedDelay = false;
        if (unitFightInfo.attackBuffDelay > 0)
        {
            unitFightInfo.attackBuffDelay--;
            processedDelay = true;
        }
        else
        {
            unitFightInfo.attackBuff = 0;
        }
        if (unitFightInfo.speedBuffDelay > 0)
        {
            unitFightInfo.speedBuffDelay--;
            processedDelay = true;
        }
        else
        {
            unitFightInfo.speedBuff = 0;
        }
        //#bubuSpeed
        unitFightInfo.turnDelay += unitFightInfo.CurrentSpeed;
        if (unitFightInfo.turnDelay < (unitFightInfo.turnStep + 1) * 200)
        {
            actionStruct = new ActionStruct(unitFightInfo.UnitID);
            return processedDelay;
        }
        unitFightInfo.turnStep++;
        /*if (unitFightInfo.poisonDelay > 0)
        {
            unitFightInfo.poisonDelay--;
        }
        else if (unitFightInfo.poison > 0)
        {
            unitFightInfo.poison--;
            unitFightInfo.poisonDelay = 1;
            DamageUnit(unitFightInfo, 10);
            if (unitFightInfo.currentHealth <= 0)
            {
                actionStruct = new ActionStruct(unitFightInfo.UnitID);
                return false;
            }
        }*/
        if (unitFightInfo.attackDelay > 0)
        {
            unitFightInfo.attackDelay--;
        }
        if (unitFightInfo.Reviver)
        {
            unitFightInfo.targetNode = GetClosestBody(unitFightInfo);
        }
        else if (unitFightInfo.scriptableUnit.supports)
        {
            unitFightInfo.targetNode = GetClosestAlly(unitFightInfo);
        }
        else
        {
            unitFightInfo.targetNode = GetClosestTarget(unitFightInfo);
        }
        if (unitFightInfo.targetNode == unitFightInfo.currentNode)
        {
            actionStruct = new ActionStruct(unitFightInfo.UnitID);
            return processedDelay;
        }
        float distanceToTarget = Vector2Int.Distance(unitFightInfo.currentNode.position, unitFightInfo.targetNode.position);
        if (!unitFightInfo.Reviver && !unitFightInfo.scriptableUnit.supports)
        {
            bool tooClose = unitFightInfo.scriptableUnit.attackRange > 1 && distanceToTarget <= unitFightInfo.scriptableUnit.attackRange * 1.5f;
            bool shouldRun = !unitFightInfo.scriptableUnit.dontRun && unitFightInfo.currentHealth <= unitFightInfo.MaxHealth * .3f;
            if ((shouldRun || tooClose) && unitFightInfo.justAttacked)
            {
                FightNode gotoNode = GetFurthestNode(unitFightInfo);
                if (gotoNode != unitFightInfo.currentNode)
                {
                    unitFightInfo.justAttacked = false;
                    MoveUnit(unitFightInfo, gotoNode);
                    actionStruct = new ActionStruct(unitFightInfo.UnitID, gotoNode.position);
                    return true;
                }
            }
        }
        if (distanceToTarget <= unitFightInfo.scriptableUnit.attackRange * 1.5f)
        {
            if (unitFightInfo.attackDelay <= 0)
            {
                unitFightInfo.justAttacked = true;
                List<int> targets = new List<int>();
                FightNode finalNode = unitFightInfo.targetNode;
                if (unitFightInfo.scriptableUnit.selfCast)
                {
                    finalNode = unitFightInfo.currentNode;
                }
                FightNode targetNode = finalNode;
                if (unitFightInfo.scriptableUnit.areaOfEffect > 0)
                {
                    GetAreaOfEffectNodes(finalNode, unitFightInfo.scriptableUnit.areaOfEffect);
                    for (int i = 0; i < neighbourNodes.Count; i++)
                    {
                        int targetID = ResolveActionOnNode(unitFightInfo, neighbourNodes[i], out _);
                        if (targetID != -1)
                        {
                            targets.Add(targetID);
                        }
                    }
                }
                else
                {
                    int targetID = ResolveActionOnNode(unitFightInfo, finalNode, out targetNode);
                    if (targetID != -1)
                    {
                        targets.Add(targetID);
                    }
                }
                unitFightInfo.attackDelay += unitFightInfo.scriptableUnit.attackSpeed + unitFightInfo.cooldown;
                if (targetNode == null)
                {
                    actionStruct = new ActionStruct(unitFightInfo.UnitID);
                    return processedDelay;
                }
                else
                {
                    actionStruct = new ActionStruct(unitFightInfo.UnitID, targetNode.position, targets, unitFightInfo.CurrentDamage / 10);
                    return true;
                }
            }
            else
            {
                if (!unitFightInfo.Reviver && !unitFightInfo.scriptableUnit.supports)
                {
                    actionStruct = new ActionStruct(unitFightInfo.UnitID);
                    return processedDelay;
                }
                else
                {
                    FightNode gotoNode = GetClosestNode(unitFightInfo, unitFightInfo.targetNode);
                    MoveUnit(unitFightInfo, gotoNode);
                    actionStruct = new ActionStruct(unitFightInfo.UnitID, gotoNode.position);
                    return true;
                }
            }
        }
        else
        {
            FightNode gotoNode = GetClosestNode(unitFightInfo, unitFightInfo.targetNode);
            MoveUnit(unitFightInfo, gotoNode);
            actionStruct = new ActionStruct(unitFightInfo.UnitID, gotoNode.position);
            return true;
        }
    }
    private void MoveUnit(UnitFightInfo unitFightInfo, FightNode gotoNode)
    {
        unitFightInfo.currentNode.unit = null;
        unitFightInfo.currentNode = gotoNode;
        unitFightInfo.currentNode.unit = unitFightInfo;
    }
    private int ResolveActionOnNode(UnitFightInfo unitFightInfo, FightNode targetNode, out FightNode returnNode)
    {
        returnNode = targetNode;
        if (unitFightInfo.scriptableUnit.unitType == UnitType.Druid)
        {
            GetFreeNeighbours(unitFightInfo.currentNode);
            if (neighbourNodes.Count > 0)
            {
                AddSummonUnit(unitFightInfo, UnitType.Wolf, unitFightInfo.unitInfo.unitColorInfo);
                returnNode = neighbourNodes[0];
            }
            else
            {
                returnNode = null;
            }
            return -1;
        }
        if (unitFightInfo.scriptableUnit.unitType == UnitType.TimeBender)
        {
            if (targetNode.unit != null)
            {
                if (!targetNode.unit.isPurple ^ unitFightInfo.isPurple)
                {
                    targetNode.unit.speedBuff = 2;
                }
                else
                {
                    targetNode.unit.speedBuff = -2;
                }
                targetNode.unit.speedBuffDelay = 4;
            }
            return -1;
        }
        if (unitFightInfo.scriptableUnit.unitType == UnitType.Bard)
        {
            if (targetNode.unit != null)
            {
                if (!targetNode.unit.isPurple ^ unitFightInfo.isPurple)
                {
                    targetNode.unit.attackBuff = 3;
                }
                else
                {
                    targetNode.unit.attackBuff = -3;
                }
                targetNode.unit.attackBuffDelay = 6;
            }
            return -1;
        }
        if (unitFightInfo.Reviver)
        {
            if (targetNode.bodies.Count > 0)
            {
                UnitFightInfo revivedUnit = targetNode.bodies[0];
                if (revivedUnit.isPurple == unitFightInfo.isPurple)
                {
                    GetFreeNeighbours(targetNode);
                    if (neighbourNodes.Count > 0)
                    {
                        if (unitFightInfo.scriptableUnit.unitType == UnitType.Priest)
                        {
                            AddSummonUnit(unitFightInfo, revivedUnit.unitInfo.unitType, revivedUnit.unitInfo.unitColorInfo, revivedUnit.cooldown + 1);
                        }
                        else if (unitFightInfo.scriptableUnit.unitType == UnitType.Necromancer)
                        {
                            AddSummonUnit(unitFightInfo, UnitType.Skeleton, unitFightInfo.unitInfo.unitColorInfo);
                        }
                        int revivedUnitID = revivedUnit.UnitID;
                        RemoveUnit(revivedUnit);
                        returnNode = neighbourNodes[0];
                        return revivedUnitID;
                    }
                    else
                    {
                        returnNode = null;
                    }
                }
                else
                {
                    returnNode = null;
                }
            }
            else
            {
                returnNode = null;
            }
            return -1;
        }
        if (unitFightInfo.scriptableUnit.supports)
        {
            if (targetNode.unit != null)
            {
                if (!targetNode.unit.isPurple ^ unitFightInfo.isPurple)
                {
                    targetNode.unit.currentHealth += unitFightInfo.CurrentDamage / 10;
                    if (targetNode.unit.currentHealth > targetNode.unit.MaxHealth)
                    {
                        targetNode.unit.currentHealth = targetNode.unit.MaxHealth;
                    }
                    unitFightInfo.cooldown++;
                    return targetNode.unit.UnitID;
                }
                return -1;
            }
            return -1;
        }
        if (targetNode.unit != null)
        {
            if (targetNode.unit.isPurple ^ unitFightInfo.isPurple)
            {
                /*if (unitFightInfo.scriptableUnit.unitType == UnitType.Assassin)
                {
                    targetNode.unit.poison = 4;
                    targetNode.unit.poisonDelay = 1;
                }*/
                if (targetNode.unit.currentHealth > 0)
                {
                    int returnID = targetNode.unit.UnitID;
                    DamageUnit(targetNode.unit, unitFightInfo.CurrentDamage / 10);
                    return returnID;
                }
                return -1;
            }
            return -1;
        }
        return -1;
    }
    private void AddSummonUnit(UnitFightInfo unitFightInfo, UnitType unitType, UnitColorInfo unitColorInfo, int cooldown = 0)
    {
        int newID = unitFightInfo.UnitID * -1 - 1000000 - unitFightInfo.cooldown * 1000000;
        ScriptableUnit scriptableUnit = BaseUtils.unitDict[unitType];
        UnitInfo unitInfo = new UnitInfo(unitType, unitColorInfo, newID, scriptableUnit.speed, scriptableUnit.damage, scriptableUnit.maxHealth, 0);
        AddUnit(unitFightInfo.isPurple, neighbourNodes[0], unitInfo, cooldown);
        unitFightInfo.cooldown++;
    }
    private void DamageUnit(UnitFightInfo unit, int damage)
    {
        unit.currentHealth -= damage;
        if (unit.currentHealth <= 0)
        {
            unit.currentNode.bodies.Add(unit);
            unit.currentNode.unit = null;
            //Debug.Log(" " + unit.unitInfo.unitID + "," + unit.unitInfo.unitType + ",died ");
        }
    }
    private void AddUnit(bool isPurple, FightNode gotoNode, UnitInfo unitInfo, int cooldown = 0)
    {
        if (isPurple)
        {
            purpleUnits.Add(new UnitFightInfo(unitInfo, isPurple, gotoNode, cooldown));
            gotoNode.unit = purpleUnits[purpleUnits.Count - 1];
        }
        else
        {
            yellowUnits.Add(new UnitFightInfo(unitInfo, isPurple, gotoNode, cooldown));
            gotoNode.unit = yellowUnits[yellowUnits.Count - 1];
        }
    }
    private void RemoveUnit(UnitFightInfo unitFightInfo)
    {
        if (unitFightInfo.isPurple)
        {
            purpleUnits.Remove(unitFightInfo);
        }
        else
        {
            yellowUnits.Remove(unitFightInfo);
        }
        unitFightInfo.currentNode.bodies.Remove(unitFightInfo);
    }
    private FightNode GetClosestNode(UnitFightInfo unit, FightNode targetNode)
    {
        GetFreeNeighbours(unit.currentNode, unit.Floats);
        if (neighbourNodes.Count > 0)
        {
            neighbourNodes.Sort((left, right) => OrderByDistance(left.position, right.position, targetNode.position));
            return neighbourNodes[0];
        }
        return unit.currentNode;
    }
    private void GetAreaOfEffectNodes(FightNode fromNode, int searchRange)
    {
        neighbourNodes.Clear();
        if (searchRange == 0)
        {
            neighbourNodes.Add(fromNode);
            return;
        }
        int xRange = 0;
        for (int y = -searchRange; y <= searchRange; y++)
        {
            for (int x = -xRange; x <= xRange; x++)
            {
                int goX = fromNode.position.x + x;
                int goY = fromNode.position.y + y;
                if (!OutOfBounds(goX, goY))
                {
                    if (!nodeGrid[goX, goY].blocked)
                    {
                        neighbourNodes.Add(nodeGrid[goX, goY]);
                    }
                }
            }
            if (y < 0)
            {
                xRange++;
            }
            else
            {
                xRange--;
            }
        }
    }
    private void GetFreeNeighbours(FightNode fromNode, bool floats = false)
    {
        neighbourNodes.Clear();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                int goX = fromNode.position.x + x;
                int goY = fromNode.position.y + y;
                if (!OutOfBounds(goX, goY) && NodeWalkable(nodeGrid[goX, goY], floats))
                {
                    neighbourNodes.Add(nodeGrid[goX, goY]);
                }
            }
        }
    }
    private bool OutOfBounds(int posX, int posY)
    {
        return posX < 0 || posY < 0 || posX >= 29 || posY >= 13;
    }
    private static bool NodeWalkable(FightNode checkNode, bool floats)
    {
        return checkNode.unit == null && !checkNode.blocked && (floats || checkNode.tileType != TileType.Lava);
    }
    private FightNode GetFurthestNode(UnitFightInfo unit)
    {
        GetFreeNeighbours(unit.currentNode, unit.Floats);
        if (neighbourNodes.Count > 0)
        {
            neighbourNodes.Sort((left, right) => OrderByDistance(left.position, right.position, unit.targetNode.position));
            return neighbourNodes[neighbourNodes.Count - 1];
        }
        return unit.currentNode;
    }
    private FightNode GetClosestBody(UnitFightInfo fromUnit)
    {
        List<FightNode> targetNodes = new List<FightNode>();
        if (fromUnit.isPurple)
        {
            for (int i = 0; i < purpleUnits.Count; i++)
            {
                UnitFightInfo unit = purpleUnits[i];
                if (unit.currentHealth > 0 || unit.UnitID == fromUnit.UnitID || unit.scriptableUnit.supports)
                {
                    continue;
                }
                targetNodes.Add(unit.currentNode);
            }
        }
        else
        {
            for (int i = 0; i < yellowUnits.Count; i++)
            {
                UnitFightInfo unit = yellowUnits[i];
                if (unit.currentHealth > 0 || unit.UnitID == fromUnit.UnitID || unit.scriptableUnit.supports)
                {
                    continue;
                }
                targetNodes.Add(unit.currentNode);
            }
        }
        if (targetNodes.Count == 0)
        {
            return fromUnit.currentNode;
        }
        targetNodes.Sort((left, right) => OrderByDistance(left.position, right.position, fromUnit.currentNode.position));
        return targetNodes[0];
    }
    private FightNode GetClosestAlly(UnitFightInfo fromUnit)
    {
        List<FightNode> targetNodes = new List<FightNode>();
        if (fromUnit.isPurple)
        {
            for (int i = 0; i < purpleUnits.Count; i++)
            {
                UnitFightInfo unit = purpleUnits[i];
                if (unit.currentHealth <= 0 || unit.UnitID == fromUnit.UnitID || unit.scriptableUnit.supports)
                {
                    continue;
                }
                targetNodes.Add(unit.currentNode);
            }
        }
        else
        {
            for (int i = 0; i < yellowUnits.Count; i++)
            {
                UnitFightInfo unit = yellowUnits[i];
                if (unit.currentHealth <= 0 || unit.UnitID == fromUnit.UnitID || unit.scriptableUnit.supports)
                {
                    continue;
                }
                targetNodes.Add(unit.currentNode);
            }
        }
        if (targetNodes.Count == 0)
        {
            return fromUnit.currentNode;
        }
        targetNodes.Sort((left, right) => OrderByDistance(left.position, right.position, fromUnit.currentNode.position));
        return targetNodes[0];
    }
    private FightNode GetClosestTarget(UnitFightInfo fromUnit)
    {
        List<FightNode> targetNodes = new List<FightNode>();
        if (!fromUnit.isPurple)
        {
            for (int i = 0; i < purpleUnits.Count; i++)
            {
                UnitFightInfo unit = purpleUnits[i];
                if (unit.currentHealth <= 0)
                {
                    continue;
                }
                targetNodes.Add(unit.currentNode);
            }
        }
        else
        {
            for (int i = 0; i < yellowUnits.Count; i++)
            {
                UnitFightInfo unit = yellowUnits[i];
                if (unit.currentHealth <= 0)
                {
                    continue;
                }
                targetNodes.Add(unit.currentNode);
            }
        }
        if (targetNodes.Count == 0)
        {
            return fromUnit.currentNode;
        }
        targetNodes.Sort((left, right) => OrderByDistance(left.position, right.position, fromUnit.currentNode.position));
        return targetNodes[0];
    }
    public static int OrderByDistance(Vector2Int left, Vector2Int right, Vector2Int position)
    {
        return Comparer<float>.Default.Compare(
            Vector2Int.Distance(left, position),
            Vector2Int.Distance(right, position)
        );
    }
    private bool HasUnits(List<UnitFightInfo> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].currentHealth > 0)
            {
                return true;
            }
        }
        return false;
    }
}
