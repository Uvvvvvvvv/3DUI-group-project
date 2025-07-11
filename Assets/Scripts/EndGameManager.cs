using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using TMPro;

public class EndGameManager : MonoBehaviour
{
    public GameObject endGameCanvas; // 指向你的 Canvas
    public Button restartButton;
    public Button quitButton;

    public float endgame_timer = 5f;
    public bool playing = true;
    public bool isEndscreenOn = false;
    public bool won = false;

    public TextMeshProUGUI EndGameText;

    void Start()
    {
        endGameCanvas.SetActive(false);

        //restartButton.onClick.AddListener(RestartGame);
        //quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (!playing)
        {
            endgame_timer -= Time.deltaTime;
            if (endgame_timer < 0 && !isEndscreenOn)
            {
                if (won)
                {
                    EndGameText.text = "Congratulations, the dragon is slain! \n The village of Drakenvale thank you for your great help. \n \n Press X to restart.";
                } else
                {
                    EndGameText.text = "You died! \n \n Press X to restart";
                }
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
