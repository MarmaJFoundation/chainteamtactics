using Anonym.Isometric;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapInfo
{
    public RoomNotificationType notify_type;
    public string winner_id;
    public string mapOwner;
    public MapType mapType;
    public bool hasFightStruct;
    public bool creatingRoom;
    public bool fromNotification;
    public int rounds;
    public int betType;
    public FightStruct fightStruct;
    public List<UnitInfo> unitInfos;
    public List<Vector2Int> unitPositions;

    public MapInfo(RoomNotificationData roomNotificationData)
    {
        fightStruct = roomNotificationData.fightStruct;
        mapOwner = roomNotificationData.room_id;
        hasFightStruct = roomNotificationData.hasFightStruct;
        mapType = (MapType)roomNotificationData.map_index;
        notify_type = roomNotificationData.notify_type;
        winner_id = roomNotificationData.winner_id;
        rounds = roomNotificationData.rounds;
        betType = roomNotificationData.betType;
        unitInfos = new List<UnitInfo>();
        unitPositions = new List<Vector2Int>();
        for (int k = 0; k < 2; k++)
        {
            for (int i = 0; i < fightStruct.playerLoadouts[k].Length; i++)
            {
                int.TryParse(fightStruct.playerLoadouts[k][i].token_id, out int unitID);
                unitInfos.Add(new UnitInfo((UnitType)fightStruct.playerLoadouts[k][i].unit_type, BaseUtils.GetUnitColorInfo(unitID), unitID, fightStruct.playerLoadouts[k][i].speed, fightStruct.playerLoadouts[k][i].damage, fightStruct.playerLoadouts[k][i].health, 0));
                unitPositions.Add(fightStruct.playerLoadouts[k][i].position);
            }
        }
    }
    public MapInfo(int mapIndex)
    {
        creatingRoom = true;
        mapType = (MapType)mapIndex;
        mapOwner = Database.databaseStruct.playerAccount;
        unitInfos = new List<UnitInfo>();
        unitPositions = new List<Vector2Int>();
    }
    public MapInfo(string mapOwner, FullRoomInfo fullRoomInfo)
    {
        this.mapOwner = mapOwner;
        mapType = (MapType)fullRoomInfo.mapIndex;
        unitInfos = new List<UnitInfo>();
        unitPositions = new List<Vector2Int>();
        for (int i = 0; i < fullRoomInfo.loadout.Length; i++)
        {
            int.TryParse(fullRoomInfo.loadout[i].token_id, out int unitID);
            unitInfos.Add(new UnitInfo((UnitType)fullRoomInfo.loadout[i].unit_type, BaseUtils.GetUnitColorInfo(unitID), unitID, fullRoomInfo.loadout[i].speed, fullRoomInfo.loadout[i].damage, fullRoomInfo.loadout[i].health, 0));
            unitPositions.Add(fullRoomInfo.loadout[i].position);
        }
    }
}
public class MapController : MonoBehaviour
{
    public NearHelper nearHelper;
    public MainMenuController mainMenuController;
    public BattleController battleController;
    public BattleRosterController battleRosterController;
    public RankAnimator rankAnimator;
    public Canvas battleCanvas;
    public LogController logController;
    public TextAsset mapFile;
    public Transform mapTransform;
    public GameObject startWindow;
    public GameObject victoryWindow;
    public GameObject defeatWindow;
    public GameObject logWindow;
    public GameObject shadowObj;
    public GameObject healthbarPrefab;
    public GameObject staticbarPrefab;
    public GameObject timeWindow;
    public GameObject[] mapObjs;
    public GameObject[] backgroundObjs;
    public GameObject[] topPrefabs;
    public GameObject arrowObj;
    public CustomButton[] timeButtons;
    public Image[] timeImages;
    public CustomText timeText;
    public SpriteRenderer arrowSprite;
    public LayerMask terrainLayer;
    public LayerMask propLayer;
    public LayerMask unitLayer;
    public UnitType placeUnit;
    [HideInInspector]
    public bool placingUnits;
    private IsoTile arrowIso;
    public bool placingPurple;
    private bool ownsMap;
    public bool gameLocked = true;
    private int mapIndex;
    private int supportAmount;
    public static NodeInfo[,] nodeGrid;
    private readonly HashSet<SpriteRenderer> hiddenProps = new HashSet<SpriteRenderer>();
    public static readonly List<UnitController> yellowUnits = new List<UnitController>();
    public static readonly List<UnitController> purpleUnits = new List<UnitController>();
    private int raycastUnit = -1;
    private float previousTime;
    public MapInfo mapInfo;
    public void Setup(MapInfo mapInfo)
    {
        this.mapInfo = mapInfo;
        BaseUtils.onTavern = false;
        battleCanvas.gameObject.SetActive(true);
        battleCanvas.enabled = true;
        LoadMap(mapInfo.mapType);
        ClearUnits();
        ownsMap = mapInfo.mapOwner == Database.databaseStruct.playerAccount;
        for (int i = 0; i < mapInfo.unitInfos.Count; i++)
        {
            bool ownsUnit = Database.OwnsUnit(mapInfo.unitInfos[i].unitID);
            if (BaseUtils.offlineMode)
            {
                ownsUnit = mapInfo.unitPositions[i].x < 15;
            }
            bool isUnitPurple = true;
            if ((ownsMap && ownsUnit) || (!ownsMap && !ownsUnit))
            {
                isUnitPurple = false;
            }
            else if ((ownsMap && !ownsUnit) || (!ownsMap && ownsUnit))
            {
                isUnitPurple = true;
            }
            PlaceUnit(nodeGrid[mapInfo.unitPositions[i].x, mapInfo.unitPositions[i].y], mapInfo.unitInfos[i], isUnitPurple);
        }
        if (mapInfo.hasFightStruct)
        {
            OnReceiveFightStruct(
                mapInfo.fightStruct,
                mapInfo.winner_id == Database.databaseStruct.playerAccount,
                mapInfo.notify_type == RoomNotificationType.FightFinish,
                mapInfo.notify_type == RoomNotificationType.RoundFinish,
                true);
        }
        else
        {
            supportAmount = 0;
            placingPurple = !ownsMap;
            placingUnits = true;
            gameLocked = false;
            AnimateShowSide();
            battleRosterController.Setup(placingPurple);
        }
    }
    private void AnimateShowSide()
    {
        foreach (IsoTile isoTile in mapObjs[mapIndex].GetComponentsInChildren<IsoTile>())
        {
            if (isoTile.transform.position.x < 0 && !placingPurple)
            {
                StartCoroutine(BlinkCube(isoTile.GetComponentInChildren<SpriteRenderer>(), new Color(.5f, .5f, 1)));
            }
            else if (isoTile.transform.position.x > 0 && placingPurple)
            {
                StartCoroutine(BlinkCube(isoTile.GetComponentInChildren<SpriteRenderer>(), new Color(.75f, .25f, 1)));
            }
        }
    }
    private IEnumerator BlinkCube(SpriteRenderer sprite, Color goColor)
    {
        for (int i = 0; i < 3; i++)
        {
            float timer = 0;
            while (timer <= 1)
            {
                sprite.color = Color.Lerp(Color.white, goColor, timer.Evaluate(CurveType.PeakParabol));
                timer += Time.deltaTime * 5;
                yield return null;
            }
            yield return new WaitForSeconds(.1f);
        }
    }
    public void OnFinishClick()
    {
        if (BaseUtils.offlineMode)
        {
            //StartCoroutine(FakeReceiveFightConfirmation());
        }
        else
        {
            battleRosterController.OnFinishPlacement();
            logController.Setup();
            arrowObj.SetActive(false);
            foreach (SpriteRenderer sprite in hiddenProps)
            {
                sprite.color = Color.white;
            }
            hiddenProps.Clear();
            gameLocked = true;
            List<PartialLoadoutData> playerLoadout = new List<PartialLoadoutData>();
            if (placingPurple)
            {
                for (int i = 0; i < purpleUnits.Count; i++)
                {
                    playerLoadout.Add(new PartialLoadoutData()
                    {
                        position = purpleUnits[i].currentNode.position,
                        token_id = purpleUnits[i].unitID.ToString(),
                    });
                }
                StartCoroutine(nearHelper.SimulateFight(playerLoadout.ToArray(), mapInfo.mapOwner));
            }
            else
            {
                for (int i = 0; i < yellowUnits.Count; i++)
                {
                    playerLoadout.Add(new PartialLoadoutData()
                    {
                        position = yellowUnits[i].currentNode.position,
                        token_id = yellowUnits[i].unitID.ToString(),
                    });
                }
                if (!mapInfo.creatingRoom)
                {
                    StartCoroutine(nearHelper.SimulateFight(playerLoadout.ToArray(), mapInfo.mapOwner));
                }
                else
                {
                    StartCoroutine(nearHelper.EndCreateRoom(playerLoadout.ToArray()));
                }
            }
        }
    }
    public void OnReceiveFightStruct(FightStruct fightStruct, bool victory, bool finalFight, bool willPlaceUnits, bool fromNotification)
    {
        StartCoroutine(FadeIntro(fightStruct, victory, finalFight, willPlaceUnits, fromNotification));
    }
    public void OnSuccessCreateRoom()
    {
        BaseUtils.ShowWarningMessage("Success!", new string[3] { "you have successfully created a challenge.", "now you need to wait for another player to accept it.", "once accepted, you will receive a notification in the game." }, ExitMapController, true);
    }
    public void ExitMapController()
    {
        StartCoroutine(WaitAndDispose());
    }
    private IEnumerator WaitAndDispose()
    {
        mainMenuController.FadeScreenIn();
        yield return new WaitForSeconds(1 / 3f);
        for (int i = 0; i < mapObjs.Length; i++)
        {
            mapObjs[i].SetActive(false);
            backgroundObjs[i].SetActive(false);
        }
        ClearUnits();
        BaseUtils.onTavern = true;
        battleCanvas.gameObject.SetActive(false);
        battleCanvas.enabled = false;
        nearHelper.dataGetState = DataGetState.AfterFight;
        StartCoroutine(nearHelper.GetPlayerData());
        //mainMenuController.Setup();
    }
    public void ClearUnits()
    {
        for (int i = 0; i < yellowUnits.Count; i++)
        {
            Destroy(yellowUnits[i].gameObject);
        }
        for (int i = 0; i < purpleUnits.Count; i++)
        {
            Destroy(purpleUnits[i].gameObject);
        }
        yellowUnits.Clear();
        purpleUnits.Clear();
    }

    public void LoadMap(MapType mapType)
    {
        mapIndex = (int)mapType;
        arrowIso = arrowObj.GetComponent<IsoTile>();
        mapObjs[mapIndex].SetActive(true);
        backgroundObjs[mapIndex].SetActive(true);
        nodeGrid = new NodeInfo[29, 13];
        List<MapData> mapDatas = XMLGenerator.DeserializeObject(mapFile.text, typeof(List<MapData>)) as List<MapData>;
        for (int i = 0; i < mapDatas[mapIndex].tileInfos.Count; i++)
        {
            Vector2Int rawPos = mapDatas[mapIndex].tileInfos[i].position;
            NodeInfo nodeInfo = new NodeInfo
            {
                position = new Vector2Int(rawPos.x + 14, rawPos.y + 7),
                blocked = mapDatas[mapIndex].tileInfos[i].blocked,
                tileType = mapDatas[mapIndex].tileInfos[i].tileType
            };
            nodeGrid[rawPos.x + 14, rawPos.y + 7] = nodeInfo;
            Vector3 worldPos = new Vector3(rawPos.x, 0, rawPos.y);
            if (Physics.Raycast(worldPos + Vector3.up * 5, Vector3.down * 10, out RaycastHit raycastHit, 100f, terrainLayer))
            {
                float heightBias = 0;
                if (nodeInfo.tileType == TileType.Water)
                {
                    heightBias = BaseUtils.tileDict[TileType.Water].offset;
                }
                else if (nodeInfo.tileType == TileType.Lava)
                {
                    heightBias = BaseUtils.tileDict[TileType.Lava].offset;
                }
                nodeInfo.offset = heightBias;
                nodeInfo.worldPos = worldPos + Vector3.up * (raycastHit.transform.position.y - heightBias + 1);
            }
        }
        //placingUnits = true;
        //gameLocked = false;
    }

    public void StopTimeClick()
    {
        if (Time.timeScale != 0)
        {
            previousTime = Time.timeScale;
        }
        Time.timeScale = Time.timeScale == 0 ? previousTime : 0;
    }
    public void OnTimeButtonClick(bool advancing)
    {
        if (gameLocked)
        {
            return;
        }
        if (advancing)
        {
            Time.timeScale++;
        }
        else
        {
            Time.timeScale = 1;
        }
        Database.databaseStruct.lockSpeed = (int)Time.timeScale;
        Database.SaveDatabase();
        SetTimerProperties();
    }
    public void SetTimerProperties()
    {
        if (Time.timeScale >= 10)
        {
            Time.timeScale = 10;
            timeImages[1].color = Color.gray;
            timeButtons[1].SetDeactivated(true);
        }
        else
        {
            timeImages[1].color = Color.white;
            timeButtons[1].SetDeactivated(false);
        }
        if (Time.timeScale <= 1)
        {
            timeImages[0].color = Color.gray;
            timeButtons[0].SetDeactivated(true);
        }
        else
        {
            timeImages[0].color = Color.white;
            timeButtons[0].SetDeactivated(false);
        }
        timeText.SetString($"time speed: {(int)Time.timeScale}x");
    }
    private IEnumerator FadeIntro(FightStruct fightStruct, bool victory, bool finalFight, bool willPlaceUnits, bool fromNotification)
    {
        StartCoroutine(ShowWindow(startWindow, new Vector2(200, 50), new Vector2(400, 5)));
        yield return new WaitForSeconds(2);
        StartCoroutine(HideWindow(startWindow));
        yield return new WaitForSeconds(1);
        gameLocked = false;
        placingUnits = false;
        timeWindow.SetActive(true);
        Time.timeScale = Database.databaseStruct.lockSpeed == -1 ? 1 : Database.databaseStruct.lockSpeed;
        SetTimerProperties();
        StartCoroutine(SimulateFight(fightStruct, victory, finalFight, willPlaceUnits, fromNotification));
    }
    private IEnumerator SimulateFight(FightStruct fightStruct, bool victory, bool finalFight, bool willPlaceUnits, bool fromNotification)
    {
        //bool wasOnAction = false;
        for (int i = 0; i < fightStruct.actionStructs.Count; i++)
        {
            UnitController unitController = GetUnitController(fightStruct.actionStructs[i].unitID);
            if (unitController == null)
            {
                continue;
            }
            //SetUnitHealth(fightStruct.actionStructs[i]);
            unitController.ProcessBuffDelays();
            if (fightStruct.actionStructs[i].moveToNode != null)
            {
                MoveUnit(fightStruct.actionStructs[i]);
            }
            else if (fightStruct.actionStructs[i].targetNode != null)
            {
                ActionUnit(fightStruct.actionStructs[i]);
                for (int k = 0; k < fightStruct.actionStructs[i].targets.Count; k++)
                {
                    AffectUnit(fightStruct.actionStructs[i].targets[k], fightStruct.actionStructs[i].damage, IsUnitHealer(fightStruct.actionStructs[i].unitID));
                }
            }
            else
            {
                IdleUnit(fightStruct.actionStructs[i]);
            }
            while (unitController.onAction)
            {
                //wasOnAction = true;
                yield return null;
            }
            /*if (!wasOnAction)
            {
                yield return new WaitForSeconds(.05f);
            }*/
        }
        EndFight(victory, finalFight, willPlaceUnits, fromNotification);
    }
    private UnitController GetUnitController(int unitID)
    {
        for (int i = 0; i < purpleUnits.Count; i++)
        {
            if (unitID == purpleUnits[i].unitID)
            {
                return purpleUnits[i];
            }
        }
        for (int i = 0; i < yellowUnits.Count; i++)
        {
            if (unitID == yellowUnits[i].unitID)
            {
                return yellowUnits[i];
            }
        }
        return null;
    }
    private bool IsUnitHealer(int unitID)
    {
        for (int i = 0; i < purpleUnits.Count; i++)
        {
            if (unitID == purpleUnits[i].unitID)
            {
                return purpleUnits[i].scriptableUnit.supports;
            }
        }
        for (int i = 0; i < yellowUnits.Count; i++)
        {
            if (unitID == yellowUnits[i].unitID)
            {
                return yellowUnits[i].scriptableUnit.supports;
            }
        }
        return false;
    }
    private void IdleUnit(ActionStruct actionStruct)
    {
        for (int i = 0; i < purpleUnits.Count; i++)
        {
            if (actionStruct.unitID == purpleUnits[i].unitID)
            {
                purpleUnits[i].Idle();
                return;
            }
        }
        for (int i = 0; i < yellowUnits.Count; i++)
        {
            if (actionStruct.unitID == yellowUnits[i].unitID)
            {
                yellowUnits[i].Idle();
                return;
            }
        }
    }
    /*private void SetUnitHealth(ActionStruct actionStruct)
    {
        for (int i = 0; i < purpleUnits.Count; i++)
        {
            if (actionStruct.unitID == purpleUnits[i].unitID)
            {
                purpleUnits[i].SetUnitHealth(actionStruct.currentHealth);
                return;
            }
        }
        for (int i = 0; i < yellowUnits.Count; i++)
        {
            if (actionStruct.unitID == yellowUnits[i].unitID)
            {
                yellowUnits[i].SetUnitHealth(actionStruct.currentHealth);
                return;
            }
        }
    }*/
    private void AffectUnit(int targetID, int damage, bool healing)
    {
        for (int i = 0; i < purpleUnits.Count; i++)
        {
            if (targetID == purpleUnits[i].unitID)
            {
                if (healing)
                {
                    purpleUnits[i].Heal(damage);
                }
                else
                {
                    purpleUnits[i].Damage(damage);
                }
                return;
            }
        }
        for (int i = 0; i < yellowUnits.Count; i++)
        {
            if (targetID == yellowUnits[i].unitID)
            {
                if (healing)
                {
                    yellowUnits[i].Heal(damage);
                }
                else
                {
                    yellowUnits[i].Damage(damage);
                }
                return;
            }
        }
        Debug.Log("not found" + targetID);
    }
    private void MoveUnit(ActionStruct actionStruct)
    {
        for (int i = 0; i < purpleUnits.Count; i++)
        {
            if (actionStruct.unitID == purpleUnits[i].unitID)
            {
                purpleUnits[i].Move(new Vector2Int(actionStruct.moveToNode.x, actionStruct.moveToNode.y));
                return;
            }
        }
        for (int i = 0; i < yellowUnits.Count; i++)
        {
            if (actionStruct.unitID == yellowUnits[i].unitID)
            {
                yellowUnits[i].Move(new Vector2Int(actionStruct.moveToNode.x, actionStruct.moveToNode.y));
                return;
            }
        }
    }
    private void ActionUnit(ActionStruct actionStruct)
    {
        for (int i = 0; i < purpleUnits.Count; i++)
        {
            if (actionStruct.unitID == purpleUnits[i].unitID)
            {
                purpleUnits[i].Action(new Vector2Int(actionStruct.targetNode.x, actionStruct.targetNode.y), GetRevivedUnit(purpleUnits[i], actionStruct));
                return;
            }
        }
        for (int i = 0; i < yellowUnits.Count; i++)
        {
            if (actionStruct.unitID == yellowUnits[i].unitID)
            {
                yellowUnits[i].Action(new Vector2Int(actionStruct.targetNode.x, actionStruct.targetNode.y), GetRevivedUnit(yellowUnits[i], actionStruct));
                return;
            }
        }
    }
    private UnitController GetRevivedUnit(UnitController unitController, ActionStruct actionStruct)
    {
        if (unitController.unitInfo.unitType != UnitType.Priest && unitController.unitInfo.unitType != UnitType.Necromancer && unitController.unitInfo.unitType != UnitType.Squire)
        {
            return null;
        }
        if (actionStruct.targets.Count == 0)
        {
            return null;
        }
        for (int i = 0; i < purpleUnits.Count; i++)
        {
            if (actionStruct.targets[0] == purpleUnits[i].unitID)
            {
                return purpleUnits[i];
            }
        }
        for (int i = 0; i < yellowUnits.Count; i++)
        {
            if (actionStruct.targets[0] == yellowUnits[i].unitID)
            {
                return yellowUnits[i];
            }
        }
        return null;
    }
    private void EndFight(bool victory, bool finalFight, bool willPlaceUnits, bool fromNotification)
    {
        BaseUtils.StopAllTexts();
        Time.timeScale = 1;
        gameLocked = true;
        timeWindow.SetActive(false);
        for (int i = 0; i < yellowUnits.Count; i++)
        {
            if (yellowUnits[i].currentHealth > 0)
            {
                yellowUnits[i].spriteAnimator.Animate("Idle");
                if (yellowUnits[i].healthbarController != null && yellowUnits[i].healthbarController.gameObject != null)
                {
                    Destroy(yellowUnits[i].healthbarController.gameObject);
                }
            }
        }
        for (int i = 0; i < purpleUnits.Count; i++)
        {
            if (purpleUnits[i].currentHealth > 0)
            {
                purpleUnits[i].spriteAnimator.Animate("Idle");
                if (purpleUnits[i].healthbarController != null && purpleUnits[i].healthbarController.gameObject != null)
                {
                    Destroy(purpleUnits[i].healthbarController.gameObject);
                }
            }
        }
        if (!victory)
        {
            StartCoroutine(ShowWindow(defeatWindow, new Vector2(100, 50), new Vector2(230, 5), victory, finalFight, willPlaceUnits, fromNotification, true));
        }
        else
        {
            StartCoroutine(ShowWindow(victoryWindow, new Vector2(100, 50), new Vector2(230, 5), victory, finalFight, willPlaceUnits, fromNotification, true));
        }
    }
    private IEnumerator ShowWindow(GameObject windowObj, Vector2 fromSize, Vector2 gotoSize, bool victory = false, bool finalFight = false, bool willPlaceUnits = false, bool fromNotification = false, bool endWindow = false)
    {
        windowObj.SetActive(true);
        RectTransform childRect = windowObj.transform.GetChild(0) as RectTransform;
        RectTransform childRect2 = windowObj.transform.GetChild(1) as RectTransform;
        Image image = windowObj.GetComponent<Image>();
        Color initColor = image.color;
        initColor.a = .3f;
        Color initTColor = initColor;
        initTColor.a = 0;
        Image childImage = childRect.GetComponent<Image>();
        Image childImage2 = childRect2.GetComponent<Image>();
        float timer = 0;
        while (timer <= 1)
        {
            childRect2.transform.localScale = Vector3.Lerp(new Vector3(.85f, .7f, 1) * 1.5f, Vector3.one * 1.5f, timer.Evaluate(CurveType.EaseOut));
            childRect.sizeDelta = Vector2.Lerp(fromSize, gotoSize, timer.Evaluate(CurveType.EaseOut));
            image.color = Color.Lerp(initTColor, initColor, timer.Evaluate(CurveType.EaseOut));
            childImage.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, .75f), timer.Evaluate(CurveType.EaseOut));
            childImage2.color = Color.Lerp(new Color(1, 1, 1, 0), Color.white, timer.Evaluate(CurveType.EaseOut));
            timer += Time.deltaTime * .5f;
            yield return null;
        }
        image.color = initColor;
        childRect2.transform.localScale = Vector3.one * 1.5f;
        childRect.sizeDelta = gotoSize;
        childImage.color = new Color(1, 1, 1, .75f);
        childImage2.color = Color.white;
        if (endWindow)
        {
            yield return new WaitForSeconds(1);
            StartCoroutine(HideWindow(windowObj));
            if (finalFight)
            {
                yield return new WaitForSeconds(1);
                int betAmount = 5;
                int gainAmount = 9;
                switch ((RoomTierTypes)mapInfo.betType)
                {
                    case RoomTierTypes.Tier1:
                        betAmount = 5;
                        gainAmount = 9;
                        break;
                    case RoomTierTypes.Tier2:
                        betAmount = 50;
                        gainAmount = 95;
                        break;
                    case RoomTierTypes.Tier3:
                        betAmount = 500;
                        gainAmount = 950;
                        break;
                }
                if (fromNotification)
                {
                    //has already updated money/rank
                    if (victory)
                    {
                        //#PXTBET
                        //rankAnimator.Setup(Database.databaseStruct.currentRank - 10, Database.databaseStruct.currentRank, Database.databaseStruct.pixelTokens - gainAmount, Database.databaseStruct.pixelTokens, 10, gainAmount);
                        rankAnimator.Setup(Database.databaseStruct.currentRank - 10, Database.databaseStruct.currentRank, Database.databaseStruct.pixelTokens, Database.databaseStruct.pixelTokens, 10, 0);
                    }
                    else
                    {
                        //#PXTBET
                        //rankAnimator.Setup(Database.databaseStruct.currentRank + 10, Database.databaseStruct.currentRank, Database.databaseStruct.pixelTokens + betAmount, Database.databaseStruct.pixelTokens, -10, -betAmount);
                        rankAnimator.Setup(Database.databaseStruct.currentRank + 10, Database.databaseStruct.currentRank, Database.databaseStruct.pixelTokens, Database.databaseStruct.pixelTokens, -10, 0);
                    }
                }
                else
                {
                    if (victory)
                    {
                        //#PXTBET
                        //rankAnimator.Setup(Database.databaseStruct.currentRank, Database.databaseStruct.currentRank + 10, Database.databaseStruct.pixelTokens, Database.databaseStruct.pixelTokens + gainAmount, 10, gainAmount);
                        rankAnimator.Setup(Database.databaseStruct.currentRank, Database.databaseStruct.currentRank + 10, Database.databaseStruct.pixelTokens, Database.databaseStruct.pixelTokens, 10, 0);
                    }
                    else
                    {
                        //#PXTBET
                        //rankAnimator.Setup(Database.databaseStruct.currentRank, Database.databaseStruct.currentRank - 10, Database.databaseStruct.pixelTokens, Database.databaseStruct.pixelTokens - betAmount, -10, -betAmount);
                        rankAnimator.Setup(Database.databaseStruct.currentRank, Database.databaseStruct.currentRank - 10, Database.databaseStruct.pixelTokens, Database.databaseStruct.pixelTokens, -10, 0);
                    }
                }
                yield return new WaitForSeconds(4);
                rankAnimator.gameObject.SetActive(false);
                ExitMapController();
            }
            else if (willPlaceUnits)
            {
                yield return new WaitForSeconds(1);
                ClearUnits();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                supportAmount = 0;
                placingPurple = !ownsMap;
                placingUnits = true;
                gameLocked = false;
                AnimateShowSide();
                for (int i = 0; i < mapInfo.unitInfos.Count; i++)
                {
                    bool ownsUnit = Database.OwnsUnit(mapInfo.unitInfos[i].unitID);
                    if (ownsUnit)
                    {
                        continue;
                    }
                    bool isUnitPurple = true;
                    if ((ownsMap && ownsUnit) || (!ownsMap && !ownsUnit))
                    {
                        isUnitPurple = false;
                    }
                    else if ((ownsMap && !ownsUnit) || (!ownsMap && ownsUnit))
                    {
                        isUnitPurple = true;
                    }
                    PlaceUnit(nodeGrid[mapInfo.unitPositions[i].x, mapInfo.unitPositions[i].y], mapInfo.unitInfos[i], isUnitPurple);
                }
                battleRosterController.Setup(placingPurple);
                mainMenuController.ShowWarning("round over", "please replace your units");
            }
            else
            {
                yield return new WaitForSeconds(1);
                BaseUtils.ShowWarningMessage("round over", new string[2] { "now you need to wait for the opponent to make his move", "you will get a notification then." }, ExitMapController, true);
                //ExitMapController();
            }
        }
    }
    private IEnumerator HideWindow(GameObject windowObj)
    {
        RectTransform childRect = windowObj.transform.GetChild(0) as RectTransform;
        RectTransform childRect2 = windowObj.transform.GetChild(1) as RectTransform;
        Image image = windowObj.GetComponent<Image>();
        Color initColor = image.color;
        Color initTColor = initColor;
        initTColor.a = 0;
        Image childImage = childRect.GetComponent<Image>();
        Image childImage2 = childRect2.GetComponent<Image>();
        float timer = 0;
        while (timer <= 1)
        {
            image.color = Color.Lerp(initColor, initTColor, timer.Evaluate(CurveType.EaseIn));
            childImage.color = Color.Lerp(new Color(1, 1, 1, .75f), new Color(1, 1, 1, 0), timer.Evaluate(CurveType.EaseIn));
            childImage2.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), timer.Evaluate(CurveType.EaseIn));
            timer += Time.deltaTime;
            yield return null;
        }
        windowObj.SetActive(false);
    }
    private void Update()
    {
        if (gameLocked)
        {
            return;
        }
        if (BaseUtils.onTavern && (mainMenuController.GetCanvasCasts(out _) || BaseUtils.showingWarn))
        {
            arrowObj.SetActive(false);
            return;
        }
        if (!BaseUtils.onTavern)
        {
            RaycastProps();
        }
        /*if (!placingUnits)
        {
            if (HasUnits(purpleUnits) && HasUnits(yellowUnits))
            {
                for (int i = 0; i < yellowUnits.Count; i++)
                {
                    yellowUnits[i].Tick();
                }
                for (int i = 0; i < purpleUnits.Count; i++)
                {
                    purpleUnits[i].Tick();
                }
            }
            else
            {
                EndFight();
                return;
            }
        }*/
        RaycastUnits();
        if (Physics.Raycast(BaseUtils.thisCam.ScreenPointToRay(Input.mousePosition), out RaycastHit tileInfo, 100f, terrainLayer))
        {
            arrowObj.transform.position = tileInfo.transform.position;
            //arrowIso.Update_SortingOrder();
            arrowSprite.sortingOrder = arrowIso.sortingOrder.CalcSortingOrder();
            int posX = Mathf.RoundToInt(tileInfo.transform.position.x) + 14;
            int posZ = Mathf.RoundToInt(tileInfo.transform.position.z) + 7;
            CheckForHoverUnitTooltip(posX, posZ);
            if (placingUnits)
            {
                UnitInfo unitInfo = battleRosterController.GetSelectUnit();
                bool blocked = nodeGrid[posX, posZ].blocked || (nodeGrid[posX, posZ].tileType == TileType.Lava && unitInfo.unitType != UnitType.Warlock);
                bool shiftPress = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                if (!BaseUtils.onTavern)
                {
                    if (nodeGrid[posX, posZ].unit != null)
                    {
                        blocked = true;
                    }
                    else if (shiftPress)
                    {
                        blocked = true;
                    }
                    else if (!placingPurple && posX >= 14)
                    {
                        blocked = true;
                    }
                    else if (placingPurple && posX <= 14)
                    {
                        blocked = true;
                    }
                    if (shiftPress && nodeGrid[posX, posZ].unit != null)
                    {
                        arrowSprite.color = BaseUtils.slowColor;
                    }
                    else
                    {
                        arrowSprite.color = blocked ? BaseUtils.damageColor : Color.white;
                    }
                }
                else
                {
                    if (nodeGrid[posX, posZ].unit != null)
                    {
                        arrowSprite.color = BaseUtils.speedColor;
                    }
                    else
                    {
                        arrowSprite.color = blocked ? BaseUtils.damageColor : Color.white;
                    }
                }
                if (Input.GetMouseButtonDown(0) && !BaseUtils.onTavern)
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        RemoveUnit(posX, posZ);
                    }
                    else if (!blocked)
                    {
                        if (unitInfo.unitType != UnitType.None)
                        {
                            if (battleRosterController.placedUnits.Count < 6)
                            {
                                if (supportAmount > 2 && BaseUtils.unitDict[unitInfo.unitType].supports)
                                {
                                    mainMenuController.ShowWarning("you can only have a maximum of 3 supports units", "hold shift and click to remove an unit from the map.");

                                }
                                else
                                {
                                    UnitController unitController = PlaceUnit(nodeGrid[posX, posZ], unitInfo, placingPurple);
                                    battleRosterController.OnPlaceUnit(unitController);
                                    if (BaseUtils.unitDict[unitInfo.unitType].supports)
                                    {
                                        supportAmount++;
                                    }
                                }
                            }
                            else
                            {
                                mainMenuController.ShowWarning("cannot place more units!", "hold shift and click to remove an unit from the map.");
                            }
                        }
                        else
                        {
                            mainMenuController.ShowWarning("select an unit from the roster below", "before clicking on the map to place units");
                        }
                    }
                    else if (nodeGrid[posX, posZ].unit == null)
                    {
                        mainMenuController.ShowWarning("you cannot place a unit here", "place on a white marked square instead.");
                    }
                }
            }
            else
            {
                arrowSprite.color = Color.white;
            }
            if (Input.GetMouseButton(0))
            {
                arrowObj.SetActive(false);
            }
            else if (!arrowObj.activeSelf)
            {
                arrowObj.SetActive(true);
            }
        }
        else
        {
            if (raycastUnit != -1)
            {
                mainMenuController.HideTooltip();
                raycastUnit = -1;
            }
            arrowObj.SetActive(false);
        }
    }

    private void RaycastProps()
    {
        foreach (SpriteRenderer sprite in hiddenProps)
        {
            sprite.color = Color.white;
        }
        hiddenProps.Clear();
        RaycastHit[] propInfos = Physics.RaycastAll(BaseUtils.thisCam.ScreenPointToRay(Input.mousePosition), 100f, propLayer);
        for (int i = 0; i < propInfos.Length; i++)
        {
            if (propInfos[i].transform.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = new Color(1, 1, 1, .25f);
                hiddenProps.Add(spriteRenderer);
            }
        }
    }

    private void RemoveUnit(int posX, int posZ)
    {
        if (nodeGrid[posX, posZ].unit != null)
        {
            if (nodeGrid[posX, posZ].unit.scriptableUnit.supports)
            {
                supportAmount--;
            }
            if (nodeGrid[posX, posZ].unit.isPurple)
            {
                purpleUnits.Remove(nodeGrid[posX, posZ].unit);
            }
            else
            {
                yellowUnits.Remove(nodeGrid[posX, posZ].unit);
            }
            battleRosterController.OnRemoveUnit(nodeGrid[posX, posZ].unit.unitID);
            Destroy(nodeGrid[posX, posZ].unit.gameObject);
            nodeGrid[posX, posZ].unit = null;
        }
    }
    /*private void CheckForHoverUnitTooltip()
    {
        if ((Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) || gameLocked)
        {
            if (Physics.Raycast(BaseUtils.thisCam.ScreenPointToRay(Input.mousePosition), out RaycastHit unitInfo, 100f, unitLayer))
            {
                if (unitInfo.transform.parent.parent.TryGetComponent(out UnitController unitController))
                {
                    if (raycastUnit != unitController.unitID)
                    {
                        mainMenuController.ShowTooltip(unitController.healthbarController.rectTransform, unitController.unitInfo, unitController.health, unitController.isPurple, true);
                        raycastUnit = unitController.unitID;
                    }
                }
                else
                {
                    mainMenuController.HideTooltip();
                    raycastUnit = -1;
                }
            }
            else
            {
                mainMenuController.HideTooltip();
                raycastUnit = -1;
            }
        }
    }*/
    private void CheckForHoverUnitTooltip(int posX, int posZ)
    {
        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            if (nodeGrid[posX, posZ].unit != null)
            {
                if (raycastUnit != nodeGrid[posX, posZ].unit.unitID)
                {
                    mainMenuController.ShowTooltip(nodeGrid[posX, posZ].unit.healthbarController.rectTransform, nodeGrid[posX, posZ].unit.unitInfo, nodeGrid[posX, posZ].unit.currentHealth, nodeGrid[posX, posZ].unit.isPurple, true);
                    raycastUnit = nodeGrid[posX, posZ].unit.unitID;
                }
            }
            else
            {
                mainMenuController.HideTooltip();
                raycastUnit = -1;
            }
        }
    }
    private void RaycastUnits()
    {
        RaycastHit[] propInfos;
        for (int i = 0; i < purpleUnits.Count; i++)
        {
            propInfos = Physics.RaycastAll(BaseUtils.thisCam.ScreenPointToRay(BaseUtils.thisCam.WorldToScreenPoint(purpleUnits[i].transform.position)), 100f, propLayer);
            for (int k = 0; k < propInfos.Length; k++)
            {
                if (propInfos[k].transform.TryGetComponent(out SpriteRenderer spriteRenderer))
                {
                    spriteRenderer.color = new Color(1, 1, 1, .25f);
                    hiddenProps.Add(spriteRenderer);
                }
            }
        }
        for (int i = 0; i < yellowUnits.Count; i++)
        {
            propInfos = Physics.RaycastAll(BaseUtils.thisCam.ScreenPointToRay(BaseUtils.thisCam.WorldToScreenPoint(yellowUnits[i].transform.position)), 100f, propLayer);
            for (int k = 0; k < propInfos.Length; k++)
            {
                if (propInfos[k].transform.TryGetComponent(out SpriteRenderer spriteRenderer))
                {
                    spriteRenderer.color = new Color(1, 1, 1, .25f);
                    hiddenProps.Add(spriteRenderer);
                }
            }
        }
    }
    public void RemoveUnit(UnitController unitController)
    {
        Destroy(unitController.gameObject);
        if (unitController.isPurple)
        {
            purpleUnits.Remove(unitController);
        }
        else
        {
            yellowUnits.Remove(unitController);
        }
        //unitController.currentNode.bodies.Remove(unitController);
    }
    public UnitController PlaceUnit(NodeInfo gotoNode, UnitInfo unitInfo, bool isPurple, int currentCooldown = 0)
    {
        GameObject unitObj = Instantiate(BaseUtils.unitDict[unitInfo.unitType].unitPrefab, mapObjs[mapIndex].transform);
        UnitController unitController = unitObj.GetComponent<UnitController>();
        unitController.Setup(this, gotoNode, unitInfo, isPurple, currentCooldown);
        if (isPurple)
        {
            purpleUnits.Add(unitController);
        }
        else
        {
            yellowUnits.Add(unitController);
        }
        return unitController;
    }
}
