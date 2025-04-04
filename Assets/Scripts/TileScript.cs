using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class TileScript : MonoBehaviour
{
    public bool isRotatable = false;
    public int[] correctRotations = new int[] { 0 };

    private PuzzleFunctionBottom puzzleManager;
    private int currentRotation = 0;
    private bool isCorrect = false;

    public void Initialize(PuzzleFunctionBottom manager)
    {
        puzzleManager = manager;
        currentRotation = (int)transform.localEulerAngles.y;
        isCorrect = CheckIfCorrect();
    }

    void OnMouseDown()
    {
        if (!isRotatable || puzzleManager == null || puzzleManager.complete) return;

        RotateButton();
    }

    void RotateButton()
    {
        currentRotation = (currentRotation + 90) % 360;
        transform.localRotation = Quaternion.Euler(0, currentRotation, 0);

        bool newCorrectState = CheckIfCorrect();
        if (newCorrectState != isCorrect)
        {
            puzzleManager.ReportRotation(newCorrectState);
            isCorrect = newCorrectState;
        }
    }

    public bool CheckIfCorrect()
    {
        if (!isRotatable) return false;

        foreach (int angle in correctRotations)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(currentRotation, angle)) < 0.1f)
                return true;
        }
        return false;
    }
}
