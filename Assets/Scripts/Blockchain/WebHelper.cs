using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

class WebHelper
{

    static public IEnumerator SendPost<T>(string url, string dataString, Action<T> onSuccess)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(dataString);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Debug.Log("Erro: " + request.error);
        }
        else
        {
            //Debug.Log("Status Code: " + request.responseCode);
            //Debug.Log(request.downloadHandler.text);
            //onSuccess(JsonUtility.FromJson<T>(request.downloadHandler.text));// JsonUtility = TRASH!!!
            onSuccess(JsonConvert.DeserializeObject<T>(request.downloadHandler.text));
        }
    }


    static public IEnumerator SendGet<T>(string url, Action<T> onSuccess)
    {
        var request = new UnityWebRequest(url, "GET");

        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Debug.Log("Erro: " + request.error);
        }
        else
        {
            //Debug.Log("Status Code: " + request.responseCode);
            //Debug.Log("Content: " + request.downloadHandler.text);
            //onSuccess(JsonUtility.FromJson<T>(request.downloadHandler.text));// JsonUtility = TRASH!!!
            onSuccess(JsonConvert.DeserializeObject<T>(request.downloadHandler.text));
        }
    }
}

