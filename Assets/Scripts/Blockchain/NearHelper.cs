using Anonym.Isometric;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public enum DataGetState
{
    Login = 0,
    MarketPurchase = 1,
    AfterFight = 2,
    AfterTrash = 3,
    AfterHire = 4,
    AfterNotification = 5
}

public class NearHelper : MonoBehaviour
{
    private string WalletUri = "https://wallet.testnet.near.org";
    private string ContractId = "pixeltoken.testnet";
    [HideInInspector]
    public string WalletSuffix = ".testnet";
    //private string BackendUri = "https://intern-testnet.pixeldapps.co/api";
    private string BackendUri = "https://pd-testnet.marmaj.org/api";
    public bool Testnet = true;
    private bool firstLogin = true;
    public MainMenuController mainMenuController;
    public MapController mapController;
    public NotificationController notificationController;
    public MarketController marketController;
    public LobbyController lobbyController;
    public OfficeController officeController;
    public RankController rankController;
    public TavernController tavernController;
    public LoginController loginController;
    [HideInInspector]
    public DataGetState dataGetState;

    [SerializeField] private GameObject validationFailedWindow;

    [DllImport("__Internal")]
    private static extern void WSInit(string callbackClass, string callbackMethod);
    [DllImport("__Internal")]
    private static extern void WSLogin(string contractId);
    [DllImport("__Internal")]
    public static extern void WSLogout();
    [DllImport("__Internal")]
    private static extern void AddKey(string publicKey, string contractId);

    private bool isSignedIn = false;

    private void Start()
    {
        if (!Testnet)
        {
            BackendUri = "https://pd.marmaj.org/api";
            WalletUri = "https://wallet.near.org";
            ContractId = "pixeltoken.near";
            WalletSuffix = ".near";
        }

#if UNITY_EDITOR
        Setup();
#else
        WSInit(this.gameObject.name, "WSInitFinished");
#endif
    }

    public void Setup()
    {
        validationFailedWindow.SetActive(false);
        loginController.Setup();

        if (BaseUtils.offlineMode)
        {
            loginController.OnReceiveLoginAuthorise();
            return;
        }
        if (Database.databaseStruct.playerAccount != null && Database.databaseStruct.privateKey != null && Database.databaseStruct.publicKey != null)
        {
            loginController.windowObj.SetActive(false);
            CheckForValidLogin();
        }
        else
        {
            Debug.Log("No login found, please login");
        }
    }

    public void WSInitFinished(string accountId)
    {
        isSignedIn = !string.IsNullOrEmpty(accountId);
        if (!string.IsNullOrEmpty(accountId)
            && (string.IsNullOrEmpty(Database.databaseStruct.playerAccount) || Database.databaseStruct.playerAccount != accountId))
        {
            if (accountId.Contains(WalletSuffix))
            {
                Database.databaseStruct.playerAccount = accountId;
            }
            else
            {
                Database.databaseStruct.playerAccount = accountId + WalletSuffix;
            }

            StartCoroutine(WebHelper.SendGet<ProxyAccessKeyResponse>(BackendUri + "/proxy/get-ed25519pair", LoginCallback));
        }
        else
        {
            if (string.IsNullOrEmpty(accountId))
            {
                Database.databaseStruct.playerAccount = null;
            }

            Setup();
        }
    }

    public void CheckForValidLogin()
    {
        StartCoroutine(CheckLoginValid(Database.databaseStruct.playerAccount, Database.databaseStruct.publicKey));
    }
    public IEnumerator CallViewMethod<T>(string methodName, string args, Action<T> callbackFunction)
    {
        var requestData = RequestParamsBuilder.CreateFunctionCallRequest(this.ContractId, methodName, args);
        var content = JsonUtility.ToJson(requestData);
        return WebHelper.SendPost<T>(BackendUri + "/proxy/call-view-function", content, callbackFunction);
    }

    public IEnumerator CallChangeMethod<T>(string methodName, string args, Action<T> callbackFunction, string accountId, string privatekey, bool attachYoctoNear, bool raise_gas = false)
    {
        var requestData = RequestParamsBuilder.CreateFunctionCallRequest(this.ContractId, methodName, args, accountId, privatekey, attachYoctoNear, raise_gas);
        var content = JsonUtility.ToJson(requestData);

        return WebHelper.SendPost<T>(BackendUri + "/proxy/call-change-function", content, callbackFunction);
    }

    public void WalletSelectorLogin()
    {
        if (!isSignedIn)
        {
            WSLogin(ContractId);
        } else
        {
            Setup();
        }
    }

    public void AddKeyFinished()
    {
        Setup();
    }

    public void Login(string account_id)
    {
        Database.databaseStruct.playerAccount = account_id + WalletSuffix;
        StartCoroutine(WebHelper.SendGet<ProxyAccessKeyResponse>(BackendUri + "/proxy/get-ed25519pair", LoginCallback));
    }

    private void LoginCallback(ProxyAccessKeyResponse res)
    {
        Database.databaseStruct.privateKey = res.privateKey;
        Database.databaseStruct.publicKey = res.publicKey;
        Database.SaveDatabase();

        StartCoroutine(AddKeyCoroutine());

        //Application.OpenURL(WalletUri + "/login/?success_url=https%3A%2F%2Fecosystem.pixeldapps.co%2Fcallback%3Fpage%3Dlogin_success&failure_url=https%3A%2F%2Fecosystem.pixeldapps.co%2Fcallback%3Fpage%3Dlogin_fail&public_key=" + res.publicKey + "&contract_id=" + ContractId);
        //Application.OpenURL(WalletUri + "/login/?referrer=CryptoHero%20Client&public_key=" + res.publicKey + "&contract_id=" + ContractId);
    }

    private IEnumerator AddKeyCoroutine()
    {
        BaseUtils.ShowLoading();

        yield return new WaitForSeconds(1f);

        AddKey(Database.databaseStruct.publicKey, ContractId);
    }

    public void Logout(bool fromPlayer = false)
    {
        Database.databaseStruct.playerAccount = null;
        Database.databaseStruct.publicKey = null;
        Database.databaseStruct.privateKey = null;
        if (!fromPlayer)
        {
            if (!firstLogin)
            {
                BaseUtils.ShowWarningMessage("Invalid Login", new string[1] { "Login has been invalidated, please try again." });
                firstLogin = false;
            }
            loginController.ResetLogin();
        }
    }
    private IEnumerator CheckLoginValid(string accountId, string publickey)
    {
        BaseUtils.ShowLoading();
        var requestData = RequestParamsBuilder.CreateAccessKeyCheckRequest(accountId, publickey);
        var content = JsonUtility.ToJson(requestData);

        return WebHelper.SendPost<BackendResponse<ProxyCheckAccessKeyResponse>>(BackendUri + "/chainteamtactics/is-valid-login", content, CheckLoginValidCallback);
    }

    private void CheckLoginValidCallback(BackendResponse<ProxyCheckAccessKeyResponse> res)
    {

        //Debug.Log(res.data.allowance);
        //Debug.Log("Is player registered: " + res.data.player_registered);
        //Debug.Log("Is key valid: " + res.data.valid);

        BaseUtils.HideLoading();
        if (!res.success)
        {
            BaseUtils.ShowWarningMessage("Error on login", new string[2] { "Login was not possible!", "please check if your wallet has enough balance to cover transaction fees." });
            loginController.Setup();

            return;
        }
        if (!res.data.valid)
        {

            //BaseUtils.ShowWarningMessage("Error on login", new string[2] { "Login was not possible!", "Data was not valid." });
            //Logout();
            //Database.databaseStruct.validCredentials = false;
            //loginController.Setup();

            validationFailedWindow.SetActive(true);

            return;
        }

        string cropAllowance = "0";
        try
        {
            cropAllowance = res.data.allowance.Substring(0, 4);
        } catch (Exception ex)
        {
            Debug.LogError("Error while parsing allowance string\n" + ex.Message);
        }

        float.TryParse(cropAllowance, out float allowanceFloat);
        Database.databaseStruct.allowance = allowanceFloat;
        if (!res.data.player_registered)
        {
            StartCoroutine(RegisterPlayerCoroutine());
        }
        else
        {
            Database.databaseStruct.validCredentials = true;
            dataGetState = DataGetState.Login;
            StartCoroutine(GetPlayerData());
        }
    }

    public IEnumerator RegisterPlayerCoroutine()
    {
        BaseUtils.ShowLoading();
        return CallChangeMethod<BackendResponse<object>>("ctt_register_player", null, GetPlayerDataCoroutine, Database.databaseStruct.playerAccount, Database.databaseStruct.privateKey, false);
    }

    public void GetPlayerDataCoroutine(BackendResponse<object> res)
    {
        BaseUtils.HideLoading();
        if (res.success)
        {
            dataGetState = DataGetState.Login;
            StartCoroutine(GetPlayerData());
        }
        else
        {
            loginController.Setup();
            BaseUtils.ShowWarningMessage("Error on login", new string[2] { "Login was not possible!", "please check if your wallet has enough balance to cover transaction fees." });
        }
    }

    public IEnumerator GetPlayerData(bool skipLoadingScreen = false)
    {
        if (!skipLoadingScreen)
        {
            BaseUtils.ShowLoading();
        }
        var d = new PlayerDataRequest
        {
            account_id = Database.databaseStruct.playerAccount
        };
        var dString = JsonUtility.ToJson(d);

        return WebHelper.SendPost<BackendResponse<PlayerDataResponse>>(BackendUri + "/chainteamtactics/get-playerdata", dString, GetPlayerDataCallback);
    }

    public void GetPlayerDataCallback(BackendResponse<PlayerDataResponse> res)
    {
        BaseUtils.HideLoading();
        if (!res.success)
        {
            SendErrorMessage(res.error);
            return;
        }
        if (res.data.maintenance)
        {
            mainMenuController.SetMaintenanceMode();
            return;
        }
        ChainTeamTactics.FillDatabaseFromPlayerResponse(res.data);
        switch (dataGetState)
        {
            case DataGetState.Login:
                loginController.OnReceiveLoginAuthorise();
                break;
            case DataGetState.MarketPurchase:
                marketController.OnReceiveNewPlayerData();
                break;
            case DataGetState.AfterFight:
                mainMenuController.Setup();
                break;
            case DataGetState.AfterTrash:
                tavernController.OnReceiveNewPlayerData();
                break;
            case DataGetState.AfterHire:
                officeController.OnReceiveNewPlayerData();
                break;
            case DataGetState.AfterNotification:
                notificationController.OnReceiveNewPlayerData();
                break;
        }
        dataGetState = DataGetState.Login;
    }
    public IEnumerator RequestSellUnit(int creatureID, long price)
    {
        BaseUtils.ShowLoading();
        var d = new OfferSellAuth
        {
            account_id = Database.databaseStruct.playerAccount,
            privatekey = Database.databaseStruct.privateKey,
            unitdata = new OfferSellWrapper()
            {
                token_id = creatureID.ToString(),
                price = (price * 1000000).ToString()
            }
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<object>>(BackendUri + "/chainteamtactics/marketplace/offer-unit", dString, SellUnitCallback);
    }
    public void SellUnitCallback(BackendResponse<object> res)
    {
        RemoveAllowanceAndCheck();
        BaseUtils.HideLoading();
        if (res.success)
        {
            tavernController.OnAcceptUnitSell();
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator RequestMarketData(UnitType unitType, int unitTier = 4, int minStat = 0)
    {
        BaseUtils.ShowLoading();
        var d = new MarketRequest
        {
            account_id = Database.databaseStruct.playerAccount,
            unitdata = new MarketRequestUnit()
            {
                unit_type = (int)unitType,
                tier_type = unitTier,
                minStat = minStat
            }
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<List<MarketWrapper>>>(BackendUri + "/chainteamtactics/marketplace/advanced-search", dString, MarketDataCallback);
    }
    public void MarketDataCallback(BackendResponse<List<MarketWrapper>> res)
    {
        BaseUtils.HideLoading();
        if (res.success)
        {
            marketController.OnReceiveUnits(res.data);
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator RequestCancelUnitSell(int unitID)
    {
        BaseUtils.ShowLoading();
        var d = new OfferAuth
        {
            account_id = Database.databaseStruct.playerAccount,
            privatekey = Database.databaseStruct.privateKey,
            unitdata = new OfferWrapper
            {
                token_id = unitID.ToString()
            }
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<object>>(BackendUri + "/chainteamtactics/marketplace/cancel-offer-unit", dString, CancelUnitSellCallback);
    }
    public void CancelUnitSellCallback(BackendResponse<object> res)
    {
        RemoveAllowanceAndCheck();
        BaseUtils.HideLoading();
        if (res.success)
        {
            tavernController.OnCancelUnitSell();
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator RequestBuyUnit(int unitID)
    {
        BaseUtils.ShowLoading();
        var d = new OfferAuth
        {
            account_id = Database.databaseStruct.playerAccount,
            privatekey = Database.databaseStruct.privateKey,
            unitdata = new OfferWrapper
            {
                token_id = unitID.ToString()
            }
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<string>>(BackendUri + "/chainteamtactics/marketplace/buy-unit", dString, BuyUnitCallback);
    }
    public void BuyUnitCallback(BackendResponse<string> res)
    {
        RemoveAllowanceAndCheck();
        BaseUtils.HideLoading();
        if (res.success)
        {
            Application.OpenURL(res.data);
            marketController.ShowBuyAuthorize();
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator RequestHireUnit()
    {
        BaseUtils.ShowLoading();
        var d = new PlayerDataRequest
        {
            account_id = Database.databaseStruct.playerAccount
        };
        var dString = JsonUtility.ToJson(d);
        return CallChangeMethod<BackendResponse<UnitToken>>("ctt_hire_random_unit", dString, HireUnitCallback, Database.databaseStruct.playerAccount, Database.databaseStruct.privateKey, false, true);
    }
    public void HireUnitCallback(BackendResponse<UnitToken> res)
    {
        RemoveAllowanceAndCheck();
        BaseUtils.HideLoading();
        if (res.success)
        {
            officeController.OnHireCallBack(res.data);
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public void RemoveAllowanceAndCheck()
    {
        Database.databaseStruct.allowance -= 50;
        if (Database.databaseStruct.allowance <= 0)
        {
            BaseUtils.ShowWarningMessage("Not enough allowance", new string[3] { "Your allowance has ran out", "to continue operating smoothly, you need to re-login.", "Press Ok to logout and then login again" }, OnAcceptRelogin);
        }
    }
    public IEnumerator RequestTrashUnit(int unitID)
    {
        BaseUtils.ShowLoading();
        var d = new OfferWrapper
        {
            token_id = unitID.ToString()
        };
        var dString = JsonUtility.ToJson(d);

        return CallChangeMethod<BackendResponse<object>>("ctt_fire_unit", dString, TrashUnitCallback, Database.databaseStruct.playerAccount, Database.databaseStruct.privateKey, false);
    }
    public void TrashUnitCallback(BackendResponse<object> res)
    {
        RemoveAllowanceAndCheck();
        BaseUtils.HideLoading();
        if (res.success)
        {
            dataGetState = DataGetState.AfterTrash;
            StartCoroutine(GetPlayerData());
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator RequestLobbyData()
    {
        BaseUtils.ShowLoading();
        var d = new GetRoomRequest
        {
            account_id = Database.databaseStruct.playerAccount,
            privatekey = Database.databaseStruct.privateKey,
            publickey = Database.databaseStruct.publicKey
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<List<LobbyRoomInfo>>>(BackendUri + "/chainteamtactics/challenge/get-all-rooms", dString, LobbyDataCallBack);
    }
    public void LobbyDataCallBack(BackendResponse<List<LobbyRoomInfo>> res)
    {
        BaseUtils.HideLoading();
        if (res.success)
        {
            lobbyController.OnReceiveLobbyRoomData(res.data);
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator RequestRankData()
    {
        BaseUtils.ShowLoading();
        var d = new PlayerDataRequest
        {
            account_id = Database.databaseStruct.playerAccount
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<List<RankWrapper>>>(BackendUri + "/chainteamtactics/get-leaderboard", dString, RankDataCallBack);
    }
    public void RankDataCallBack(BackendResponse<List<RankWrapper>> res)
    {
        BaseUtils.HideLoading();
        if (res.success)
        {
            rankController.OnReceiveRankData(res.data);
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator RequestRoomInfo(string roomOwner)
    {
        BaseUtils.ShowLoading();
        var d = new JoinRoomRequest
        {
            account_id = Database.databaseStruct.playerAccount,
            privatekey = Database.databaseStruct.privateKey,
            publickey = Database.databaseStruct.publicKey,
            leader_id = roomOwner
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<FullRoomInfo>>(BackendUri + "/chainteamtactics/challenge/get-room-info", dString, RoomInfoCallback);
    }
    public void RoomInfoCallback(BackendResponse<FullRoomInfo> res)
    {
        BaseUtils.HideLoading();
        if (res.success)
        {

        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator RequestJoinRoom(string roomOwner)
    {
        BaseUtils.ShowLoading();
        var d = new JoinRoomRequest
        {
            account_id = Database.databaseStruct.playerAccount,
            privatekey = Database.databaseStruct.privateKey,
            publickey = Database.databaseStruct.publicKey,
            leader_id = roomOwner
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<FullRoomInfo>>(BackendUri + "/chainteamtactics/challenge/join-room", dString, JoinRoomCallback);
    }
    public void JoinRoomCallback(BackendResponse<FullRoomInfo> res)
    {
        BaseUtils.HideLoading();
        if (res.success)
        {
            lobbyController.OnJoinRoom(res.data, "");
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator RequestForfeitRoom()
    {
        BaseUtils.ShowLoading();
        var d = new ForfeitRoomRequest
        {
            account_id = Database.databaseStruct.playerAccount,
            privatekey = Database.databaseStruct.privateKey,
            publickey = Database.databaseStruct.publicKey
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<object>>(BackendUri + "/chainteamtactics/challenge/notify-room", dString, ForfeitRoomCallback);
    }
    public void ForfeitRoomCallback(BackendResponse<object> res)
    {
        BaseUtils.HideLoading();
        if (res.success)
        {
            notificationController.ShowRewards(false);
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator RequestRevokeRoom()
    {
        BaseUtils.ShowLoading();
        var d = new ForfeitRoomRequest
        {
            account_id = Database.databaseStruct.playerAccount,
            privatekey = Database.databaseStruct.privateKey,
            publickey = Database.databaseStruct.publicKey
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<object>>(BackendUri + "/chainteamtactics/challenge/revoke-room", dString, RevokeRoomCallback);
    }
    public void RevokeRoomCallback(BackendResponse<object> res)
    {
        BaseUtils.HideLoading();
        if (res.success)
        {
            dataGetState = DataGetState.AfterNotification;
            StartCoroutine(GetPlayerData());
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator RequestCreateRoom(int betType)
    {
        BaseUtils.ShowLoading();
        var d = new CreateRoomRequest
        {
            account_id = Database.databaseStruct.playerAccount,
            privatekey = Database.databaseStruct.privateKey,
            publickey = Database.databaseStruct.publicKey,
            bet_type = betType
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<int>>(BackendUri + "/chainteamtactics/challenge/begin-create-room", dString, CreateRoomCallBack);
    }
    public void CreateRoomCallBack(BackendResponse<int> res)
    {
        BaseUtils.HideLoading();
        if (res.success)
        {
            mainMenuController.OnReceiveMapInfo(new MapInfo(res.data));
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator EndCreateRoom(PartialLoadoutData[] playerLoadout)
    {
        BaseUtils.ShowLoading();
        var d = new EndCreateRoomRequest
        {
            account_id = Database.databaseStruct.playerAccount,
            privatekey = Database.databaseStruct.privateKey,
            publickey = Database.databaseStruct.publicKey,
            player_loadout = playerLoadout
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<FullRoomInfo>>(BackendUri + "/chainteamtactics/challenge/end-create-room", dString, EndCreateRoomCallBack);
    }
    public void EndCreateRoomCallBack(BackendResponse<FullRoomInfo> res)
    {
        BaseUtils.HideLoading();
        if (res.success)
        {
            mapController.OnSuccessCreateRoom();
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    public IEnumerator SimulateFight(PartialLoadoutData[] playerLoadout, string roomOwner)
    {
        BaseUtils.ShowLoading();
        var d = new SimulateFightRequest
        {
            account_id = Database.databaseStruct.playerAccount,
            privatekey = Database.databaseStruct.privateKey,
            publickey = Database.databaseStruct.publicKey,
            player_loadout = playerLoadout,
            leader_id = roomOwner
        };
        var dString = JsonUtility.ToJson(d);
        return WebHelper.SendPost<BackendResponse<FightStruct>>(BackendUri + "/chainteamtactics/challenge/simulate-fight", dString, SimulateFightCallBack);
    }
    public void SimulateFightCallBack(BackendResponse<FightStruct> res)
    {
        BaseUtils.HideLoading();
        if (res.success)
        {
            bool victory = (res.data.purpleWins && mapController.mapInfo.mapOwner != Database.databaseStruct.playerAccount) ||
                          (!res.data.purpleWins && mapController.mapInfo.mapOwner == Database.databaseStruct.playerAccount);
            bool wonLastRound = mapController.mapInfo.winner_id == Database.databaseStruct.playerAccount;
            bool finalFight = mapController.mapInfo.rounds == 2 || (mapController.mapInfo.rounds == 1 && ((victory && wonLastRound) || (!victory && !wonLastRound)));
            //Debug.Log(mapController.mapInfo.rounds + "," + mapController.mapInfo.winner_id + "," + finalFight);
            mapController.OnReceiveFightStruct(res.data, victory, finalFight, false, false);
        }
        else
        {
            SendErrorMessage(res.error);
        }
    }
    private void SendErrorMessage(string error)
    {
        switch (error)
        {
            case "LackBalanceForState":
                BaseUtils.ShowWarningMessage("Lack of balance", new string[2] { "Not enough funds to cover simple transactions", "please fill your wallet with some near." });
                break;
            case "NotEnoughAllowance":
                BaseUtils.ShowWarningMessage("Not enough allowance", new string[3] { "Your allowance has ran out", "to continue operating smoothly, you need to re-login.", "Press Ok to logout and then login again" }, OnAcceptRelogin);
                break;
            default:
                BaseUtils.ShowWarningMessage("Error on loading info", new string[2] { "There was an error with your connection", "if it persists, refresh your page." });
                break;
        }
    }
    private void OnAcceptRelogin()
    {
        mainMenuController.OnLogoutClick();
    }
}
