using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;

public class EndGameManager : MonoBehaviour
{
    public GameObject endGameCanvas; // 指向你的 Canvas
    public Button restartButton;
    public Button quitButton;

    public float endgame_timer = 5f;
    public bool playing = true;
    public bool isEndscreenOn = false;

    void Start()
    {
        endGameCanvas.SetActive(false);

        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (!playing)
        {
            endgame_timer -= Time.deltaTime;
            if (endgame_timer < 0 && !isEndscreenOn)
            {
                endGameCanvas.SetActive(true);
                isEndscreenOn=true; 
            }
        }

        if (isEndscreenOn && XRDialogueInput.ConfirmPressed) {
            isEndscreenOn = false;
            endGameCanvas.SetActive(false);
            RestartGame();
        }
    }

    public void TriggerEndGame()
    {
        playing = false;
        Debug.Log("Endgame triggered");
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Game restarted");

    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game (Editor won't close)");
    }
}
