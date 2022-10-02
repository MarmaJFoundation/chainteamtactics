using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
public struct UnitInfo
{
    public UnitType unitType;
    public UnitColorInfo unitColorInfo;
    public int unitID;
    public int speed;
    public int damage;
    public int health;
    public int price;
    public UnitInfo(LobbyLoadoutData partialLoadoutData)
    {
        int.TryParse(partialLoadoutData.token_id, out unitID);
        unitType = (UnitType)partialLoadoutData.unit_type;
        unitColorInfo = BaseUtils.GetUnitColorInfo(unitID);
        speed = 0;
        damage = 0;
        health = 0;
        price = 0;
    }
    public UnitInfo(UnitToken unitToken)
    {
        int.TryParse(unitToken.token_id, out unitID);
        unitType = (UnitType)unitToken.unit_type;
        unitColorInfo = BaseUtils.GetUnitColorInfo(unitID);
        speed = unitToken.speed;
        damage = unitToken.damage;
        health = unitToken.health;
        price = (int)(unitToken.price / 1000000);
    }
    public UnitInfo(UnitType unitType, UnitColorInfo unitColorInfo, int unitID, int speed, int damage, int health, int price)
    {
        this.health = health;
        this.unitType = unitType;
        this.unitColorInfo = unitColorInfo;
        this.unitID = unitID;
        this.speed = speed;
        this.damage = damage;
        this.price = price;
    }
    public UnitInfo(UnitInfo unitInfo, int health, int price)
    {
        this.price = price;
        this.health = health;
        unitType = unitInfo.unitType;
        unitColorInfo = unitInfo.unitColorInfo;
        unitID = unitInfo.unitID;
        speed = unitInfo.speed;
        damage = unitInfo.damage;
    }
    public UnitInfo(UnitInfo unitInfo)
    {
        health = unitInfo.health;
        unitType = unitInfo.unitType;
        unitColorInfo = unitInfo.unitColorInfo;
        unitID = unitInfo.unitID;
        speed = unitInfo.speed;
        damage = unitInfo.damage;
        price = unitInfo.price;
    }
}
/*public class AccSaleInfo
{
    public string playerAccount;
    public List<UnitInfo> unitInfo;
}*/
public class DatabaseStruct
{
    public string playerAccount;
    public string publicKey;
    public string privateKey;
    public bool validCredentials;
    public bool firstLogin;
    public int pixelTokens;
    public int lockSpeed = -1;
    public int fightBalance;
    public int currentRank;
    public float allowance;
    //public List<AccSaleInfo> unitSaleInfos = new List<AccSaleInfo>();
    public List<UnitInfo> ownedUnits = new List<UnitInfo>();
    public List<VisualUnitStruct> visualIndexes = new List<VisualUnitStruct>();
}
public struct VisualUnitStruct
{
    public int visualIndex;
    public int unitID;

    public VisualUnitStruct(int visualIndex, int unitID)
    {
        this.visualIndex = visualIndex;
        this.unitID = unitID;
    }
}
public class Database : MonoBehaviour
{
    public static DatabaseStruct databaseStruct;
    private static string databaseName;
    private static readonly string currentVersion = "0.1";
    public static readonly int maxUnits = 36;
    public static readonly List<int> unitsOnSale = new List<int>();
    private void Awake()
    {
        databaseName = "CTTDatabase";
        if (PlayerPrefs.HasKey("CTTVersion"))
        {
            string clientVersion = PlayerPrefs.GetString("CTTVersion");
            if (clientVersion != currentVersion && PlayerPrefs.HasKey(databaseName))
            {
                PlayerPrefs.DeleteKey(databaseName);
            }
        }
        else if (PlayerPrefs.HasKey(databaseName))
        {
            PlayerPrefs.DeleteKey(databaseName);
        }

        PlayerPrefs.SetString("CTTVersion", currentVersion);

        if (PlayerPrefs.HasKey(databaseName))
        {
            LoadDatabase();
            if (BaseUtils.offlineMode)
            {
                databaseStruct.playerAccount = "offlineplayer.near";
                databaseStruct.currentRank = 2180;
                databaseStruct.pixelTokens = 32100;
            }
        }
        else
        {
            databaseStruct = new DatabaseStruct();
            if (BaseUtils.offlineMode)
            {
                databaseStruct.playerAccount = "offlineplayer.near";
                databaseStruct.currentRank = 2180;
                databaseStruct.pixelTokens = 32100;
            }
            SaveDatabase();
        }
    }
    public static void AddUnit(UnitInfo unitInfo)
    {
        if (databaseStruct.ownedUnits.Count >= maxUnits)
        {
            return;
        }
        List<int> emptyVisualIndexes = new List<int>();
        for (int i = 0; i < maxUnits; i++)
        {
            emptyVisualIndexes.Add(i);
        }
        for (int i = 0; i < databaseStruct.visualIndexes.Count; i++)
        {
            emptyVisualIndexes.Remove(databaseStruct.visualIndexes[i].visualIndex);
        }
        emptyVisualIndexes.Sort((x, y) => x.CompareTo(y));
        databaseStruct.ownedUnits.Add(unitInfo);
        databaseStruct.visualIndexes.Add(new VisualUnitStruct(emptyVisualIndexes[0], unitInfo.unitID));
    }
    public static void RemoveUnit(int unitID)
    {
        for (int i = 0; i < databaseStruct.ownedUnits.Count; i++)
        {
            if (databaseStruct.ownedUnits[i].unitID == unitID)
            {
                databaseStruct.ownedUnits.RemoveAt(i);
                break;
            }
        }
        for (int i = 0; i < databaseStruct.visualIndexes.Count; i++)
        {
            if (databaseStruct.visualIndexes[i].unitID == unitID)
            {
                databaseStruct.visualIndexes.RemoveAt(i);
                break;
            }
        }
    }
    public static bool UnitHasVisualIndex(int unitID)
    {
        for (int i = 0; i < databaseStruct.visualIndexes.Count; i++)
        {
            if (databaseStruct.visualIndexes[i].unitID == unitID)
            {
                return true;
            }
        }
        return false;
    }
    public static void SetVisualIndex(int unitID, int visualIndex)
    {
        for (int i = 0; i < databaseStruct.visualIndexes.Count; i++)
        {
            if (databaseStruct.visualIndexes[i].unitID == unitID)
            {
                databaseStruct.visualIndexes[i] = new VisualUnitStruct(visualIndex, unitID);
                break;
            }
        }
    }
    public static bool HasSixUnits()
    {
        int unitCount = 0;
        for (int i = 0; i < databaseStruct.ownedUnits.Count; i++)
        {
            if (databaseStruct.ownedUnits[i].price == 0)
            {
                unitCount++;
                if (unitCount >= 6)
                {
                    return true;
                }
            }
        }
        Debug.Log(unitCount);
        return unitCount >= 6;
    }
    public static bool HasNonSupportUnits()
    {
        int nonSupportAmount = 0;
        for (int i = 0; i < databaseStruct.ownedUnits.Count; i++)
        {
            if (!BaseUtils.unitDict[databaseStruct.ownedUnits[i].unitType].supports && databaseStruct.ownedUnits[i].price == 0)
            {
                nonSupportAmount++;
                if (nonSupportAmount >= 3)
                {
                    return true;
                }
            }
        }
        return nonSupportAmount >= 3;
    }
    public static bool OwnsUnit(int unitID)
    {
        for (int i = 0; i < databaseStruct.ownedUnits.Count; i++)
        {
            if (databaseStruct.ownedUnits[i].unitID == unitID)
            {
                return true;
            }
        }
        return false;
    }
    /*public static void GetUnitsOnSale()
    {
        unitsOnSale.Clear();
        for (int i = 0; i < databaseStruct.unitSaleInfos.Count; i++)
        {
            if (databaseStruct.unitSaleInfos[i].playerAccount == databaseStruct.playerAccount)
            {
                for (int k = 0; k < databaseStruct.unitSaleInfos[i].unitInfo.Count; k++)
                {
                    unitsOnSale.Add(databaseStruct.unitSaleInfos[i].unitInfo[k].unitID);
                }
            }
        }
    }
    public static void AddSale(int unitID)
    {
        int accountIndex = -1;
        for (int k = 0; k < databaseStruct.unitSaleInfos.Count; k++)
        {
            if (databaseStruct.unitSaleInfos[k].playerAccount == databaseStruct.playerAccount)
            {
                accountIndex = k;
                break;
            }
        }
        if (accountIndex == -1)
        {
            databaseStruct.unitSaleInfos.Add(new AccSaleInfo() { unitInfo = new List<UnitInfo>(), playerAccount = databaseStruct.playerAccount });
            accountIndex = databaseStruct.unitSaleInfos.Count - 1;
        }
        databaseStruct.unitSaleInfos[accountIndex].unitInfo.Add(GetUnit(unitID));
    }
    public static UnitInfo GetUnit(int unitID)
    {
        for (int i = 0; i < databaseStruct.ownedUnits.Count; i++)
        {
            if (databaseStruct.ownedUnits[i].unitID == unitID)
            {
                return databaseStruct.ownedUnits[i];
            }
        }
        return new UnitInfo();
    }
    public static void RemoveSale(int unitID)
    {
        for (int i = 0; i < databaseStruct.unitSaleInfos.Count; i++)
        {
            if (databaseStruct.unitSaleInfos[i].playerAccount == databaseStruct.playerAccount)
            {
                for (int k = 0; k < databaseStruct.unitSaleInfos[i].unitInfo.Count; k++)
                {
                    if (databaseStruct.unitSaleInfos[i].unitInfo[k].unitID == unitID)
                    {
                        databaseStruct.unitSaleInfos[i].unitInfo.RemoveAt(k);
                        break;
                    }
                }
                break;
            }
        }
    }
    public static UnitInfo GetSaleUnit(int unitID)
    {
        for (int i = 0; i < databaseStruct.unitSaleInfos.Count; i++)
        {
            if (databaseStruct.unitSaleInfos[i].playerAccount == databaseStruct.playerAccount)
            {
                for (int k = 0; k < databaseStruct.unitSaleInfos[i].unitInfo.Count; k++)
                {
                    if (databaseStruct.unitSaleInfos[i].unitInfo[k].unitID == unitID)
                    {
                        return databaseStruct.unitSaleInfos[i].unitInfo[k];
                    }
                }
                break;
            }
        }
        return new UnitInfo();
    }*/
    public static int GetUnitFromID(int unitID)
    {
        for (int i = 0; i < databaseStruct.ownedUnits.Count; i++)
        {
            if (databaseStruct.ownedUnits[i].unitID == unitID)
            {
                return i;
            }
        }
        return -1;
    }
    public static int GetUnitFromVisualIndex(int visualIndex)
    {
        for (int i = 0; i < databaseStruct.visualIndexes.Count; i++)
        {
            if (databaseStruct.visualIndexes[i].visualIndex == visualIndex)
            {
                return databaseStruct.visualIndexes[i].unitID;
            }
        }
        return -1;
    }
    public static void SetPrice(int unitID, int price)
    {
        for (int i = 0; i < databaseStruct.ownedUnits.Count; i++)
        {
            if (databaseStruct.ownedUnits[i].unitID == unitID)
            {
                databaseStruct.ownedUnits[i] = new UnitInfo(databaseStruct.ownedUnits[i], databaseStruct.ownedUnits[i].health, price);
                break;
            }
        }
    }
    private void LoadDatabase()
    {
        databaseStruct = XMLGenerator.DeserializeObject(PlayerPrefs.GetString(databaseName), typeof(DatabaseStruct)) as DatabaseStruct;
    }
    public static void SaveDatabase()
    {
        PlayerPrefs.SetString(databaseName, XMLGenerator.SerializeObject(databaseStruct, typeof(DatabaseStruct)));
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Database))]
public class OfflineDatabaseUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Clear Database"))
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
#endif
