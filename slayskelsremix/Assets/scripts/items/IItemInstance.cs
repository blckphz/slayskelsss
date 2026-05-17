public interface IItemInstance
{
    ItemData Blueprint { get; }
    float CurrentDurability { get; set; }
    float MaxDurability { get; }
    int StackCount { get; set; }

    bool IsBroken { get; }
    void UseDurability(float amount);
}