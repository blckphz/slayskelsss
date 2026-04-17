using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DestroyOnAnimationEnd : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float waveSpeed = 2f;
    [SerializeField] private string shaderProperty = "_WaveDistance";

    private Renderer rend;
    private MaterialPropertyBlock mpb;

    private float waveDistance = 0f;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        // Increase wave distance over time
        waveDistance += Time.deltaTime * waveSpeed;

        // Apply to shader
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(shaderProperty, waveDistance);
        rend.SetPropertyBlock(mpb);
    }

    // Called via Animation Event at the end of the explosion
    public void OnAnimationComplete()
    {
        Destroy(gameObject);
    }
}