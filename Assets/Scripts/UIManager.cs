using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text timerText;
    [SerializeField] public float gameTime = 60f;
    [SerializeField] private CanvasGroup levelWonUI;
    [SerializeField] private GameObject winPanel;
    [FormerlySerializedAs("LevelLostUI")] [SerializeField] private CanvasGroup levelLostUI;
    [SerializeField] private GameObject lostPanel;
    [SerializeField] private CanvasGroup pauseUI;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseBtn;
    [FormerlySerializedAs("planeMover")] [SerializeField] private GameObject planeController;
    private bool isBlinking = false;
    private bool isPaused = false;
    
    public void Awake()
    {
        Time.timeScale = 1f;
        StartCoroutine(TimerCountdown());
    }
    
    private IEnumerator TimerCountdown()
    {
        while (gameTime > 0)
        {
            if (!isPaused) // Only update the timer if the game is not paused
            {
                gameTime -= Time.deltaTime;
                UpdateTimerDisplay();

                if (gameTime <= 15f && !isBlinking)
                {
                    StartCoroutine(BlinkTimer());
                }
            }
            yield return null;
        }

        GameOver();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(gameTime / 60);
        int seconds = Mathf.FloorToInt(gameTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private IEnumerator BlinkTimer()
    {
        isBlinking = true;
        Color originalColor = timerText.color;
    
        while (gameTime <= 15f && gameTime > 0)
        {
            timerText.color = (timerText.color == Color.red) ? Color.white : Color.red;
            yield return new WaitForSeconds(0.5f);
        }

        timerText.color = originalColor;
        isBlinking = false;
    }
    public void TriggerGameWon()
    {
        levelWonUI.gameObject.SetActive(true);
        StopCoroutine(TimerCountdown());
        Destroy(timerText);
        if (GameManager.levelToLoad < 5)
        {
            PlayerPrefs.SetInt("levelToLoad", ++GameManager.levelToLoad);
        }
        PlayerPrefs.Save();
    }
    public void GameOver()
    {
        Debug.Log("Time's up! Game Over.");
        planeController.SetActive(false);
        levelLostUI.gameObject.SetActive(true);
        timerText.enabled = false;
    }
    public void OpenPauseMenu()
    {
        pauseUI.gameObject.SetActive(true);
        pauseBtn.SetActive(false);
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ClosePauseMenu()
    {
        Time.timeScale = 1f;
        pauseUI.gameObject.SetActive(false);
        pauseBtn.SetActive(true);
        isPaused = false;
    }
}
