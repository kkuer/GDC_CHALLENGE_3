using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ButtonSlot
{
    public Transform slotTransform;
    [HideInInspector] public int assignedNumber;
}

public class PuzzleFunctionBack : MonoBehaviour
{
    public bool instable;
    public bool delayCheck;

    public static PuzzleFunctionBack instance { get; private set; }

    [Header("Settings")]
    public GameObject buttonPrefab;
    public ButtonSlot[] buttonSlots = new ButtonSlot[9];
    public float completionDelay = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool _complete;
    [SerializeField] private int _nextExpectedNumber;

    public bool complete { get => _complete; private set => _complete = value; }
    public int nextExpectedNumber { get => _nextExpectedNumber; private set => _nextExpectedNumber = value; }

    private Dictionary<int, ChangeNumber> buttonObjects = new Dictionary<int, ChangeNumber>();
    private Coroutine completionRoutine;
    private List<int> availableNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    public GameObject warningLight;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

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

    public void backFaceFunction()
    {
        warningLight.GetComponent<LightScript>().flashing = true;
        InitializeButtons();
        GameManager.instance.instablePuzzles++;
    }

    public void InitializeButtons()
    {
        ResetAll();
        AssignRandomNumbersToSlots();
        complete = false;
        nextExpectedNumber = 1;
    }

    private void AssignRandomNumbersToSlots()
    {
        // Create a shuffled copy of numbers 1-9
        List<int> shuffledNumbers = new List<int>(availableNumbers);
        ShuffleList(shuffledNumbers);

        // Assign numbers to slots
        for (int i = 0; i < buttonSlots.Length; i++)
        {
            if (buttonSlots[i].slotTransform == null)
            {
                Debug.LogError($"Slot {i} is not assigned!");
                continue;
            }

            int assignedNumber = shuffledNumbers[i];
            buttonSlots[i].assignedNumber = assignedNumber;
            CreateButton(assignedNumber, buttonSlots[i].slotTransform);
        }
    }

    private void CreateButton(int number, Transform slot)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, slot);
        ChangeNumber changeNumber = buttonObj.GetComponent<ChangeNumber>();
        changeNumber.updateNumber(number);

        // Ensure collider exists
        if (buttonObj.GetComponent<Collider>() == null)
        {
            buttonObj.AddComponent<BoxCollider>();
        }

        if (changeNumber == null)
        {
            changeNumber = buttonObj.AddComponent<ChangeNumber>();
        }

        changeNumber.Initialize(this, number);
        buttonObjects.Add(number, changeNumber);
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public void HandleButtonPress(ChangeNumber pressedButton)
    {
        if (pressedButton.buttonNumber == nextExpectedNumber)
        {
            // Correct button in sequence
            pressedButton.clicked = true;
            nextExpectedNumber++;
            CheckSequenceCompletion();
        }
        else
        {
            // Wrong button pressed - reset sequence
            ResetButtonStates();
        }
    }

    private void CheckSequenceCompletion()
    {
        if (nextExpectedNumber > 9) // All buttons pressed in order
        {
            completionRoutine = StartCoroutine(CompleteSequence());
        }
    }

    private IEnumerator CompleteSequence()
    {
        yield return new WaitForSeconds(completionDelay);

        complete = true;
        instable = false;
        GameManager.instance.instablePuzzles--;
        warningLight.GetComponent<LightScript>().flashing = false;
        Debug.Log("Sequence completed successfully!");

        // Auto-reset after completion
        ResetAll();
    }

    private void ResetButtonStates()
    {
        foreach (var button in buttonObjects.Values)
        {
            if (button != null)
            {
                button.clicked = false;
            }
        }
        nextExpectedNumber = 1;
    }

    public void ResetAll()
    {
        // Stop any running coroutines
        if (completionRoutine != null)
        {
            StopCoroutine(completionRoutine);
            completionRoutine = null;
        }

        // Destroy all buttons
        foreach (var button in buttonObjects.Values)
        {
            if (button != null && button.gameObject != null)
            {
                Destroy(button.gameObject);
            }
        }

        buttonObjects.Clear();
        complete = false;
        nextExpectedNumber = 1;
    }

    // Editor button for testing
    [ContextMenu("Reset And Initialize")]
    private void EditorInitialize()
    {
        InitializeButtons();
    }

    public IEnumerator randomInstability()
    {
        delayCheck = true;
        yield return new WaitForSeconds(Random.Range(10f, 20f));
        delayCheck = false;
        instable = true;
        backFaceFunction();
    }
}
