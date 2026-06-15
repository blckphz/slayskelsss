using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text loadingText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(canvas.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string text)
    {
        if (loadingText != null)
            loadingText.text = text;
    }

    public void FinishLoading()
    {
        Destroy(canvas.gameObject);
        Destroy(gameObject);
    }
}