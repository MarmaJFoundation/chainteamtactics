using System;
using System.Collections.Generic;
using UnityEngine;

public class ChainTeamTactics
{
    public static void FillDatabaseFromPlayerResponse(PlayerDataResponse res)
    {
        Database.databaseStruct.pixelTokens = (int)((long.Parse(res.balance.pixeltoken) / 1000000));
        Database.databaseStruct.fightBalance = res.playerdata.fight_balance;
        Database.databaseStruct.currentRank = res.playerdata.rating;
        Database.databaseStruct.ownedUnits.Clear();
        Database.databaseStruct.visualIndexes.Clear();
        List<UnitInfo> units = ConvertUnits(res.units);
        //Database.GetUnitsOnSale();
        for (int i = 0; i < units.Count; i++)
        {
            if (Database.UnitHasVisualIndex(units[i].unitID))
            {
                Database.databaseStruct.ownedUnits.Add(units[i]);
            }
            else
            {
                Database.AddUnit(units[i]);
            }
            if (Database.unitsOnSale.Contains(units[i].unitID))
            {
                Database.unitsOnSale.Remove(units[i].unitID);
            }
        }
        if (res.notifications != null)
        {
            NotificationController.notifications.Clear();
            for (int i = 0; i < res.notifications.Length; i++)
            {
                NotificationController.notifications.Add(res.notifications[i]);
            }
        }
        else
        {
            Debug.Log("notifications are null");
        }
        Database.SaveDatabase();
    }
    private static List<UnitInfo> ConvertUnits(List<UnitToken> units)
    {
        List<UnitInfo> returnUnits = new List<UnitInfo>();
        for (int i = 0; i < units.Count; i++)
        {
            returnUnits.Add(new UnitInfo(units[i]));
        }
        return returnUnits;
    }
}
