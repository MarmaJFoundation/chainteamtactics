using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.Http;

public class KeyPair
{
    public string publicKey;
    public string privateKey;
}
[System.Serializable]
public class RequestParamsBuilder
{
    static public RequestParams CreateFunctionCallRequest(string contract_id, string methodName, string args, string accountId = null, string privatekey = null, bool attachYoctoNear = false, bool raise_gas = false)
    {
        return new RequestParams
        {
            contract_id = contract_id,
            account_id = accountId,
            method_name = methodName,
            args = args,
            privatekey = privatekey,
            attachYoctoNear = attachYoctoNear,
            raise_gas = raise_gas,
        };

    }
    static public RequestParams CreateFunctionViewRequest<T>(string contract_id, string methodName, string args)
    {
        return new RequestParams
        {
            contract_id = contract_id,
            method_name = methodName,
            args = args,
        };

    }
    static public RequestParams CreateAccessKeyCheckRequest(string accountId, string publickey)
    {
        return new RequestParams
        {
            account_id = accountId,
            publickey = publickey,
        };

    }
}

[Serializable]
public class WalletAuth
{
    public string account_id;
    public string privatekey;
}
[Serializable]
public class OfferAuth : WalletAuth
{
    public string token_id;
    public string price;
    public OfferWrapper unitdata;
}
[Serializable]
public class OfferWrapper
{
    public string token_id;
}
[Serializable]
public class OfferSellAuth : WalletAuth
{
    public string token_id;
    public string price;
    public OfferSellWrapper unitdata;
}
[Serializable]
public class OfferSellWrapper
{
    public string token_id;
    public string price;
}
[Serializable]
public class RequestParams
{
    public string account_id;
    public string contract_id;
    public string method_name;
    public string args;
    public string privatekey;
    public string publickey;
    public bool attachYoctoNear;
    public bool raise_gas;
}
[Serializable]
public class ProxyAccessKeyResponse
{
    public string privateKey;
    public string publicKey;
}
[Serializable]
public class ProxyCheckAccessKeyResponse
{
    public bool valid;
    public string allowance;
    public bool fullAccess;
    public bool player_registered;
}
[Serializable]
public class PlayerDataRequest
{
    public string account_id;
}
[Serializable]
public class RankWrapper
{
    public string account_id;
    //public int won;
    //public int lost;
    public int rank;
    public int position;
    public List<LobbyLoadoutData> loadout;
}
public class TimestampHelper
{
    public static DateTime GetDateTimeFromTimestamp(long timestamp)
    {
        if (timestamp == 0)
        {
            return DateTime.Now;
        }
        DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddMilliseconds(timestamp).ToLocalTime();
        return dtDateTime;
    }
}
[Serializable]
public class BackendResponse<T>
{
    public bool success;
    public string error;
    public T data;
}
[Serializable]
public class PlayerDataResponse
{
    public PlayerBalance balance;
    public PlayerData playerdata;
    public List<UnitToken> units;
    public bool maintenance;
    public RoomNotificationData[] notifications;
}
[Serializable]
public class PlayerBalance
{
    public string pixeltoken;
    public long titan_timer;
    public int titan;
    public int common;
    public int rare;
    public int epic;
    public int legendary;
}
[Serializable]
public class PlayerData
{
    public int matches_won;
    public int matches_lost;
    public int rating;
    public int fight_balance;
}
[Serializable]
public class CharacterData
{
    public int class_type;
    public int experience;
    public int level;
    public long injured_timer;
    public List<int> inventory;
}
[Serializable]
public class UnitToken
{
    public string token_id;
    public int unit_type;
    public int tier_type;
    public int health;
    public int damage;
    public int speed;
    public string owner;
    public long price;
    public long cd;
}
[Serializable]
public class PlayerBattleRequest : WalletAuth
{
    public PlayerFightData playerdata;
}
[Serializable]
public class PlayerFightData
{
    public int difficulty;
    public int class_type;
    public string[] inventory;
}
[Serializable]
public class MarketRequest
{
    public string account_id;
    public MarketRequestUnit unitdata;
}
[Serializable]
public class MarketRequestUnit
{
    public int unit_type;
    public int tier_type;
    public int minStat;
}
[Serializable]
public class MarketWrapper
{
    public UnitToken unit_data;
}
[Serializable]
public class LobbyRoomInfo
{
    public string account_id;
    public int won;
    public int lost;
    public int rank;
    public int betType;
    public LobbyLoadoutData[] loadout;// porém com 'int unit_type' e sem 'Vector2Int position'
}
[Serializable]
public class LobbyLoadoutData
{
    public string token_id;
    public int unit_type;
    public Vector2Int position;
}
[Serializable]
public class FullRoomInfo
{
    public int mapIndex;
    public FullPlayerLoadoutData[] loadout;
}
[Serializable]
public class PartialLoadoutData
{
    public string token_id;
    public Vector2Int position;
}
[Serializable]
public class FullPlayerLoadoutData
{
    public string token_id;
    public int unit_type;
    public int health;
    public int damage;
    public int speed;
    public Vector2Int position;
}
[Serializable]
public class CreateRoomRequest
{
    public string account_id;
    public string publickey;
    public string privatekey;
    public int bet_type;
}
[Serializable]
public class JoinRoomRequest
{
    public string account_id;
    public string publickey;
    public string privatekey;
    public string leader_id;
}
[Serializable]
public class GetRoomRequest
{
    public string account_id;
    public string privatekey;
    public string publickey;
}
[Serializable]
public class ForfeitRoomRequest
{
    public string account_id;
    public string privatekey;
    public string publickey;
}
[Serializable]
public class EndCreateRoomRequest
{
    public string account_id;
    public string publickey;
    public string privatekey;
    public PartialLoadoutData[] player_loadout;
}
[Serializable]
public class SimulateFightRequest
{
    public string account_id;
    public string privatekey;
    public string publickey;
    public string leader_id;
    public PartialLoadoutData[] player_loadout;
}
[Serializable]
public enum RoomTierTypes
{
    Tier1 = 0,
    Tier2 = 1,
    Tier3 = 2,
}
[Serializable]
public enum RoomNotificationType
{
    PendingCreate = 0,
    PendingJoin = 1,
    RoundFinish = 2,
    FightFinish = 3,
    RevokedRoom = 4
}
[Serializable]
public class RoomNotificationData
{
    public RoomNotificationType notify_type;
    public string room_id;
    public int map_index;
    public FullPlayerLoadoutData[] loadout;
    public string[] target_ids;
    public string winner_id;
    public FightStruct fightStruct;
    public bool hasFightStruct;
    public int rounds;
    public int betType;
}