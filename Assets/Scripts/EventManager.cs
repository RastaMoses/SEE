using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    //Player
    public static event Action<string> OnActionMapChange;
    public static event Action OnEnterFishing;
    public static event Action OnExitFishing;

    public static void EnterFishingEvent() { OnEnterFishing?.Invoke(); Debug.Log("Enter Fishing Event"); }
    public static void ExitFishingEvent() { OnExitFishing?.Invoke(); Debug.Log("Exit Fishing Event"); }
    public static void ActionMapChangeEvent(string newActionMap) { OnActionMapChange?.Invoke(newActionMap); Debug.Log("Changed Action Map Event"); }
}
