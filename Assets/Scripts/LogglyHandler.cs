using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class LogglyHandler : MonoBehaviour
{
    //Register the HandleLog function on scene start to fire on debug.log events
    void Start()
    {
        if (Debug.isDebugBuild && !Application.isEditor)
        {
            Application.logMessageReceived += HandleLog;
        }
    }

    //Create a string to store log level in
    string level = "";

    //Capture debug.log output, send logs to Loggly
    public void HandleLog(string logString, string stackTrace, LogType type)
    {
        //Initialize WWWForm and store log level as a string
        level = type.ToString();
        var loggingForm = new WWWForm();

        //Add log message to WWWForm
        loggingForm.AddField("LEVEL", level);
        loggingForm.AddField("Message", logString);
        loggingForm.AddField("Stack_Trace", stackTrace);

        //Add any User, Game, or Device MetaData that would be useful to finding issues later
        loggingForm.AddField("Device_Model", SystemInfo.deviceModel);
        StartCoroutine(SendData(loggingForm));
    }

    public IEnumerator SendData(WWWForm form)
    {
        var TOKEN = "99c7645f-4e2b-4194-b073-d99f53e0b75b";

        //Send WWW Form to Loggly, replace TOKEN with your unique ID from Loggly
        var sendLog = UnityWebRequest.Post($"https://logs-01.loggly.com/inputs/{TOKEN}/tag/Unity3D", form);

        yield return sendLog.SendWebRequest();

        if (sendLog.isNetworkError || sendLog.isHttpError)
        {
            Debug.Log(sendLog.uri);
            Debug.Log(sendLog.error);
            Debug.Log(sendLog.downloadHandler.text);
        }
    }
}
