using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class fishingManager : MonoBehaviour
{
    public FishingUI fishingUI;
    public GameObject fishingGameObject;
    public GameObject pressEPrompt;

    public InputActionReference interactAction;

    private CanvasGroup canvasGroup;

    private bool playerInRange;
    private bool isFishing;

    // 🔒 HARD LOCK
    private bool isEnding;

    public float fadeDuration = 0.25f;

    public enum EndReason
    {
        Timeout,
        Completed,
        Unknown
    }

    private void Awake()
    {
        canvasGroup = fishingGameObject.GetComponent<CanvasGroup>();

        fishingGameObject.SetActive(false);
        pressEPrompt.SetActive(false);
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (playerInRange && !isFishing && !isEnding)
            StartFishing();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (!isEnding)
                pressEPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            pressEPrompt.SetActive(false);
        }
    }

    private void StartFishing()
    {
        isFishing = true;
        isEnding = false;

        pressEPrompt.SetActive(false);
        fishingGameObject.SetActive(true);

        canvasGroup.alpha = 0f;
        StartCoroutine(FadeIn());

        fishingUI.StartGame();

        Debug.Log("🎣 START");
    }

    public void EndFishing(bool success, EndReason reason = EndReason.Unknown)
    {
        // 🔒 HARD GUARD (prevents delay double triggers)
        if (isEnding)
        {
            Debug.Log($"⛔ BLOCKED EndFishing | Reason ignored: {reason}");
            return;
        }

        isEnding = true;
        isFishing = false;

        Debug.Log($"🎣 END | Success: {success} | Reason: {reason}");

        fishingUI.StopGame();

        var hitZone = fishingGameObject.GetComponentInChildren<HitZone>();
        if (hitZone != null)
        {
            hitZone.gameActive = false;
            hitZone.activeNotes.Clear();
        }

        var spawner = fishingGameObject.GetComponentInChildren<NoteSpawner>();
        if (spawner != null)
            spawner.ResetSpawner();

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        fishingGameObject.SetActive(false);

        isEnding = false; // reset for next run

        if (playerInRange)
            pressEPrompt.SetActive(true);
    }
}