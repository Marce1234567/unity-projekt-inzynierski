
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject tutorialPanel;
    public TMP_Text tutorialText;

    [Header("Tutorial Settings")]
    public float movementThreshold = 0.1f;

    private int currentStep = 0;
    private Vector3 startingPosition;
    private bool tutorialActive = false;

    void Start()
    {
        startingPosition = Vector3.zero;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        StartTutorial();
    }

    void Update()
    {
        if (!tutorialActive)
            return;

        // Krok 1: poruszanie siê
        if (currentStep == 1)
        {
            CheckMovement();
        }
        // Krok 2: skakanie
        else if (currentStep == 2)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ShowStep(3);
            }
        }
        // Krok 3: bieganie
        else if (currentStep == 3)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                ShowStep(4);
            }
        }
        // Koniec samouczka
        else if (currentStep == 4)
        {
            tutorialActive = false;

            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
        }
    }

    public void StartTutorial()
    {
        tutorialActive = true;
        ShowStep(1);
    }

    void CheckMovement()
    {
        // Sprawdza, czy gracz nacisn¹³ klawisz ruchu
        bool moving =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.UpArrow) ||
            Input.GetKey(KeyCode.DownArrow) ||
            Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.RightArrow);

        if (moving)
        {
            ShowStep(2);
        }
    }

    void ShowStep(int step)
    {
        currentStep = step;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        if (tutorialText == null)
            return;

        switch (step)
        {
            case 1:
                tutorialText.text =
                    "U¿yj WASD lub strza³ek, aby siê poruszaæ";
                break;

            case 2:
                tutorialText.text =
                    "Naciœnij SPACJÊ, aby skoczyæ";
                break;

            case 3:
                tutorialText.text =
                    "Przytrzymaj LEFT SHIFT, aby biec";
                break;

            case 4:
                tutorialText.text =
                    "Œwietnie! Mo¿esz rozpocz¹æ parkour!";
                break;
        }
    }
}
