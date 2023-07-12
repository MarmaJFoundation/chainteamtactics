using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum WindowType
{
    Menu = 0,
    Market = 1,
    Match = 2,
    Office = 3,
    Rank = 4,
    Encyc = 5,
    Lobby = 6
}
public class MainMenuController : MonoBehaviour
{
    public NearHelper nearHelper;
    public TavernController tavernController;
    public TooltipController tooltipController;
    public MarketController marketController;
    public LobbyController lobbyController;
    public OfficeController officeController;
    public NotificationController notificationController;
    public RankController rankController;
    public MapController mapController;
    public CanvasScaler canvasScaler;
    public GameObject warningObj;
    public Image warningImage;
    public CustomText[] warningTexts;
    private RectTransform tooltipTarget;
    private Coroutine tooltipCoroutine;
    private Coroutine warningCoroutine;
    public RectTransform textTooltip;
    public CustomText[] textTooltipTexts;
    private TooltipPosition tooltipPosition;
    private RectTransform textTooltipTarget;
    public CustomText playerNameText;
    public CustomText balanceText;
    public CustomText rankText;
    public CustomTooltip customTooltip;
    public Image smallRankImage;
    public Image smallRankEdge;
    public CustomText smallRankText;
    public GameObject smallRankObj;
    public Image[] rankImages;
    public Sprite[] rankPlateSprites;
    public Sprite[] rankSymbolSprites;
    public Sprite[] rankLetterSprites;
    public Image unitPreviewEdge;
    public Image unitPreviewImage;
    public Image fadeScreen;
    public Canvas menuCanvas;
    public UnitInfo draggingUnit;
    public GameObject blackScreen;
    [HideInInspector]
    public bool showingPreview;
    private bool gameTooltip;
    public WindowType currentWindow;
    public GameObject maintenanceWindow;
    public GameObject authWindow;
    public GraphicRaycaster[] canvasRaycasters;
    public void Setup(bool keepPage = false)
    {
        menuCanvas.gameObject.SetActive(true);
        menuCanvas.enabled = true;
        UpdateBalance();
        UpdateRank();
        notificationController.Setup();
        playerNameText.SetString(Database.databaseStruct.playerAccount);
        tavernController.Setup(keepPage);
        FadeScreenOut();
    }
    public void OnMintClick()
    {
        Application.OpenURL("https://ecosystem.pixeldapps.co/?page=ctt");
    }
    public void OnReceiveMapInfo(MapInfo mapInfo)
    {
        StartCoroutine(WaitAndDispose(mapInfo));
    }
    private IEnumerator WaitAndDispose(MapInfo mapInfo)
    {
        FadeScreenIn();
        yield return new WaitForSeconds(1 / 3f);
        if (currentWindow != WindowType.Menu)
        {
            OnWindowSet(0);
        }
        menuCanvas.gameObject.SetActive(false);
        menuCanvas.enabled = false;
        tavernController.Dispose();
        mapController.Setup(mapInfo);
        FadeScreenOut();
    }
    public void OnWindowSet(int windowIndex)
    {
        notificationController.Setup();
        smallRankObj.SetActive(false);
        blackScreen.SetActive(windowIndex != 0 && windowIndex != 2);
        switch (currentWindow)
        {
            case WindowType.Market:
                marketController.Dispose();
                break;
            case WindowType.Office:
                officeController.Dispose();
                break;
            case WindowType.Rank:
                rankController.Dispose();
                break;
            case WindowType.Encyc:
                //encycController.Dispose();
                break;
            case WindowType.Lobby:
                lobbyController.Dispose();
                break;
        }
        if ((WindowType)windowIndex == currentWindow)
        {
            OnWindowSet(0);
            return;
        }
        currentWindow = (WindowType)windowIndex;
        switch (currentWindow)
        {
            case WindowType.Market:
                smallRankObj.SetActive(true);
                marketController.Setup();
                break;
            case WindowType.Office:
                officeController.Setup();
                break;
            case WindowType.Rank:
                rankController.Setup();
                break;
            case WindowType.Encyc:
                //encycController.Setup();
                break;
            case WindowType.Lobby:
                smallRankObj.SetActive(true);
                lobbyController.Setup();
                break;
        }
        HideTextTooltip();
        Database.SaveDatabase();
    }
    public void UpdateBalance()
    {
        balanceText.SetString($"balance: {Database.databaseStruct.pixelTokens} pxt");
    }
    public void UpdateRank()
    {
        rankText.SetString($"rank: {Database.databaseStruct.currentRank}");
        smallRankText.SetString($"rank: {Database.databaseStruct.currentRank}");
        int rankIndex = BaseUtils.RankToIndex(Database.databaseStruct.currentRank);
        string rankName = BaseUtils.RankToName(rankIndex);
        smallRankImage.sprite = rankController.rankSprites[rankIndex];
        smallRankEdge.sprite = BaseUtils.rankEdges[Mathf.Clamp(Mathf.FloorToInt(rankIndex / 3), 0, 4)];
        //plate
        rankImages[0].sprite = rankPlateSprites[Mathf.Clamp(Mathf.FloorToInt(rankIndex / 3), 0, 4)];
        //symbol
        rankImages[1].sprite = rankSymbolSprites[Mathf.Clamp(Mathf.FloorToInt(rankIndex / 3), 0, 4)];
        //letter
        rankImages[2].sprite = rankLetterSprites[rankIndex];
        for (int i = 0; i < 3; i++)
        {
            rankImages[i].SetNativeSize();
        }
        customTooltip.tooltipText[0] = rankName;
        rankImages[3].sprite = BaseUtils.rankEdges[Mathf.Clamp(Mathf.FloorToInt(rankIndex / 3), 0, 4)];
        rankImages[4].sprite = BaseUtils.rankSlots[Mathf.Clamp(Mathf.FloorToInt(rankIndex / 3), 0, 4)];
    }
    public void OnAddTokenClick()
    {
        HideTextTooltip();
        Application.OpenURL("https://app.ref.finance/#wrap.near%7Cpixeltoken.near");
    }
    public void OnLogoutClick()
    {
        nearHelper.Logout(true);
        Database.SaveDatabase();

        NearHelper.WSLogout();
        //SceneManager.LoadScene(0);
    }
    public void OnAuthorizedClick()
    {
        switch (nearHelper.dataGetState)
        {
            case DataGetState.MarketPurchase:
                marketController.OnAuthorizedClick();
                break;
        }
    }
    public void OnDiscordClick()
    {
        Application.OpenURL("https://discord.gg/xFAAa8Db6f");
    }
    public void OnTwitterClick()
    {
        Application.OpenURL("https://twitter.com/PixelDapps");
    }
    private void LateUpdate()
    {
        if (unitPreviewEdge.gameObject.activeSelf)
        {
            unitPreviewEdge.rectTransform.position = BaseUtils.thisCam.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 20);
        }
        if (textTooltip.gameObject.activeSelf)
        {
            Vector3 offsetPos = Vector3.zero;
            switch (tooltipPosition)
            {
                case TooltipPosition.BottomCenter:
                    offsetPos += Vector3.down * (textTooltip.sizeDelta.y / 2 + textTooltipTarget.sizeDelta.y / 2);
                    break;
                case TooltipPosition.BottomRight:
                    offsetPos += Vector3.down * (textTooltip.sizeDelta.y / 2 + textTooltipTarget.sizeDelta.y / 2);
                    offsetPos += Vector3.right * (textTooltip.sizeDelta.x / 2 + textTooltipTarget.sizeDelta.x / 2);
                    break;
                case TooltipPosition.BottomLRight:
                    offsetPos += Vector3.down * (textTooltip.sizeDelta.y / 2 + textTooltipTarget.sizeDelta.y / 2);
                    offsetPos += Vector3.right * textTooltipTarget.sizeDelta.x / 2;
                    break;
                case TooltipPosition.BottomLeft:
                    offsetPos += Vector3.down * (textTooltip.sizeDelta.y / 2 + textTooltipTarget.sizeDelta.y / 2);
                    offsetPos += Vector3.left * (textTooltip.sizeDelta.x / 2 + textTooltipTarget.sizeDelta.x / 2);
                    break;
                case TooltipPosition.TopCenter:
                    offsetPos += Vector3.up * (textTooltip.sizeDelta.y / 2 + textTooltipTarget.sizeDelta.y / 2);
                    break;
                case TooltipPosition.TopRight:
                    offsetPos += Vector3.up * (textTooltip.sizeDelta.y / 2 + textTooltipTarget.sizeDelta.y / 2);
                    offsetPos += Vector3.right * (textTooltip.sizeDelta.x / 2 + textTooltipTarget.sizeDelta.x / 2);
                    break;
                case TooltipPosition.TopLRight:
                    offsetPos += Vector3.up * (textTooltip.sizeDelta.y / 2 + textTooltipTarget.sizeDelta.y / 2);
                    offsetPos += Vector3.right * textTooltipTarget.sizeDelta.x / 2;
                    break;
                case TooltipPosition.TopLeft:
                    offsetPos += Vector3.up * (textTooltip.sizeDelta.y / 2 + textTooltipTarget.sizeDelta.y / 2);
                    offsetPos += Vector3.left * (textTooltip.sizeDelta.x / 2 + textTooltipTarget.sizeDelta.x / 2);
                    break;
                case TooltipPosition.Right:
                    offsetPos += Vector3.right * (textTooltip.sizeDelta.x / 2 + textTooltipTarget.sizeDelta.x / 2);
                    break;
            }
            offsetPos *= canvasScaler.scaleFactor;
            textTooltip.position = BaseUtils.thisCam.ScreenToWorldPoint(BaseUtils.thisCam.WorldToScreenPoint(textTooltipTarget.position) + offsetPos);
            if (!textTooltipTarget.gameObject.activeSelf || !textTooltipTarget.parent.gameObject.activeSelf)
            {
                HideTextTooltip();
            }
        }
        if (tooltipController.gameObject.activeSelf)
        {
            if (tooltipTarget == null)
            {
                HideTooltip();
                return;
            }
            Vector3 offsetPos = Vector3.zero;
            Vector3 screenPos = BaseUtils.thisCam.WorldToScreenPoint(tooltipTarget.position);
            if (gameTooltip)
            {
                if (screenPos.x > Screen.width / 2)
                {
                    offsetPos += Vector3.left * (10 + tooltipController.rectTransform.sizeDelta.x / 2);
                }
                else
                {
                    offsetPos += Vector3.right * (10 + tooltipController.rectTransform.sizeDelta.x / 2);
                }
            }
            else
            {
                if (screenPos.x > Screen.width / 2)
                {
                    offsetPos += Vector3.left * (tooltipTarget.sizeDelta.x / 2 + tooltipController.rectTransform.sizeDelta.x / 2);
                }
                else
                {
                    offsetPos += Vector3.right * (tooltipTarget.sizeDelta.x / 2 + tooltipController.rectTransform.sizeDelta.x / 2);
                }
                if (screenPos.y < Screen.height / 2)
                {
                    offsetPos += Vector3.up * (-tooltipTarget.sizeDelta.y / 2 + tooltipController.rectTransform.sizeDelta.y / 2);
                }
                else
                {
                    offsetPos += Vector3.down * (-tooltipTarget.sizeDelta.y / 2 + tooltipController.rectTransform.sizeDelta.y / 2);
                }
            }
            offsetPos *= canvasScaler.scaleFactor;
            tooltipController.rectTransform.position = BaseUtils.thisCam.ScreenToWorldPoint(BaseUtils.thisCam.WorldToScreenPoint(tooltipTarget.position) + offsetPos);
            if (!tooltipTarget.gameObject.activeSelf || !tooltipTarget.parent.gameObject.activeSelf)
            {
                HideTooltip();
            }
        }
    }
    public void BeginDrag(UnitCell unitCell)
    {
        if (showingPreview)
        {
            return;
        }
        showingPreview = true;
        draggingUnit = unitCell.unitInfo;
        unitPreviewEdge.gameObject.SetActive(true);
        unitPreviewEdge.sprite = unitCell.edgeImage.sprite;
        unitPreviewImage.material = unitCell.unitImage.material;
        unitPreviewImage.sprite = unitCell.unitImage.sprite;
    }
    public void EndDrag()
    {
        showingPreview = false;
        unitPreviewEdge.gameObject.SetActive(false);
    }
    public void SwitchUnits(UnitCell unitInfoA, UnitCell unitInfoB)
    {
        if (unitInfoB == null)
        {
            return;
        }
        int indexA = unitInfoA.visualIndex;
        int indexB = unitInfoB.visualIndex;
        int idA = unitInfoA.emptyCell ? -1 : unitInfoA.unitInfo.unitID;
        int idB = unitInfoB.emptyCell ? -1 : unitInfoB.unitInfo.unitID;
        Database.SetVisualIndex(idA, indexB);
        Database.SetVisualIndex(idB, indexA);
        tavernController.Setup(true, indexA <= 16 || indexB <= 16);
    }
    public void ShowTextTooltip(RectTransform target, string[] tooltipText, TooltipPosition tooltipPosition)
    {
        this.tooltipPosition = tooltipPosition;
        textTooltipTarget = target;
        for (int i = 0; i < textTooltipTexts.Length; i++)
        {
            textTooltipTexts[i].gameObject.SetActive(i < tooltipText.Length);
            if (i < tooltipText.Length)
            {
                textTooltipTexts[i].SetString(tooltipText[i]);
            }
        }
        textTooltip.gameObject.SetActive(true);
    }
    public void HideTextTooltip()
    {
        textTooltip.gameObject.SetActive(false);
    }
    public void ShowWarning(string text1, string text2)
    {
        warningTexts[0].SetString(text1);
        warningTexts[1].SetString(text2);
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }
        warningCoroutine = StartCoroutine(WarningCoroutine());
    }
    private IEnumerator WarningCoroutine()
    {
        warningObj.SetActive(true);
        float timer = 0;
        while (timer <= 1)
        {
            warningObj.transform.position = Vector3.Lerp(Vector3.down * 3, Vector3.zero, timer.Evaluate(CurveType.EaseOut));
            warningTexts[0].SetString(Color.Lerp(new Color(1, 1, 1, 0), Color.white, timer.Evaluate(CurveType.EaseOut)));
            warningTexts[1].SetString(Color.Lerp(new Color(1, 1, 1, 0), Color.white, timer.Evaluate(CurveType.EaseOut)));
            warningImage.color = Color.Lerp(new Color(1, 1, 1, 0), Color.white, timer.Evaluate(CurveType.EaseOut));
            timer += Time.deltaTime * 4;
            yield return null;
        }
        warningObj.transform.position = Vector3.zero;
        warningTexts[0].SetString(Color.white);
        warningTexts[1].SetString(Color.white);
        warningImage.color = Color.white;
        yield return new WaitForSeconds(2);
        timer = 0;
        while (timer <= 1)
        {
            warningObj.transform.position = Vector3.Lerp(Vector3.zero, Vector3.down * 3, timer.Evaluate(CurveType.EaseOut));
            warningTexts[0].SetString(Color.Lerp(Color.white, new Color(1, 1, 1, 0), timer.Evaluate(CurveType.EaseOut)));
            warningTexts[1].SetString(Color.Lerp(Color.white, new Color(1, 1, 1, 0), timer.Evaluate(CurveType.EaseOut)));
            warningImage.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), timer.Evaluate(CurveType.EaseOut));
            timer += Time.deltaTime * 4;
            yield return null;
        }
        warningObj.SetActive(false);
    }
    public void ShowTooltip(RectTransform target, UnitInfo unitInfo, int currentHealth, bool isPurple, bool fromClick = false, bool randomUnit = false, bool fromMarket = false)
    {
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
        }
        gameTooltip = fromClick;
        if (fromClick)
        {
            tooltipTarget = target;
            tooltipController.Setup(unitInfo, currentHealth, isPurple, randomUnit, fromMarket);
        }
        else
        {
            tooltipCoroutine = StartCoroutine(WaitAndShowTooltip(target, unitInfo, currentHealth, isPurple, randomUnit, fromMarket));
        }
    }
    private IEnumerator WaitAndShowTooltip(RectTransform target, UnitInfo unitInfo, int currentHealth, bool isPurple, bool randomUnit, bool fromMarket)
    {
        float timer = 0;
        Vector3 mousePos = Input.mousePosition;
        while (timer <= 1)
        {
            if (Vector3.Distance(Input.mousePosition, mousePos) > .3f)
            {
                tooltipCoroutine = StartCoroutine(WaitAndShowTooltip(target, unitInfo, currentHealth, isPurple, randomUnit, fromMarket));
                yield break;
            }
            timer += Time.deltaTime * 5;
            yield return null;
        }
        tooltipTarget = target;
        tooltipController.Setup(unitInfo, currentHealth, isPurple, randomUnit, fromMarket);
    }
    public void HideTooltip()
    {
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
        }
        tooltipController.Dispose();
    }
    public void FadeScreenIn()
    {
        StartCoroutine(FadeScreenInCoroutine());
    }
    private IEnumerator FadeScreenInCoroutine()
    {
        fadeScreen.gameObject.SetActive(true);
        float timer = 0;
        while (timer <= 1)
        {
            fadeScreen.color = Color.Lerp(Color.clear, Color.black, timer.Evaluate(CurveType.EaseIn));
            timer += Time.deltaTime * 3;
            yield return null;
        }
    }
    public void FadeScreenOut()
    {
        StartCoroutine(FadeScreenOutCoroutine());
    }
    private IEnumerator FadeScreenOutCoroutine()
    {
        fadeScreen.gameObject.SetActive(true);
        float timer = 0;
        while (timer <= 1)
        {
            fadeScreen.color = Color.Lerp(Color.black, Color.clear, timer.Evaluate(CurveType.EaseOut));
            timer += Time.deltaTime * 3;
            yield return null;
        }
        fadeScreen.gameObject.SetActive(false);
    }
    public bool GetCanvasCasts(out List<RaycastResult> raycastResults)
    {
        raycastResults = new List<RaycastResult>();
        PointerEventData pointerEvent = new PointerEventData(null)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };
        for (int i = 0; i < canvasRaycasters.Length; i++)
        {
            canvasRaycasters[i].Raycast(pointerEvent, raycastResults);
        }
        return raycastResults.Count > 0;
    }
    public void SetMaintenanceMode()
    {
        maintenanceWindow.SetActive(true);
    }
}
