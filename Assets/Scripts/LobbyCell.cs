using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;
public class LobbyCell : EnhancedScrollerCellView
{
    public int DataIndex { get; private set; }
    private LobbyController lobbyController;
    private LobbyData lobbyData;
    public Image rankEdgeImage;
    public Image rankSymbolImage;
    public Image rankLetterImage;
    public Image[] loadoutImages;
    public Image[] loadoutSlots;
    public CustomTooltip[] loadoutTooltips;
    public CustomText nameText;
    public CustomText rankText;
    public CustomText wonText;
    public CustomText lostText;
    public CustomText betText;
    public GameObject pivotObj;
    public GameObject fightButton;
    public GameObject yoursButton;
    public void SetData(int dataIndex, LobbyController lobbyController, LobbyData lobbyData)
    {
        DataIndex = dataIndex;
        if (lobbyData == null)
        {
            pivotObj.SetActive(false);
            return;
        }
        pivotObj.SetActive(true);
        this.lobbyController = lobbyController;
        this.lobbyData = lobbyData;
        if (lobbyData.playername == Database.databaseStruct.playerAccount)
        {
            fightButton.SetActive(false);
            yoursButton.SetActive(true);
        }
        else
        {
            fightButton.SetActive(true);
            yoursButton.SetActive(false);
        }
        int rankIndex = BaseUtils.RankToIndex(lobbyData.rank);
        rankSymbolImage.sprite = lobbyController.mainMenuController.rankSymbolSprites[Mathf.Clamp(Mathf.FloorToInt(rankIndex / 3), 0, 4)];
        rankLetterImage.sprite = lobbyController.mainMenuController.rankLetterSprites[rankIndex];
        rankEdgeImage.sprite = BaseUtils.rankEdges[Mathf.Clamp(Mathf.FloorToInt(rankIndex / 3), 0, 4)];
        nameText.SetString(lobbyData.playername);
        rankText.SetString($"rank: {lobbyData.rank}");
        wonText.SetString($"won: {lobbyData.won}");
        lostText.SetString($"lost: {lobbyData.lost}");
        //#PXTBET
        /*switch ((RoomTierTypes)lobbyData.betType)
        {
            case RoomTierTypes.Tier1:
                betText.SetString($"pxt bet: 5");
                break;
            case RoomTierTypes.Tier2:
                betText.SetString($"pxt bet: 50");
                break;
            case RoomTierTypes.Tier3:
                betText.SetString($"pxt bet: 500");
                break;
        }*/
        betText.SetString("free match");
        for (int i = 0; i < lobbyData.loadout.Count; i++)
        {
            ScriptableUnit scriptableUnit = BaseUtils.unitDict[lobbyData.loadout[i].unitType];
            loadoutTooltips[i].tooltipText[0] = scriptableUnit.unitType.ToString();
            loadoutSlots[i].sprite = BaseUtils.tierSlots[scriptableUnit.unitTier - 1];
            loadoutImages[i].sprite = scriptableUnit.defaultImage;
            loadoutImages[i].transform.localScale = !lobbyData.selfData ? new Vector3(-1, 1, 1) : Vector3.one;
            Material goMaterial = new Material(!lobbyData.selfData ? BaseUtils.purpleMaterialUI : BaseUtils.yellowMaterialUI);
            goMaterial.SetColor("_HairColor", lobbyData.loadout[i].unitColorInfo.hairColor);
            goMaterial.SetColor("_SkinColor", lobbyData.loadout[i].unitColorInfo.skinColor);
            goMaterial.SetColor("_RarityOneColor", lobbyData.loadout[i].unitColorInfo.rarityColors[0]);
            goMaterial.SetColor("_RarityTwoColor", lobbyData.loadout[i].unitColorInfo.rarityColors[1]);
            goMaterial.SetColor("_RarityThreeColor", lobbyData.loadout[i].unitColorInfo.rarityColors[2]);
            goMaterial.SetColor("_RarityFourColor", lobbyData.loadout[i].unitColorInfo.rarityColors[3]);
            goMaterial.SetFloat("_HasRarity", lobbyData.loadout[i].unitColorInfo.hasRarity ? 1f : 0f);
            loadoutImages[i].material = goMaterial;
        }
    }
    public void OnFightClick()
    {
        if (lobbyData == null)
        {
            return;
        }
        lobbyController.OnFightClick(lobbyData);
    }
}
