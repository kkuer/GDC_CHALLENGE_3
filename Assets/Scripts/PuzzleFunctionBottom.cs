using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PuzzleFunctionBottom : MonoBehaviour
{
    public List<GameObject> panelPrefabs = new List<GameObject>();
    public Transform bottomTransform;
    [HideInInspector] public bool complete = false;

    public bool instable;
    public bool delayCheck;
    public GameObject warningLight;

    private GameObject currentPanel;
    private TileScript[] allButtons;
    private int correctRotations = 0;
    private int totalCorrectNeeded = 0;

    void Start()
    {
        delayCheck = false;
        instable = false;
    }
    private void Update()
    {
        if (!delayCheck && !instable)
        {
            StartCoroutine(randomInstability());
        }
    }

    void InstantiatePanel()
    {
        // Clear previous puzzle if exists
        if (currentPanel != null)
        {
            Destroy(currentPanel);
            complete = false;
            correctRotations = 0;
            totalCorrectNeeded = 0;
        }

        currentPanel = Instantiate(panelPrefabs[Random.Range(0, panelPrefabs.Count)], bottomTransform);
        StartCoroutine(InitializePuzzle());
    }

    IEnumerator InitializePuzzle()
    {
        // Wait one frame to ensure all components are ready
        yield return null;

        // Find all buttons in the instantiated panel
        allButtons = currentPanel.GetComponentsInChildren<TileScript>();

        // Count rotatable buttons and set up references
        totalCorrectNeeded = 0;
        correctRotations = 0; // Reset counter
        foreach (TileScript button in allButtons)
        {
            if (button.isRotatable)
            {
                totalCorrectNeeded++;
                // Initialize button's starting state
                button.Initialize(this);

                // If button starts in correct rotation, count it
                if (button.CheckIfCorrect())
                {
                    correctRotations++;
                }
                    
            }
        }

        Debug.Log($"Puzzle ready: {totalCorrectNeeded} rotatable buttons, {correctRotations} already correct");
    }

    public void ReportRotation(bool isNowCorrect)
    {
        if (complete) return;

        correctRotations += isNowCorrect ? 1 : -1;

        Debug.Log($"Correct rotations: {correctRotations}/{totalCorrectNeeded}");

        if (correctRotations >= totalCorrectNeeded)
            StartCoroutine(PuzzleComplete());
    }

    IEnumerator PuzzleComplete()
    {
        Debug.Log("Puzzle complete!");
        yield return new WaitForSeconds(1f);
        complete = true;
        instable = false;
        GameManager.instance.instablePuzzles--;
        warningLight.GetComponent<LightScript>().flashing = false;

        //delete puzzle
        if (currentPanel != null)
        {
            Destroy(currentPanel);
            currentPanel = null;
        }
    }
    public IEnumerator randomInstability()
    {
        delayCheck = true;
        float randomRange = Random.Range(10f, 20f);
        yield return new WaitForSeconds(randomRange);
        complete = false;
        delayCheck = false;
        instable = true;
        InstantiatePanel();
        warningLight.GetComponent<LightScript>().flashing = true;
        GameManager.instance.instablePuzzles++;
        GameManager.instance.instableTimeRemaining += 10;
    }
}
