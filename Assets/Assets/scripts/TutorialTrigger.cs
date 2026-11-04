
using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider))]
public class TutorialTrigger : MonoBehaviour
{
    [Header("UI")]
    public GameObject tutorialPanel;
    public TMP_Text tutorialText;

    [Header("Podpowiedz")]
    [TextArea(2, 4)]
    public string message =
        "Przytrzymaj Left Ctrl + W, aby sie czolgac";

    [Header("Ustawienia")]
    public bool showOnlyOnce = true;

    private bool playerInside = false;
    private bool wasShown = false;
    private bool completed = false;

    private void Awake()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Jesli komunikat byl juz pokazany,
        // nie pokazuj go ponownie.
        if (showOnlyOnce && wasShown)
            return;

        playerInside = true;
        wasShown = true;

        if (tutorialText != null)
            tutorialText.text = message;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    private void Update()
    {
        if (!playerInside || completed)
            return;

        bool crawling =
            Input.GetKey(KeyCode.LeftControl) &&
            (Input.GetKey(KeyCode.W) ||
             Input.GetKey(KeyCode.A) ||
             Input.GetKey(KeyCode.S) ||
             Input.GetKey(KeyCode.D) ||
             Input.GetKey(KeyCode.UpArrow) ||
             Input.GetKey(KeyCode.DownArrow) ||
             Input.GetKey(KeyCode.LeftArrow) ||
             Input.GetKey(KeyCode.RightArrow));

        if (crawling)
        {
            completed = true;
            HideTutorial();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        HideTutorial();
    }

    private void HideTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }
}
