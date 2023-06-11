using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Game.GameMode CurrentGameMode { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
            Instance = this;
            DontDestroyOnLoad(gameObject);
    }

    public static GameObject InstantiateCustom(GameObject original, Vector3 position, Quaternion rotation)
    {
        return Instantiate(original, position, rotation);
    }

    public void SetGameMode(Game.GameMode gameMode)
    {
        CurrentGameMode = gameMode;
    }
}

