using System.Collections;
using UnityEngine;

public class treefallsequence : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallAngle = 90f;
    public float fallDuration = 0.5f;

    [Header("Cleanup")]
    public float destroyDelay = 1f;

    [Header("Direction")]
    public bool randomDirection = true;

    private bool hasFallen = false;

    private GameObject lootPrefab;
    private int lootAmount;

    public void SetupLoot(GameObject loot, int amount)
    {
        lootPrefab = loot;
        lootAmount = amount;
    }

    void Start()
    {
        StartCoroutine(FallRoutine());
    }

    IEnumerator FallRoutine()
    {
        if (hasFallen) yield break;
        hasFallen = true;

        Quaternion startRot = transform.rotation;

        float dir = randomDirection
            ? (Random.value > 0.5f ? 1f : -1f)
            : 1f;

        Quaternion endRot = startRot *
            Quaternion.Euler(0f, 0f, dir * fallAngle);

        float t = 0f;

        while (t < fallDuration)
        {
            t += Time.deltaTime;

            float lerp = t / fallDuration;

            transform.rotation = Quaternion.Lerp(
                startRot,
                endRot,
                lerp
            );

            yield return null;
        }

        transform.rotation = endRot;

        SpawnLoot();

        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }

    void SpawnLoot()
    {
        if (lootPrefab == null) return;

        for (int i = 0; i < lootAmount; i++)
        {
            GameObject loot = Instantiate(
                lootPrefab,
                transform.position,
                Quaternion.identity
            );

            LootArc arc = loot.GetComponent<LootArc>();

            if (arc != null)
            {
                arc.Initialize(transform.position);
            }
        }
    }
}