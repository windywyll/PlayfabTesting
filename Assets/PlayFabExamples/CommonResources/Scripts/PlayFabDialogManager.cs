using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayFabDialogManager: MonoBehaviour
{

    [Serializable]
    public class Window
    {
        public string Key;
        public GameObject Container;
    }

    public List<Window> Windows = new List<Window>();
    public delegate void WindowEvent(string key);
    private static event WindowEvent WindowEvents;

    void Awake()
    {
        WindowEvents += HandleWindowEvent;
    }

    private void HandleWindowEvent(string key)
    {
        foreach (var w in Windows)
        {
            w.Container.gameObject.SetActive(false);
        }
        var window = Windows.Find((w) => { return w.Key == key; });
        if (window != null)
        {
            window.Container.SetActive(true);
        }
    }

    public static void SendEvent(string key)
    {
        if (WindowEvents != null)
        {
            WindowEvents(key);
        }
    }

}
