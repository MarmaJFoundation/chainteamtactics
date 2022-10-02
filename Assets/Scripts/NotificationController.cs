using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationController : MonoBehaviour
{
    public NearHelper nearHelper;
    public RankController rankController;
    public LobbyController lobbyController;
    public RankAnimator rankAnimator;
    public MainMenuController mainMenuController;
    public Color winColor;
    public Color loseColor;
    public Color goldColor;
    public Sprite[] bellSprites;
    public Sprite[] buttonSprites;
    public Image bellImage;
    public Image bellEdge;
    public Transform notificationParent;
    public GameObject notificationPrefab;
    public static readonly List<RoomNotificationData> notifications = new List<RoomNotificationData>();
    private readonly List<NotificationWindow> activeWindows = new List<NotificationWindow>();
    private float timer;
    private RoomNotificationData notificationData;
    public void Setup()
    {
        if (BaseUtils.offlineMode)
        {
            /*if (notifications.Count == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    AddFakeNotification(1 * i, RoundType.None, RoundType.Won);
                    AddFakeNotification(2 * i, RoundType.None, RoundType.Lost);
                    AddFakeNotification(3 * i, RoundType.Won, RoundType.None);
                    AddFakeNotification(4 * i, RoundType.Lost, RoundType.None);
                }
            }
            OnReceiveNotifications();*/
        }
        else
        {
            //nearHelper.RequestNotifications();
        }
        OnReceiveNewPlayerData();
    }
    private void Update()
    {
        if (!BaseUtils.onTavern || mainMenuController.currentWindow != WindowType.Menu)
        {
            timer = 0;
            return;
        }
        timer += Time.deltaTime;
        if (timer > 30)
        {
            timer = 0;
            OnBellClick(true);
        }
    }
    public void OnReceiveNewPlayerData()
    {
        timer = 0;
        UpdateBellImage();
        for (int i = activeWindows.Count - 1; i >= 0; i--)
        {
            if (activeWindows[i].indestructible)
            {
                continue;
            }
            Destroy(activeWindows[i].gameObject);
            activeWindows.RemoveAt(i);
        }
        for (int i = 0; i < notifications.Count; i++)
        {
            /*if (notifications[i].notify_type == RoomNotificationType.PendingJoin && notifications[i].target_ids[0] != Database.databaseStruct.playerAccount)
            {
                StartCoroutine(nearHelper.RequestRevokeRoom());
                return;
            }*/
            GameObject notificationObj = Instantiate(notificationPrefab, notificationParent);
            NotificationWindow notificationWindow = notificationObj.GetComponent<NotificationWindow>();
            notificationWindow.Setup(this, notifications[i]);
            activeWindows.Add(notificationWindow);
        }
    }
    public void OnAcceptForfeit(RoomNotificationData notificationData)
    {
        this.notificationData = notificationData;
        StartCoroutine(nearHelper.RequestForfeitRoom());
    }
    public void ShowRewards(bool victory)
    {
        StartCoroutine(RewardAnimation(victory));
    }
    private IEnumerator RewardAnimation(bool victory)
    {
        int betAmount = 5;
        int gainAmount = 9;
        switch ((RoomTierTypes)notificationData.betType)
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
        yield return new WaitForSeconds(4);
        rankAnimator.gameObject.SetActive(false);
        OnBellClick();
    }
    /*private void AddFakeNotification(int ID, RoundType fightType, RoundType roundType)
    {
        notifications.Add(new NotificationInfo()
        {
            challengerName = "offlineplayer" + ID,
            challengerRank = BaseUtils.RandomInt(800, 2000),
            fightType = fightType,
            roundType = roundType,
            notificationID = ID
        });
    }*/
    public void OnBellClick(bool skipLoadingScreen = false)
    {
        if (BaseUtils.offlineMode)
        {
            OnReceiveNewPlayerData();
        }
        else
        {
            nearHelper.dataGetState = DataGetState.AfterNotification;
            StartCoroutine(nearHelper.GetPlayerData(skipLoadingScreen));
        }
    }
    public void RemoveNotification(NotificationWindow notificationWindow)
    {
        for (int i = 0; i < activeWindows.Count; i++)
        {
            if (activeWindows[i] == notificationWindow)
            {
                activeWindows.RemoveAt(i);
                break;
            }
        }
        Destroy(notificationWindow.gameObject);
    }
    private void UpdateBellImage()
    {
        bellImage.sprite = bellSprites[notifications.Count > 0 ? 1 : 0];
        bellEdge.sprite = buttonSprites[notifications.Count > 0 ? 1 : 0];
    }
}
