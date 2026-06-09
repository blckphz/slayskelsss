using System.Collections;
using UnityEngine;

public class homeInteract : MonoBehaviour
{
    [SerializeField] private float saveInterval = 5f;
    [SerializeField] private float fadeSpeed = 2f;

    private bool playerInside;
    private float nextSaveTime;

    private CanvasGroup saveIconGroup;
    private Coroutine fadeRoutine;

    private void Start()
    {
        GameObject saveIcon = GameObject.Find("SaveIcon");

        if (saveIcon != null)
        {
            saveIconGroup = saveIcon.GetComponent<CanvasGroup>();

            if (saveIconGroup != null)
            {
                saveIconGroup.alpha = 0f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        TrySaveGame();

        nextSaveTime = Time.time + saveInterval;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        InteractionUI.Instance.Hide();
    }

    private void Update()
    {
        if (!playerInside) return;

        if (Time.time >= nextSaveTime)
        {
            TrySaveGame();
            nextSaveTime = Time.time + saveInterval;
        }
    }

    private void TrySaveGame()
    {
        if (BuildingSaveManager.Instance != null)
        {
            BuildingSaveManager.Instance.SaveNow();
        }


        ShowSaveIconFade();

    }

    private void ShowSaveIconFade()
    {
        if (saveIconGroup == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeSaveIcon());
    }

    private IEnumerator FadeSaveIcon()
    {
        // Fade IN (0 → 1)
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            saveIconGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        // Hold briefly
        yield return new WaitForSeconds(0.3f);

        // Fade OUT (1 → 0)
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            saveIconGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
    }

}