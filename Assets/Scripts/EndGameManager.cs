using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    public GameObject endGameCanvas; // 指向你的 Canvas
    public Button restartButton;
    public Button quitButton;

    void Start()
    {
        endGameCanvas.SetActive(false);  // 默认不显示

        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void TriggerEndGame()
    {
        endGameCanvas.SetActive(true);
        Time.timeScale = 0f; // 暂停游戏
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game (Editor won't close)");
    }
}
