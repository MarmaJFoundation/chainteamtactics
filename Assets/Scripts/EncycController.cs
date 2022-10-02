using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncycController : MonoBehaviour
{
    public GameObject encycWindow;
    public void Setup()
    {
        encycWindow.SetActive(true);
    }
    public void Dispose()
    {
        encycWindow.SetActive(false);
    }
}
