using UnityEngine;
using Unity.Cinemachine;

public class CinemachineSnapToPlayer : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform player;

    private void Awake()
    {
        if (cinemachineCamera == null)
            cinemachineCamera = GetComponent<CinemachineCamera>();

        if (cinemachineCamera == null || player == null)
            return;

        // Assign follow target
        cinemachineCamera.Follow = player;

        // Snap camera immediately to player position
        Vector3 pos = player.position;
        pos.z = Camera.main != null ? Camera.main.transform.position.z : -10f;

        transform.position = pos;
    }

    private void Start()
    {
        // Force Cinemachine to update immediately
        if (cinemachineCamera != null)
        {
            cinemachineCamera.PreviousStateIsValid = false;
        }
    }
}