using UnityEngine;

public class stoneWORLDobjectbehav : ItemHealth
{
    public bool destroyed = false;

    protected override void Die()
    {
        destroyed = true;
        base.Die();
    }

    public stonedata GetSaveData()
    {
        return new stonedata
        {
            uniqID = UniqOverworldItemID,
            health = health,
            position = transform.position,
            destroyed = destroyed
        };
    }

    public void LoadData(stonedata data)
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