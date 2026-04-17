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
        // Cache the CanvasGroup for fading
        canvasGroup = fishingGameObject.GetComponent<CanvasGroup>();

        // Ensure everything starts hidden
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
        // Only start if we are in range, not already fishing, and not currently in the "Fade Out" process
        if (playerInRange && !isFishing && !isEnding)
            StartFishing();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // Only show prompt if the game isn't currently active or ending
            if (!isFishing && !isEnding)
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

        // Reset UI Alpha before Fade In
        canvasGroup.alpha = 0f;
        StopAllCoroutines(); // Stop any lingering FadeOuts
        StartCoroutine(FadeIn());

        // 1. Reset and Start the Spawner
        var spawner = fishingGameObject.GetComponentInChildren<NoteSpawner>();
        if (spawner != null)
        {
            spawner.ResetSpawner();
        }

        // 2. Reset and Start the HitZone (Listening for Space/Input)
        var hitZone = fishingGameObject.GetComponentInChildren<HitZone>();
        if (hitZone != null)
        {
            hitZone.StartGame();
        }

        // 3. Start the actual sequence (Phase 1: Audio, then Phase 2: Spawn)
        fishingUI.StartGame();

        Debug.Log("🎣 START FISHING SEQUENCE");
    }

    public void EndFishing(bool success, EndReason reason = EndReason.Unknown)
    {
        // Prevent EndFishing from being called multiple times per session
        if (isEnding) return;

        isEnding = true;
        isFishing = false;

        Debug.Log($"🎣 END | Success: {success} | Reason: {reason}");

        // Stop the UI sequence (stops any active coroutines in FishingUI)
        fishingUI.StopGame();

        // Deactivate the HitZone immediately so no more hits register
        var hitZone = fishingGameObject.GetComponentInChildren<HitZone>();
        if (hitZone != null)
        {
            hitZone.gameActive = false;
            hitZone.activeNotes.Clear();
        }

        // Clean up the spawner (kills any notes flying in the air)
        var spawner = fishingGameObject.GetComponentInChildren<NoteSpawner>();
        if (spawner != null)
            spawner.ResetSpawner();

        // Fade out the UI
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

        // Game is officially "closed" now
        isEnding = false;

        // Bring back the prompt if the player is still standing there
        if (playerInRange)
            pressEPrompt.SetActive(true);
    }
}