namespace linq2db.Sample.Tests
{
    public sealed class Entity2Item
    {
        public Id<Entity, long> EntityId { get; set; }
        public Entity Entity { get; set; }
        public Id<Item, long> ItemId { get; set; }
        public Item Item { get; set; }
    }
}