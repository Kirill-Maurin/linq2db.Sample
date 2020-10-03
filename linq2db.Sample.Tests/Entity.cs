using System.Collections.Generic;
using JetBrains.Annotations;

namespace linq2db.Sample.Tests
{
    public sealed class Entity : IHasWriteableId<Entity, long>
    {
        public Id<Entity, long> Id { get; set; }
        public string Name { get; set; }
        
        public IEnumerable<Detail> Details { get; set; }
        public IEnumerable<Child> Children { get; set; }
        public IEnumerable<Entity2Item> Items { get; set; }
    }
}