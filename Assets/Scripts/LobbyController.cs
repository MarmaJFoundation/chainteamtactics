using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;

public class LobbyController : MonoBehaviour, IEnhancedScrollerDelegate
{
    private SmallList<LobbyData> elementData = new SmallList<LobbyData>();
    public NearHelper nearHelper;
    public MainMenuController mainMenuController;
    public MapController mapController;
    public EnhancedScroller scroller;
    public EnhancedScrollerCellView cellViewPrefab;
    public GameObject lobbyWindow;
    public GameObject challengeWindow;
    public int numberOfCellsPerRow = 3;
    public GameObject[] sortButtons;
    public CustomInput sortInput;
    public CustomInput challengeInput;
    private string sortString;
    private bool sortingUpwards;
    private bool hasRoom;
    private string challengerName;
    private string challengeString;
    public void Setup()
    {
        hasRoom = false;
        lobbyWindow.SetActive(true);
        scroller.Delegate = this;
        elementData.Clear();
        sortInput.Setup(false);
        sortString = "rank";
        sortInput.inputText.SetString("rank");
        sortingUpwards = true;
        sortButtons[0].SetActive(sortingUpwards);
        sortButtons[1].SetActive(!sortingUpwards);
        if (BaseUtils.offlineMode)
        {
            for (int i = 0; i < 100; i++)
            {
                elementData.Add(new LobbyData()
                {
                    loadout = new List<UnitInfo>()
                    { BaseUtils.GenerateRandomUnit(),
                    BaseUtils.GenerateRandomUnit(),
                    BaseUtils.GenerateRandomUnit(),
                    BaseUtils.GenerateRandomUnit(),
                    BaseUtils.GenerateRandomUnit(),
                    BaseUtils.GenerateRandomUnit(),},
                    lost = BaseUtils.RandomInt(0, 100),
                    won = BaseUtils.RandomInt(0, 100),
                    rank = BaseUtils.RandomInt(800, 3000),
                    playername = "OfflinePlayer" + i,
                    selfData = false
                });
            }
            scroller.ReloadData();
        }
        else
        {
            StartCoroutine(nearHelper.RequestLobbyData());
        }
    }
    public void OnReceiveLobbyRoomData(List<LobbyRoomInfo> lobbyRoomInfos)
    {
        for (int i = 0; i < lobbyRoomInfos.Count; i++)
        {
            List<UnitInfo> convertedLoadout = new List<UnitInfo>();
            for (int k = 0; k < lobbyRoomInfos[i].loadout.Length; k++)
            {
                int.TryParse(lobbyRoomInfos[i].loadout[k].token_id, out int unitID);
                convertedLoadout.Add(new UnitInfo((UnitType)lobbyRoomInfos[i].loadout[k].unit_type, BaseUtils.GetUnitColorInfo(unitID), unitID, 0, 0, 0, 0));
            }
            elementData.Add(new LobbyData()
            {
                loadout = convertedLoadout,
                lost = lobbyRoomInfos[i].lost,
                won = lobbyRoomInfos[i].won,
                playername = lobbyRoomInfos[i].account_id,
                rank = lobbyRoomInfos[i].rank,
                selfData = lobbyRoomInfos[i].account_id == Database.databaseStruct.playerAccount,
                betType = lobbyRoomInfos[i].betType
            });
            if (lobbyRoomInfos[i].account_id == Database.databaseStruct.playerAccount)
            {
                hasRoom = true;
            }
        }
        SortData();
    }

    private void SortData()
    {
        List<LobbyData> listToSort = new List<LobbyData>();
        LobbyData selfData = null;
        for (int i = 0; i < elementData.Count; i++)
        {
            if (elementData[i].selfData)
            {
                selfData = elementData[i];
                continue;
            }
            listToSort.Add(elementData[i]);
        }
        if (sortString.Length > 0)
        {
            if ("rank".Contains(sortString) || sortString.Contains("rank"))
            {
                if (sortingUpwards)
                {
                    listToSort.Sort((x, y) => x.rank.CompareTo(y.rank));
                }
                else
                {
                    listToSort.Sort((x, y) => y.rank.CompareTo(x.rank));
                }
            }
            else if ("unit".Contains(sortString) || sortString.Contains("unit"))
            {
                if (sortingUpwards)
                {
                    listToSort.Sort((x, y) => x.Units.CompareTo(y.Units));
                }
                else
                {
                    listToSort.Sort((x, y) => y.Units.CompareTo(x.Units));
                }
            }
            else if ("pxt".Contains(sortString) || sortString.Contains("pxt") || "bet".Contains(sortString) || sortString.Contains("bet"))
            {
                if (sortingUpwards)
                {
                    listToSort.Sort((x, y) => x.betType.CompareTo(y.betType));
                }
                else
                {
                    listToSort.Sort((x, y) => y.betType.CompareTo(x.betType));
                }
            }
        }
        elementData.Clear();
        for (int i = 0; i < listToSort.Count; i++)
        {
            elementData.Add(listToSort[i]);
        }
        if (selfData != null)
        {
            elementData.Insert(selfData, 0);
        }
        scroller.ReloadData();
    }

    public void Dispose()
    {
        lobbyWindow.SetActive(false);
    }
    public void OnTypeSort(string inputText)
    {
        if (sortString == inputText)
        {
            return;
        }
        sortString = inputText;
        SortData();
    }
    public void OnSortUpwardsClick()
    {
        sortingUpwards = !sortingUpwards;
        sortButtons[0].SetActive(sortingUpwards);
        sortButtons[1].SetActive(!sortingUpwards);
        SortData();
    }
    public void OnFightClick(LobbyData lobbyData)
    {
        challengerName = lobbyData.playername;
        int betAmount = 5;
        int gainAmount = 9;
        switch ((RoomTierTypes)lobbyData.betType)
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
        OnAcceptFight();
        //#PXTBET
        //BaseUtils.ShowWarningMessage("Wait", new string[4] { $"To accept this fight, you need to make a {betAmount} pxt deposit", "on defeat, you will lose this money.", $"on victory, you will earn it back {gainAmount} pxt", "do you wish to continue?" }, OnAcceptFight);
    }
    private void OnAcceptFight()
    {
        StartCoroutine(nearHelper.RequestJoinRoom(challengerName));
    }
    public void OnJoinRoom(FullRoomInfo fullRoomInfo, string ownerName)
    {
        if (ownerName == "")
        {
            ownerName = challengerName;
        }
        mainMenuController.OnReceiveMapInfo(new MapInfo(ownerName, fullRoomInfo));
    }
    public void OnNewChallengeClick()
    {
        if (!Database.HasSixUnits())
        {
            BaseUtils.ShowWarningMessage("Not possible", new string[2] { "you need atleast 6 units in order to play", "please hire new or buy some units." });
            return;
        }
        if (!Database.HasNonSupportUnits())
        {
            BaseUtils.ShowWarningMessage("Not possible", new string[2] { "you need atleast 3 units on your team to be non-support", "please hire new or trade some of your units." });
            return;
        }
        if (hasRoom)
        {
            BaseUtils.ShowWarningMessage("Not possible", new string[2] { "you already have a room created", "accept another challenge or wait for yours to be accepted." });
            return;
        }
        //#PXTBET
        //challengeWindow.SetActive(true);
        //challengeInput.Setup(true);
        StartCoroutine(nearHelper.RequestCreateRoom(0));
    }
    public void ChallengeExitClick()
    {
        challengeWindow.SetActive(false);
    }
    public void OnChallengeType(string input)
    {
        if (challengeString == input)
        {
            return;
        }
        challengeString = input;
    }
    public void OnAcceptChallenge()
    {
        challengeWindow.SetActive(false);
        if (challengeString == "")
        {
            BaseUtils.ShowWarningMessage("Not possible", new string[1] { "please select an amount of pxt to bet" });
            return;
        }
        int challengeIndex = -1;
        if (challengeString.Contains("500"))
        {
            challengeIndex = 2;
        }
        else if (challengeString.Contains("50"))
        {
            challengeIndex = 1;
        }
        else if (challengeString.Contains("5"))
        {
            challengeIndex = 0;
        }
        if (challengeIndex == -1)
        {
            BaseUtils.ShowWarningMessage("Not possible", new string[2] { "selected betting amount isnt available", "select a correct amount of pxt" });
            return;
        }
        StartCoroutine(nearHelper.RequestCreateRoom(challengeIndex));
    }
    #region EnhancedScroller Handlers
    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return Mathf.CeilToInt((float)elementData.Count / (float)numberOfCellsPerRow);
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 70f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        LobbyRow cellView = scroller.GetCellView(cellViewPrefab) as LobbyRow;
        var di = dataIndex * numberOfCellsPerRow;
        cellView.SetData(ref elementData, this, di);
        return cellView;
    }

    #endregion
}
public class LobbyData
{
    public string playername;
    public List<UnitInfo> loadout;
    public int won;
    public int lost;
    public int rank;
    public int betType;
    public bool selfData;

    public int Units
    {
        get
        {
            int unitRank = 0;
            for (int i = 0; i < loadout.Count; i++)
            {
                unitRank += BaseUtils.unitDict[loadout[i].unitType].unitTier;
            }
            return unitRank;
        }
    }
}
