using UnityEngine;
using System.Collections;

public class fishingManager : MonoBehaviour//, IInteractable
{
    [Header("References")]
    public FishingUI fishingUI;
    public GameObject fishingGameObject;
    public GameObject pressEPrompt;

    [Header("Settings")]
    public float fadeDuration = 0.25f;

    private CanvasGroup canvasGroup;
    private bool isFishing;
    private bool isEnding;

    public enum EndReason
    {
        Timeout,
        Completed,
        Unknown
    }

    private void Awake()
    {
        canvasGroup = fishingGameObject.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogError("Fishing GameObject needs a CanvasGroup!");
        }

        fishingGameObject.SetActive(false);
        pressEPrompt.SetActive(false);
    }

    // ====================================
    // IInteractable
    // ====================================

    public void Interact(InventoryManager playerInventory)
    {
        if (!isFishing && !isEnding)
        {
            StartFishing();
        }
    }

    

    public string GetPrompt()
    {
        return "Fish";
    }

    

    public void OnFocus()
    {
        if (!isFishing && !isEnding)
        {
            pressEPrompt.SetActive(true);
        }
    }

    public void OnLoseFocus()
    {
        pressEPrompt.SetActive(false);
    }

    // ====================================
    // Fishing Logic
    // ====================================

    private void StartFishing()
    {
        isFishing = true;
        isEnding = false;

        pressEPrompt.SetActive(false);
        fishingGameObject.SetActive(true);

        canvasGroup.alpha = 0f;

        StopAllCoroutines();
        StartCoroutine(FadeIn());

        // Reset NoteSpawner
        var spawner = fishingGameObject.GetComponentInChildren<NoteSpawner>();
        if (spawner != null)
        {
            spawner.ResetSpawner();
        }

        // Start HitZone
        var hitZone = fishingGameObject.GetComponentInChildren<HitZone>();
        if (hitZone != null)
        {
            hitZone.StartGame();
        }

        // Start Fishing UI
        if (fishingUI != null)
        {
            fishingUI.StartGame();
        }

        Debug.Log("🎣 START FISHING");
    }

    public void EndFishing(bool success, EndReason reason = EndReason.Unknown)
    {
        if (isEnding) return;

        isEnding = true;
        isFishing = false;

        Debug.Log($"🎣 END | Success: {success} | Reason: {reason}");

        // Stop Fishing UI
        if (fishingUI != null)
        {
            fishingUI.StopGame();
        }

        // Disable HitZone
        var hitZone = fishingGameObject.GetComponentInChildren<HitZone>();
        if (hitZone != null)
        {
            hitZone.gameActive = false;
            hitZone.activeNotes.Clear();
        }

        // Reset spawner
        var spawner = fishingGameObject.GetComponentInChildren<NoteSpawner>();
        if (spawner != null)
        {
            spawner.ResetSpawner();
        }

        StartCoroutine(FadeOut());
    }

    // ====================================
    // Fade Effects
    // ====================================

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

        isEnding = false;

        // PlayerInteraction2D will show prompt again automatically
        pressEPrompt.SetActive(false);
    }
}