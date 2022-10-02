using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;

public class RankCell : EnhancedScrollerCellView
{
    public int DataIndex { get; private set; }
    [HideInInspector]
    public RankData data;
    private RankController rankController;

    public CustomText rankText;
    public CustomText playerName;
    public CustomText posText;
    public Image edgeImage;
    public Image rankSymbol;
    public Image[] unitImages;
    public Image[] unitSlots;
    public CustomTooltip[] unitTooltips;
    public void SetData(RankController rankController, int dataIndex, RankData data)
    {
        this.rankController = rankController;
        this.data = data;
        DataIndex = dataIndex;
        posText.SetString($"{data.position}.");
        rankText.SetString($"Rank: {data.playerRank}");
        playerName.SetString(data.playerName);
        int rankIndex = BaseUtils.RankToIndex(data.playerRank);
        rankSymbol.sprite = rankController.rankSprites[rankIndex];
        edgeImage.sprite = BaseUtils.rankEdges[Mathf.Clamp(Mathf.FloorToInt(rankIndex / 3), 0, 4)];
        edgeImage.color = data.selfRank ? Color.white : new Color(1, 1, 1, .5f);
        for (int i = 0; i < data.loadout.Count; i++)
        {
            ScriptableUnit scriptableUnit = BaseUtils.unitDict[data.loadout[i].unitType];
            unitTooltips[i].tooltipText[0] = scriptableUnit.unitType.ToString();
            unitSlots[i].sprite = BaseUtils.tierSlots[scriptableUnit.unitTier - 1];
            unitImages[i].sprite = scriptableUnit.defaultImage;
            unitImages[i].transform.localScale = data.playerName != Database.databaseStruct.playerAccount ? new Vector3(-1, 1, 1) : Vector3.one;
            Material goMaterial = new Material(data.playerName != Database.databaseStruct.playerAccount ? BaseUtils.purpleMaterialUI : BaseUtils.yellowMaterialUI);
            goMaterial.SetColor("_HairColor", data.loadout[i].unitColorInfo.hairColor);
            goMaterial.SetColor("_SkinColor", data.loadout[i].unitColorInfo.skinColor);
            goMaterial.SetColor("_RarityOneColor", data.loadout[i].unitColorInfo.rarityColors[0]);
            goMaterial.SetColor("_RarityTwoColor", data.loadout[i].unitColorInfo.rarityColors[1]);
            goMaterial.SetColor("_RarityThreeColor", data.loadout[i].unitColorInfo.rarityColors[2]);
            goMaterial.SetColor("_RarityFourColor", data.loadout[i].unitColorInfo.rarityColors[3]);
            goMaterial.SetFloat("_HasRarity", data.loadout[i].unitColorInfo.hasRarity ? 1f : 0f);
            unitImages[i].material = goMaterial;
        }
    }
}
