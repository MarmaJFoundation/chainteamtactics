using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum RoundType
{
    None = 0,
    Lost = 1,
    Won = 2
}

public class NotificationWindow : MonoBehaviour
{
    public bool indestructible;
    public CustomText[] messageTexts;
    public CustomText buttonText;
    public CustomText titleText;
    public GameObject forfeitButton;
    public Image rankImage;
    public Image edgeImage;
    public Image highlightImage;
    public CustomTooltip rankTooltip;
    private NotificationController notificationController;
    public RoomNotificationData notificationInfo;

    public void Setup(NotificationController notificationController, RoomNotificationData notificationInfo)
    {
        this.notificationController = notificationController;
        this.notificationInfo = notificationInfo;
        //string roomName = notificationInfo.room_id.Substring(0, notificationInfo.room_id.IndexOf('.') + 5);
        titleText.SetString(notificationInfo.room_id == Database.databaseStruct.playerAccount ? "your challenge" : notificationInfo.room_id + " challenge");
        //int rank = Database.databaseStruct.currentRank;
        //int rankIndex = BaseUtils.RankToIndex(rank);
        //rankImage.sprite = notificationController.rankController.rankSprites[rankIndex];
        //edgeImage.sprite = BaseUtils.rankEdges[Mathf.Clamp(Mathf.FloorToInt(rankIndex / 3), 0, 4)];
        //rankTooltip.tooltipText[0] = $"rank: {rank}";
        indestructible = false;
        forfeitButton.SetActive(false);
        switch (notificationInfo.notify_type)
        {
            case RoomNotificationType.RevokedRoom:
                messageTexts[0].SetString("Challenge revoked");
                messageTexts[1].SetString("fight has been terminated due to inactivity", notificationController.loseColor);
                messageTexts[2].gameObject.SetActive(true);
                messageTexts[2].SetString("pxt have been refunded if there were any", Color.white);
                buttonText.SetString("Okay");
                //forfeitButton.SetActive(false);
                break;
            case RoomNotificationType.PendingCreate:
                messageTexts[0].SetString("Finish setting up your challenge");
                messageTexts[1].SetString("You have pending tasks to do", Color.white);
                messageTexts[2].gameObject.SetActive(false);
                buttonText.SetString("Okay");
                //forfeitButton.SetActive(false);
                break;
            case RoomNotificationType.PendingJoin:
                messageTexts[0].SetString("Finish setting up your units");
                messageTexts[1].SetString("You have pending tasks to do", Color.white);
                messageTexts[2].gameObject.SetActive(false);
                buttonText.SetString("Okay");
                //forfeitButton.SetActive(false);
                break;
            case RoomNotificationType.RoundFinish:
                messageTexts[0].SetString("Round has finished");
                messageTexts[1].SetString("Watch the fight with the result", Color.white);
                messageTexts[2].gameObject.SetActive(false);
                buttonText.SetString("Watch");
                //forfeitButton.SetActive(true);
                break;
            case RoomNotificationType.FightFinish:
                indestructible = true;
                //forfeitButton.SetActive(false);
                messageTexts[0].SetString("Battle has finished");
                if (notificationInfo.hasFightStruct)
                {
                    messageTexts[1].SetString("Watch the fight with the result", Color.white);
                    messageTexts[2].gameObject.SetActive(false);
                    buttonText.SetString("Watch");
                }
                else
                {
                    messageTexts[2].gameObject.SetActive(true);
                    int gainAmount = 9;
                    int loseAmount = 5;
                    switch ((RoomTierTypes)notificationInfo.betType)
                    {
                        case RoomTierTypes.Tier1:
                            loseAmount = 5;
                            gainAmount = 9;
                            break;
                        case RoomTierTypes.Tier2:
                            loseAmount = 50;
                            gainAmount = 95;
                            break;
                        case RoomTierTypes.Tier3:
                            loseAmount = 500;
                            gainAmount = 950;
                            break;
                    }
                    if (notificationInfo.winner_id == Database.databaseStruct.playerAccount)
                    {
                        messageTexts[1].SetString("You have won the battle.", notificationController.winColor);
                        //#PXTBET
                        //messageTexts[2].SetString($"You won {gainAmount} pxt", notificationController.goldColor);
                        messageTexts[2].SetString($"No pxt was bet", Color.white);
                    }
                    else
                    {
                        messageTexts[1].SetString("You have been defeated.", notificationController.loseColor);
                        //#PXTBET
                        //messageTexts[2].SetString($"You lost {loseAmount} pxt", notificationController.loseColor);
                        messageTexts[2].SetString($"No pxt was bet", Color.white);
                    }
                    buttonText.SetString("Okay");
                }
                break;
        }
        StartCoroutine(BlinkCoroutine());
    }
    private IEnumerator BlinkCoroutine()
    {
        highlightImage.enabled = true;
        for (int i = 0; i < 3; i++)
        {
            float timer = 0;
            while (timer <= 1)
            {
                highlightImage.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, .9f), timer.Evaluate(CurveType.PeakParabol));
                timer += Time.deltaTime * 5;
                yield return null;
            }
            yield return new WaitForSeconds(.1f);
        }
        highlightImage.enabled = false;
    }
    public void OnRematchClick()
    {
        switch (notificationInfo.notify_type)
        {
            case RoomNotificationType.PendingCreate:
                if (Database.HasSixUnits())
                {
                    notificationController.mainMenuController.OnReceiveMapInfo(new MapInfo(notificationInfo.map_index));
                }
                else
                {
                    BaseUtils.ShowWarningMessage("Not possible", new string[2] { "you need atleast 6 units in order to play", "please hire new or buy some units." });
                }
                break;
            case RoomNotificationType.PendingJoin:
                if (Database.HasSixUnits())
                {
                    notificationController.lobbyController.OnJoinRoom(new FullRoomInfo() { loadout = notificationInfo.loadout, mapIndex = notificationInfo.map_index }, notificationInfo.room_id);
                }
                else
                {
                    BaseUtils.ShowWarningMessage("Not possible", new string[2] { "you need atleast 6 units in order to play", "please hire new or buy some units." });
                }
                break;
            case RoomNotificationType.RoundFinish:
                if (Database.HasSixUnits())
                {
                    notificationController.mainMenuController.OnReceiveMapInfo(new MapInfo(notificationInfo));
                }
                else
                {
                    BaseUtils.ShowWarningMessage("Not possible", new string[2] { "you need atleast 6 units in order to play", "please hire new or buy some units." });
                }
                break;
            case RoomNotificationType.FightFinish:
                if (notificationInfo.hasFightStruct)
                {
                    notificationController.mainMenuController.OnReceiveMapInfo(new MapInfo(notificationInfo));
                }
                else
                {
                    notificationController.RemoveNotification(this);
                    notificationController.OnBellClick();
                }
                break;
            case RoomNotificationType.RevokedRoom:
                notificationController.OnBellClick();
                break;
        }
    }
    public void OnForfeitClick()
    {
        int betAmount = 5;
        switch ((RoomTierTypes)notificationInfo.betType)
        {
            case RoomTierTypes.Tier1:
                betAmount = 5;
                break;
            case RoomTierTypes.Tier2:
                betAmount = 50;
                break;
            case RoomTierTypes.Tier3:
                betAmount = 500;
                break;
        }
        //#PXTBET
        //BaseUtils.ShowWarningMessage("wait!", new string[3] { "you are about to forfeit this match", $"by doing so, you will lose {betAmount} pxt and 10 rank points.", "are you sure you want to do that?" }, OnAcceptForfeit);
        BaseUtils.ShowWarningMessage("wait!", new string[3] { "you are about to forfeit this match", $"by doing so, you will lose 10 rank points.", "are you sure you want to do that?" }, OnAcceptForfeit);
    }
    private void OnAcceptForfeit()
    {
        notificationController.OnAcceptForfeit(notificationInfo);
    }
}
