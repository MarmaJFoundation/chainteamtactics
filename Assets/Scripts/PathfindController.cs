using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SupportNode
{
    public NodeInfo node;
    public float unitHealth;

    public SupportNode(NodeInfo node, float unitHealth)
    {
        this.node = node;
        this.unitHealth = unitHealth;
    }
}

public class PathfindController : MonoBehaviour
{
    public readonly static List<NodeInfo> neighbourNodes = new List<NodeInfo>();
    private readonly static List<NodeInfo> openSet = new List<NodeInfo>();
    private readonly static HashSet<NodeInfo> closedSet = new HashSet<NodeInfo>();
    private readonly static Queue<NodeInfo> nodeQueue = new Queue<NodeInfo>();

    /*public static NodeInfo GetSpawnNode(UnitController fromUnitController, NodeInfo fromNodeInfo)
    {
        closedSet.Clear();
        nodeQueue.Clear();
        nodeQueue.Enqueue(fromNodeInfo);
        while (nodeQueue.Count > 0)
        {
            NodeInfo checkNodeInfo = nodeQueue.Dequeue();
            if (NodeInfoSpawnable(fromUnitController, checkNodeInfo))
            {
                return checkNodeInfo;
            }
            if (!closedSet.Contains(checkNodeInfo))
            {
                closedSet.Add(checkNodeInfo);
                GetNearNeighbours(checkNodeInfo);
            }
        }
        return fromNodeInfo;
    }
    private static bool NodeInfoSpawnable(UnitController fromUnitController, NodeInfo checkNodeInfo)
    {
        return checkNodeInfo.unit == null && !checkNodeInfo.blocked &&
               (fromUnitController.floating || checkNodeInfo.tileType != TileType.Lava);
    }*/
    public static EffectType GetNodeElement(NodeInfo nodeInfo)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int goX = nodeInfo.position.x + x;
                int goY = nodeInfo.position.y + y;
                if (!OutOfBounds(goX, goY))
                {
                    NodeInfo checkNode = MapController.nodeGrid[goX, goY];
                    if (checkNode.tileType == TileType.Water || checkNode.tileType == TileType.BlockWater)
                    {
                        return EffectType.WaterVinesEffect;
                    }
                    if (checkNode.tileType == TileType.Lava || checkNode.tileType == TileType.Earth)
                    {
                        return EffectType.FireVinesEffect;
                    }
                    if (checkNode.tileType == TileType.Marble || checkNode.tileType == TileType.BlockMarble || checkNode.tileType == TileType.Stone)
                    {
                        return EffectType.StoneVinesEffect;
                    }
                }
            }
        }
        return EffectType.VinesEffect;
    }
    /*public static NodeInfo GetRandomFreeNode(UnitController unit)
    {
        List<NodeInfo> walkableNodes = new List<NodeInfo>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if ((Mathf.Abs(x) + Mathf.Abs(y) == 2) || (x == 0 && y == 0))
                {
                    continue;
                }
                int goX = unit.currentNode.position.x + x;
                int goY = unit.currentNode.position.y + y;
                if (!OutOfBounds(goX, goY))
                {
                    NodeInfo checkNode = MapController.nodeGrid[goX, goY];
                    if (NodeWalkable(unit, checkNode))
                    {
                        walkableNodes.Add(checkNode);
                    }
                }
            }
        }
        if (walkableNodes.Count > 0)
        {
            return walkableNodes[0];
        }
        return unit.currentNode;
    }
    public static NodeInfo GetFurthestNode(UnitController unit, NodeInfo targetNode)
    {
        GetFreeNeighbours(unit);
        if (neighbourNodes.Count > 0)
        {
            neighbourNodes.Sort((left, right) => OrderByDistance(left.position, right.position, targetNode.position));
            return neighbourNodes[neighbourNodes.Count - 1];
        }
        return unit.currentNode;
    }
    public static NodeInfo GetClosestNode(UnitController unit, NodeInfo targetNode)
    {
        GetFreeNeighbours(unit);
        if (neighbourNodes.Count > 0)
        {
            neighbourNodes.Sort((left, right) => OrderByDistance(left.position, right.position, targetNode.position));
            return neighbourNodes[0];
        }
        return unit.currentNode;
    }
    public static NodeInfo GetClosestBody(UnitController fromUnit)
    {
        List<SupportNode> targetNodes = new List<SupportNode>();
        if (fromUnit.isPurple)
        {
            for (int i = 0; i < MapController.purpleUnits.Count; i++)
            {
                UnitController unit = MapController.purpleUnits[i];
                if (unit.currentHealth > 0 || unit.unitID == fromUnit.unitID || unit.scriptableUnit.supports)
                {
                    continue;
                }
                targetNodes.Add(new SupportNode(unit.currentNode, unit.MaxHealth));
            }
        }
        else
        {
            for (int i = 0; i < MapController.yellowUnits.Count; i++)
            {
                UnitController unit = MapController.yellowUnits[i];
                if (unit.currentHealth > 0 || unit.unitID == fromUnit.unitID || unit.scriptableUnit.supports)
                {
                    continue;
                }
                targetNodes.Add(new SupportNode(unit.currentNode, unit.MaxHealth));
            }
        }
        if (targetNodes.Count == 0)
        {
            return fromUnit.currentNode;
        }
        targetNodes.Sort((left, right) => OrderByDistance(left.node.position, right.node.position, fromUnit.currentNode.position));
        return targetNodes[0].node;
    }
    public static NodeInfo GetClosestAlly(UnitController fromUnit, bool ignoreHP = false)
    {
        List<SupportNode> targetNodes = new List<SupportNode>();
        if (fromUnit.isPurple)
        {
            for (int i = 0; i < MapController.purpleUnits.Count; i++)
            {
                UnitController unit = MapController.purpleUnits[i];
                if (unit.currentHealth <= 0 || unit.unitID == fromUnit.unitID)
                {
                    continue;
                }
                if ((ignoreHP || unit.currentHealth < unit.MaxHealth) && !unit.scriptableUnit.supports)
                {
                    targetNodes.Add(new SupportNode(unit.currentNode, unit.MaxHealth / unit.currentHealth));
                }
            }
        }
        else
        {
            for (int i = 0; i < MapController.yellowUnits.Count; i++)
            {
                UnitController unit = MapController.yellowUnits[i];
                if (unit.currentHealth <= 0 || unit.unitID == fromUnit.unitID)
                {
                    continue;
                }
                if ((ignoreHP || unit.currentHealth < unit.MaxHealth) && !unit.scriptableUnit.supports)
                {
                    targetNodes.Add(new SupportNode(unit.currentNode, unit.MaxHealth / unit.currentHealth));
                }
            }
        }
        if (targetNodes.Count == 0)
        {
            return fromUnit.currentNode;
        }
        targetNodes.Sort((left, right) => OrderByHealth(left, right, fromUnit.currentNode.position));
        return targetNodes[0].node;
    }
    public static NodeInfo GetClosestTarget(UnitController fromUnit)
    {
        List<NodeInfo> targetNodes = new List<NodeInfo>();
        if (!fromUnit.isPurple)
        {
            for (int i = 0; i < MapController.purpleUnits.Count; i++)
            {
                UnitController unit = MapController.purpleUnits[i];
                if (unit.currentHealth <= 0)
                {
                    continue;
                }
                targetNodes.Add(unit.currentNode);
            }
        }
        else
        {
            for (int i = 0; i < MapController.yellowUnits.Count; i++)
            {
                UnitController unit = MapController.yellowUnits[i];
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
    }*/
    public static void GetAreaOfEffectNodes(NodeInfo fromNode, int searchRange)
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
                    if (!MapController.nodeGrid[goX, goY].blocked)
                    {
                        neighbourNodes.Add(MapController.nodeGrid[goX, goY]);
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
    /*public static NodeInfo RequestPathToTarget(UnitController unit, NodeInfo fromNode, NodeInfo gotoNode)
    {
        closedSet.Clear();
        openSet.Clear();
        openSet.Add(fromNode);
        NodeInfo currentNode = fromNode;
        bool pathFail = true;
        while (openSet.Count > 0)
        {
            currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost ||
                    openSet[i].FCost == currentNode.FCost &&
                    openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            if (currentNode == gotoNode)
            {
                pathFail = false;
                break;
            }
            if (closedSet.Count > 200)
            {
                pathFail = true;
                break;
            }
            GetPathNeighbours(unit, currentNode, gotoNode);
        }
        ProcessResultPathing(fromNode, currentNode);
        if (neighbourNodes.Count == 0 || pathFail)
        {
            return fromNode;
        }
        else
        {
            NodeInfo finalNode = neighbourNodes[0];
            if (finalNode != gotoNode)
            {
                return neighbourNodes[0];
            }
            else
            {
                return fromNode;
            }
        }
    }
    private static void ProcessResultPathing(NodeInfo fromNode, NodeInfo gotoNode)
    {
        NodeInfo currentNode = gotoNode;
        neighbourNodes.Clear();
        while (currentNode != fromNode)
        {
            neighbourNodes.Add(MapController.nodeGrid[currentNode.position.x, currentNode.position.y]);
            currentNode = MapController.nodeGrid[currentNode.parentPos.x, currentNode.parentPos.y];
        }
        neighbourNodes.Reverse();
    }*/
    public static void GetFreeNeighbours(NodeInfo fromNode, bool floats = false)
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
                if (!OutOfBounds(goX, goY) && NodeWalkable(MapController.nodeGrid[goX, goY], floats))
                {
                    neighbourNodes.Add(MapController.nodeGrid[goX, goY]);
                }
            }
        }
    }
    /*public static void GetNearNeighbours(NodeInfo fromNode)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if ((Mathf.Abs(x) + Mathf.Abs(y) == 2) || (x == 0 && y == 0))
                {
                    continue;
                }
                int goX = fromNode.position.x + x;
                int goY = fromNode.position.y + y;
                if (!OutOfBounds(goX, goY) && !closedSet.Contains(MapController.nodeGrid[goX, goY]))
                {
                    nodeQueue.Enqueue(MapController.nodeGrid[goX, goY]);
                }
            }
        }
    }
    private static void GetPathNeighbours(UnitController unit, NodeInfo currentNode, NodeInfo targetNode)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if ((Mathf.Abs(x) + Mathf.Abs(y) == 2) || (x == 0 && y == 0))
                {
                    continue;
                }
                int goX = currentNode.position.x + x;
                int goY = currentNode.position.y + y;
                if (!OutOfBounds(goX, goY))
                {
                    NodeInfo checkNode = MapController.nodeGrid[goX, goY];
                    if (NodeWalkable(unit, checkNode, targetNode == checkNode) && !closedSet.Contains(checkNode))
                    {
                        int newCost = currentNode.gCost + NodeDistance(currentNode, checkNode, unit.floating);
                        bool isOpen = (openSet.IndexOf(checkNode) > -1);
                        if (newCost < checkNode.gCost || !isOpen)
                        {
                            checkNode.gCost = newCost;
                            checkNode.hCost = NodeDistance(checkNode, targetNode);
                            checkNode.parentPos = currentNode.position;
                            if (!isOpen)
                            {
                                openSet.Add(checkNode);
                            }
                        }
                    }
                }
            }
        }
    }*/
    private static bool OutOfBounds(int posX, int posY)
    {
        return posX < 0 || posY < 0 || posX >= 29 || posY >= 13;
    }
    private static bool NodeWalkable(NodeInfo checkNode, bool floats)
    {
        return checkNode.unit == null && !checkNode.blocked && (floats || checkNode.tileType != TileType.Lava);
    }
    public static int OrderByDistance(Vector2Int left, Vector2Int right, Vector2Int position)
    {
        return Comparer<float>.Default.Compare(
            Vector2.Distance(left, position),
            Vector2.Distance(right, position)
        );
    }
    public static int OrderByHealth(SupportNode left, SupportNode right, Vector2Int position)
    {
        return Comparer<float>.Default.Compare(
            Vector2.Distance(left.node.position, position) - left.unitHealth,
            Vector2.Distance(right.node.position, position) - right.unitHealth
        );
    }
    private static int NodeDistance(NodeInfo nodeA, NodeInfo nodeB, bool ignoreWeight = false)
    {
        int distX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int distY = Mathf.Abs(nodeA.position.y - nodeB.position.y);
        int weight = ignoreWeight ? 0 : BaseUtils.tileDict[nodeA.tileType].weight + BaseUtils.tileDict[nodeB.tileType].weight;

        if (distX > distY)
        {
            return (14 * distY + 10 + (distX - distY)) + weight;
        }
        return (14 * distX + 10 + (distY - distX)) + weight;
    }
}
