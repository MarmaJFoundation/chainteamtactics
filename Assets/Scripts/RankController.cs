using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RankController : MonoBehaviour, IEnhancedScrollerDelegate
{
    private readonly SmallList<RankData> elementData = new SmallList<RankData>();
    public NearHelper nearHelper;
    public EnhancedScroller scroller;
    public EnhancedScrollerCellView cellViewPrefab;
    public GameObject rankWindow;
    public Sprite[] rankSprites;
    public void Setup()
    {
        rankWindow.SetActive(true);
        scroller.Delegate = this;
        if (BaseUtils.offlineMode)
        {
            /*List<RankData> fakeData = new List<RankData>();
            for (int i = 0; i < 100; i++)
            {
                fakeData.Add(new RankWrapper()
                {
                    loadout = new List<UnitInfo>()
                    { BaseUtils.GenerateRandomUnit(),
                    BaseUtils.GenerateRandomUnit(),
                    BaseUtils.GenerateRandomUnit(),
                    BaseUtils.GenerateRandomUnit(),
                    BaseUtils.GenerateRandomUnit(),
                    BaseUtils.GenerateRandomUnit(),},
                    playerName = "offlinePlayer" + i + ".near",
                    playerRank = BaseUtils.RandomInt(800, 4000),
                    position = i
                });
            }
            OnReceiveRankData(fakeData);*/
        }
        else
        {
            StartCoroutine(nearHelper.RequestRankData());
        }
    }

    public void Dispose()
    {
        rankWindow.SetActive(false);
    }
    public void OnReceiveRankData(List<RankWrapper> rankData)
    {
        if (rankData.Count == 0)
        {
            return;
        }
        elementData.Clear();
        bool hasSelfRank = rankData[0].account_id == Database.databaseStruct.playerAccount;
        for (int i = 0; i < rankData.Count; i++)
        {
            int listIndex;
            if (!hasSelfRank)
            {
                listIndex = i + 1;
            }
            else
            {
                listIndex = i == 0 ? rankData[i].position : i;
            }
            elementData.Add(new RankData()
            {
                loadout = ConvertLoadout(rankData[i].loadout),
                playerName = rankData[i].account_id,
                playerRank = rankData[i].rank,
                position = listIndex,
                selfRank = i == 0 && hasSelfRank
            });
        }
        scroller.ReloadData();
    }
    private List<UnitInfo> ConvertLoadout(List<LobbyLoadoutData> tokenData)
    {
        List<UnitInfo> loadout = new List<UnitInfo>();
        for (int i = 0; i < tokenData.Count; i++)
        {
            loadout.Add(new UnitInfo(tokenData[i]));
        }
        return loadout;
    }
    #region EnhancedScroller Callbacks
    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return elementData.Count;
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 24f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        RankCell cellView = scroller.GetCellView(cellViewPrefab) as RankCell;
        cellView.SetData(this, dataIndex, elementData[dataIndex]);
        return cellView;
    }
    #endregion
}
public class RankData
{
    public string playerName;
    public int playerRank;
    public List<UnitInfo> loadout;
    public int position;
    public bool selfRank;
}