﻿namespace linq2db.Sample.Tests
{
    public sealed class Child : IHasWriteableId<Child, long>
    {
        public Id<Child, long> Id { get; set; }
        public Id<Entity, long> ParentId { get; set; }
        public string Name { get; set; }
        public Entity Parent { get; set; }
    }
}