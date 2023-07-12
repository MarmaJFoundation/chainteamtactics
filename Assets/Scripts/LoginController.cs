using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    public NearHelper nearhelper;
    public MainMenuController mainMenuController;
    public CustomInput customInput;
    public CustomText nearText;
    public GameObject loginButtonObj;
    public GameObject authButtonObj;
    public GameObject introObj;
    public Image introImage;
    public GameObject windowObj;
    public EventSystem eventSystem;
    public Canvas loginCanvas;
    private string playerAccountName;
    public void Setup()
    {
        loginCanvas.enabled = true;
        loginCanvas.gameObject.SetActive(true);
        windowObj.SetActive(true);
        customInput.Setup();
        customInput.ResetInput();
        customInput.OnPointerClick(null);
        eventSystem.SetSelectedGameObject(customInput.gameObject);
        nearText.SetString(nearhelper.Testnet ? ".testnet" : ".near");
    }
    public void ResetLogin()
    {
        Setup();
        customInput.enabled = true;
        customInput.ResetInput();
        customInput.OnPointerClick(null);
        eventSystem.SetSelectedGameObject(customInput.gameObject);
        authButtonObj.SetActive(false);
        loginButtonObj.SetActive(true);
    }
    //#protocol
    public void OnLoginClick()
    {
#if UNITY_EDITOR
        /*
        if (customInput.typeString == "")
        {
            BaseUtils.ShowWarningMessage("Error!", new string[2] { "Address cannot be empty.", "Enter your Near Wallet adress." });
            return;
        }
        if (customInput.typeString.Length >= 64)
        {
            BaseUtils.ShowWarningMessage("Error!", new string[2] { "Address not supported. Contact support", "If you wish to use non near accounts." });
            return;
        }
        playerAccountName = customInput.typeString.ToLower();
        if (playerAccountName.Contains(".near"))
        {
            playerAccountName = playerAccountName.Remove(playerAccountName.IndexOf('.'), 5);
        }
        */

        playerAccountName = "2dcity-villager1";

        customInput.enabled = false;
        if (BaseUtils.offlineMode)
        {
            BaseUtils.ShowLoading();
        }
        else
        {
            nearhelper.Login(playerAccountName);
        }
        StartCoroutine(WaitAndShowAuthButton());
#else
        nearhelper.WalletSelectorLogin();
#endif
    }
    private IEnumerator WaitAndShowAuthButton()
    {
        loginButtonObj.SetActive(false);
        BaseUtils.ShowLoading();
        yield return new WaitForSeconds(5);
        BaseUtils.HideLoading();
        authButtonObj.SetActive(true);
    }
    //player clicked the authorized button after the login
    public void OnAuthClick()
    {
        windowObj.SetActive(false);
        if (BaseUtils.offlineMode)
        {
            OnReceiveLoginAuthorise();
        }
        else
        {
            nearhelper.CheckForValidLogin();
        }
    }
    public void OnLoginError(int errorType)
    {
        switch (errorType)
        {
            //not authorized in near page
            case 0:
                BaseUtils.ShowWarningMessage("Error!", new string[2] { "Login info was not authorized.", "Please try again!" });
                break;
            //wrong account name
            case 1:
                BaseUtils.ShowWarningMessage("Error!", new string[2] { "Wallet adress is not correct.", "Please try again!" });
                break;
            //uknown/server error
            case 2:
                BaseUtils.ShowWarningMessage("Error!", new string[2] { "The servers are acting weird!", "Please try again later." });
                break;
        }
        BaseUtils.HideLoading();
        loginButtonObj.SetActive(true);
        authButtonObj.SetActive(false);
        customInput.enabled = true;
        customInput.ResetInput();
        customInput.OnPointerClick(null);
        eventSystem.SetSelectedGameObject(customInput.gameObject);
    }
    public void OnReceiveLoginAuthorise()
    {
        loginCanvas.enabled = false;
        loginCanvas.gameObject.SetActive(false);
        BaseUtils.HideLoading();
        if (BaseUtils.offlineMode)
        {
            Database.databaseStruct.playerAccount = "anon.near";
            Database.databaseStruct.pixelTokens = 6500;
        }
        if (!Database.databaseStruct.firstLogin)
        {
            Database.databaseStruct.firstLogin = true;
            StartCoroutine(IntroCoroutine());
        }
        else
        {
            mainMenuController.Setup();
        }

    }
    private IEnumerator IntroCoroutine()
    {
        //mainMenuController.fadeScreen.enabled = true;
        //mainMenuController.fadeScreen.color = Color.black;
        introObj.SetActive(true);
        float timer = 0;
        while (timer <= 1)
        {
            foreach (Image image in introObj.GetComponentsInChildren<Image>())
            {
                image.color = Color.Lerp(Color.clear, Color.white, timer.Evaluate(CurveType.EaseOut));
            }
            timer += Time.deltaTime * 4f;
            yield return null;
        }
        yield return new WaitForSeconds(2);
        mainMenuController.Setup();
        yield return new WaitForSeconds(2);
        timer = 0;
        while (timer <= 1)
        {
            foreach (Image image in introObj.GetComponentsInChildren<Image>())
            {
                image.color = Color.Lerp(Color.white, Color.clear, timer.Evaluate(CurveType.EaseIn));
            }
            timer += Time.deltaTime * 4f;
            yield return null;
        }
        introObj.SetActive(false);
        yield return new WaitForSeconds(.1f);
        BaseUtils.ShowWarningMessage("Welcome to Chain Team Tactics!", new string[5] {
            "Here are some useful tips for you before you start playing Chain Team Tactics:",
            "1. Look for good units in the marketplace, you need a minimum of 6 units",
            "2. Avoid buying more than 3 support units, as thats the maximum amount you can use in a fight",
            "3. Read what each unit does, their behaviours are essential to winning fights",
            "4. For more info, join our discord to meet more players who can help you with the game!"});
    }
}
