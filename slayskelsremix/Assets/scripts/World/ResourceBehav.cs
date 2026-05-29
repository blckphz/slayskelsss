using UnityEngine;

public class ResourceBehav : ItemHealth, IRespawnable
{
    public string resourceID = "stone";
    public ResourceSpawner spawner;

    public string RespawnID => resourceID;

    protected override void Die()
    {
        base.Die();

        if (spawner != null)
        {
        }
    }
}