using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankAnimator : MonoBehaviour
{
    public MainMenuController mainMenuController;
    public CustomText rankText;
    public CustomText eloText;
    public CustomText balanceText;
    public CustomText addBalanceText;
    public CustomText addEloText;
    public Image plateImage;
    public Image symbolImage;
    public Image numbersImage;
    public Image backImage;
    public Image barImage;
    public GameObject balanceObj;
    public GameObject[] exploObjs;
    public GameObject[] effectObjs;
    public Color[] rankColors;
    public Color loseColor;
    public Color addColor;
    public Color goldColor;
    private int previousIndex;

    public void Setup(int fromRank, int gotoRank, int fromBalance, int gotoBalance, int addedElo, int addedBalance)
    {
        previousIndex = -1;
        gameObject.SetActive(true);
        if (addedBalance == 0)
        {
            addBalanceText.SetString("", Color.white);
        }
        else if (addedBalance > 0)
        {
            addBalanceText.SetString($"+{addedBalance}", goldColor);
        }
        else
        {
            addBalanceText.SetString($"-{addedBalance * -1}", loseColor);
        }
        if (addedElo > 0)
        {
            addEloText.SetString($"+{addedElo}", addColor);
        }
        else
        {
            addEloText.SetString($"-{-addedElo}", loseColor);
        }
        //balanceObj.SetActive(addBalance > 0);
        SetRankImages(fromRank, fromBalance, true);
        StartCoroutine(LerpRank(fromRank, gotoRank, fromBalance, gotoBalance));
    }
    private IEnumerator LerpRank(int currentRank, int gotoRank, int currentBalance, int gotoBalance)
    {
        float timer = 0;
        int lerpRank;
        int lerpBalance;
        while (timer <= 1)
        {
            lerpRank = Mathf.RoundToInt(Mathf.Lerp(currentRank, gotoRank, timer));
            lerpBalance = Mathf.RoundToInt(Mathf.Lerp(currentBalance, gotoBalance, timer));
            SetRankImages(lerpRank, lerpBalance);
            timer += Time.deltaTime;
            yield return null;
        }
        SetRankImages(gotoRank, gotoBalance);
    }
    private void SetRankImages(int currentRank, int currentBalance, bool ignoreExplo = false)
    {
        rankText.SetString($"rank: {currentRank}");
        balanceText.SetString($"balance: {currentBalance} pxt");
        int testIndex = BaseUtils.RankToIndex(currentRank);
        if (previousIndex != testIndex)
        {
            bool willExplo = testIndex > previousIndex;
            previousIndex = testIndex;
            eloText.SetString(BaseUtils.RankToName(previousIndex));
            int setIndex = Mathf.Clamp(Mathf.FloorToInt(previousIndex / 3), 0, 4);
            plateImage.sprite = mainMenuController.rankPlateSprites[setIndex];
            symbolImage.sprite = mainMenuController.rankSymbolSprites[setIndex];
            numbersImage.sprite = mainMenuController.rankLetterSprites[previousIndex];
            backImage.sprite = BaseUtils.rankEdges[setIndex];
            barImage.color = rankColors[setIndex];
            for (int i = 0; i < effectObjs.Length; i++)
            {
                effectObjs[i].SetActive(i == setIndex);
            }
            if (!ignoreExplo && willExplo)
            {
                for (int i = 0; i < effectObjs.Length; i++)
                {
                    exploObjs[i].SetActive(i == setIndex);
                }
                StartCoroutine(BumpPlate());
            }
        }
        int prevRank = GetNextRank(currentRank) - 200;
        int nextRank = GetNextRank(currentRank);
        barImage.rectTransform.localScale = new Vector3((Mathf.Clamp01((float)(currentRank - prevRank) / (nextRank - prevRank))), 1, 1);
    }
    private IEnumerator BumpPlate()
    {
        float timer = 0;
        while (timer <= 1)
        {
            plateImage.rectTransform.localScale = Vector3.Lerp(Vector3.one * 2, Vector3.one * 3, timer.Evaluate(CurveType.PeakParabol));
            timer += Time.deltaTime * 2;
            yield return null;
        }
        plateImage.rectTransform.localScale = Vector3.one * 2;
    }
    private int GetNextRank(int previousRank)
    {
        return 800 + Mathf.Clamp(Mathf.CeilToInt((previousRank - 800) / 200) * 200 + 200, 200, 2600);
    }
}
