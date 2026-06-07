using UnityEngine;

public class leafbehav : ItemHealth
{
    public bool destroyed = false;

    protected override void Die()
    {
        destroyed = true;
        base.Die();
    }

    public LeafData GetSaveData()
    {
        return new LeafData
        {
            uniqID = UniqOverworldItemID,
            health = health,
            position = transform.position,
            destroyed = destroyed
        };
    }

    public void LoadData(LeafData data)
    {
        if (data == null)
            return;

        UniqOverworldItemID = data.uniqID;
        health = data.health;
        transform.position = data.position;
        destroyed = data.destroyed;

        if (destroyed)
        {
            Destroy(gameObject);
        }
    }
}