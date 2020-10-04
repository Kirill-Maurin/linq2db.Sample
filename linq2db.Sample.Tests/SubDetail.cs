﻿namespace linq2db.Sample.Tests
{
    public sealed class SubDetail : IHasWriteableId<SubDetail, long>
    {
        public Id<SubDetail, long> Id { get; set; }
        public Id<Detail, long> MasterId { get; set; }
        public string Name { get; set; }
        public Detail Master { get; set; }
    }
}