namespace linq2db.Sample.Tests
{
    public sealed class Item : IHasWriteableId<Item, long>
    {
        public Id<Item, long> Id { get; set; }
        public string Name { get; set; }
    }
}