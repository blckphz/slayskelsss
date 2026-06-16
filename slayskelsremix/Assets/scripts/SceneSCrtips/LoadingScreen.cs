using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    [SerializeField] private Canvas loadingCanvas;
    [SerializeField] private TMP_Text loadingText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);

            if (loadingCanvas != null)
            {
                DontDestroyOnLoad(loadingCanvas.gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Show()
    {
        if (loadingCanvas != null)
            loadingCanvas.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (loadingCanvas != null)
            loadingCanvas.gameObject.SetActive(false);
    }

    public void SetText(string text)
    {
        if (loadingText != null)
            loadingText.text = text;
    }

    public void FinishLoading()
    {
        Hide();
    }
}