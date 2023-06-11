using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lorder : MonoBehaviour
{
    private Game game;

    private void Awake()
    {
        game =FindObjectOfType<Game>();
    }
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickToGameSceneButtonEasy()
    {
        SceneManager.LoadScene("Easy");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameMode(Game.GameMode.Easy);
        }

    }
    public void OnClickToGameSceneButtonNormal()
    {
        SceneManager.LoadScene("Normal");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameMode(Game.GameMode.Normal);
        }

    }

    public void OnClickToGameSceneButtonHard()
    {
        SceneManager.LoadScene("Hard");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameMode(Game.GameMode.Hard);
        }

    }
}
