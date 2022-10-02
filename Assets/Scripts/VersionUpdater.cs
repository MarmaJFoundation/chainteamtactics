using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionUpdater : MonoBehaviour
{
    public NearHelper nearHelper;
    public CustomText customText;
    private string lastName = "";
    private void Update()
    {
        if (lastName != Application.version)
        {
            if (nearHelper.Testnet)
            {
                customText.SetString($"testnet version: {Application.version}");
            }
            else
            {
                customText.SetString($"mainnet Beta v. {Application.version}");
            }
            lastName = Application.version;
        }
    }
}
