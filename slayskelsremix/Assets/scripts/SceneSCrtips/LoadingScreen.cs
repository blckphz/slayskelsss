using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    [SerializeField] private Canvas loadingCanvas;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private GameObject Background;

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
    }

    public void Show()
    {

            Background.SetActive(true);
            loadingText.gameObject.SetActive(true);
            loadingCanvas.gameObject.SetActive(true);

    }

    public void Hide()
    {
        Background.SetActive(false);
        loadingText.gameObject.SetActive(false);
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