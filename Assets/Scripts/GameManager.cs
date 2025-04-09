using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public float instableTimeRemaining = 10f;

    public bool gameActive;
    public float gameTimer = 180;

    public TMP_Text timerText;
    public TMP_Text instableTimerText;

    public GameObject instableTimerObject;
    public TMP_Text instableTimerObjectSubheader;

    public GameObject gameOverScreen;

    public int instablePuzzles;
    public int puzzlesCompleted;
    public TMP_Text totalPuzzlesLabel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameActive = true;
        Time.timeScale = 1;

        instablePuzzles = 0;
        puzzlesCompleted = 0;
        instableTimeRemaining = 10f;
        instableTimerObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameTimer > 0 && gameActive)
        {
            gameTimer -= Time.deltaTime;
            updateTimer(gameTimer);

            if (gameTimer <= 10)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
        else
        {
            endGame();
        }

        if (instablePuzzles > 0)
        {
            instableTimerObject.SetActive(true);

            if (instableTimeRemaining > 0)
            {
                instableTimeRemaining -= Time.deltaTime;
                UpdateInstableTimer();
            }
            else
            {
                endGame();
            }
        }
        else
        {
            instableTimerObject.SetActive(false);
            instableTimeRemaining = 10f;
        }
        if (instablePuzzles == 1)
        {
            instableTimerObjectSubheader.text = $"RESET <b>[1]</b> MODULE IN";
        }
        else if (instablePuzzles > 1)
        {
            instableTimerObjectSubheader.text = $"RESET <b>[{instablePuzzles.ToString()}]</b> MODULES IN";
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }

    }

    void updateTimer(float currentTime)
    {
        currentTime += 1;

        float mins = Mathf.FloorToInt(currentTime / 60);
        float secs = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:0}:{1:00}", mins, secs);
    }

    void UpdateInstableTimer()
    {
        instableTimerText.text = instableTimeRemaining.ToString("F1");
    }

    void endGame()
    {
        gameTimer = 0;
        instableTimeRemaining = 0;
        gameActive = false;
        gameOverScreen.SetActive(true);
        totalPuzzlesLabel.text = puzzlesCompleted.ToString();
        Time.timeScale = 0;
    }
}
