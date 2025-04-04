using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public float instableTimeRemaining = 25f;

    public bool gameActive;
    public float gameTimer = 180;

    public TMP_Text timerText;
    public TMP_Text instableTimerText;

    public GameObject instableTimerObject;
    public TMP_Text instableTimerObjectSubheader;

    public int instablePuzzles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        gameActive = true;
        Time.timeScale = 1;

        instablePuzzles = 0;
        instableTimeRemaining = 25f;
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
            gameTimer = 0;
            gameActive = false;
            Time.timeScale = 0;
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
                instableTimeRemaining = 0;
                gameActive = false;

                // You can add any actions to perform when timer reaches zero here

                Debug.Log("Timer has finished!");
            }
        }
        else
        {
            instableTimerObject.SetActive(false);
            instableTimeRemaining = 25f;
        }
        if (instablePuzzles == 1)
        {
            instableTimerObjectSubheader.text = $"RESET [1] MODULE IN";
        }
        else if (instablePuzzles > 1)
        {
            instableTimerObjectSubheader.text = $"RESET [{instablePuzzles.ToString()}] MODULES IN";
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
}
