using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
public class ConnectionManager : MonoBehaviour
{
    private Coroutine connectionMonitorCoroutine;
    private bool wasConnected = true;
    public NotificationManager notificationManager;
    void Start()
    {
        connectionMonitorCoroutine = StartCoroutine(CheckConnectionLoop());
    }

    private IEnumerator CheckConnectionLoop()
    {
        while (true)
        {
            bool isConnected = Application.internetReachability != NetworkReachability.NotReachable;
            if (!isConnected && wasConnected)
            {
                wasConnected = false;
                Debug.LogWarning("Internet connection lost!");
                notificationManager.Notify("Connection lost. Please check your internet.");
            
            
            }
            else if (isConnected && !wasConnected)
            {
                wasConnected = true;
                Debug.Log("Internet connection restored.");
                notificationManager.Notify("Connection restored.");
            }
            yield return new WaitForSeconds(3f);
        }
    }
}