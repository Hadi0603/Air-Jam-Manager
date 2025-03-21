using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject pauseBtn;
    [SerializeField] UIManager uiManager;
    public static int levelToLoad;
    private void Start()
    {
        levelToLoad = PlayerPrefs.GetInt("levelToLoad", 1);
    }
    public void Play()
    {
        SceneManager.LoadScene(levelToLoad);
    }
    public void Menu()
    {
        SceneManager.LoadScene(0);
    }
    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void Pause()
    {
        uiManager.OpenPauseMenu();
    }

    public void Resume()
    {
        uiManager.ClosePauseMenu();
    }

    public void Exit()
    {
        Application.Quit();
    }
    public void ClearBtn()
    {
        PlayerPrefs.SetInt("levelToLoad", 1);
    }
}
