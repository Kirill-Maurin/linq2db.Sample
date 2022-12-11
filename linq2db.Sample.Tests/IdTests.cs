using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FluentAssertions;
using LinqToDB;
using LinqToDB.Common.Logging;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;
using Xunit;

namespace linq2db.Sample.Tests
{
    public sealed class IdTests : IDisposable
    {
        public IdTests()
        {
            _efContext = new TestContext(
                new DbContextOptionsBuilder()
                    .ReplaceService<IValueConverterSelector, IdValueConverterSelector<NpgsqlValueConverterSelector>>()
                    .UseLoggerFactory(LoggerFactory.Create(b => b.AddConsole()))
                    .EnableSensitiveDataLogging()
                    .UseNpgsql(
                        "Server=localhost;Port=5432;" +
                        $"Database=test_ef_data{Thread.CurrentThread.ManagedThreadId};" +
                        "User Id=postgres;Password=TestPassword;Pooling=true;MinPoolSize=10;MaxPoolSize=100;",
                        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                    .Options);
            _efContext.Database.EnsureDeleted();
            _efContext.Database.EnsureCreated();
        }

        IDataContext CreateLinqToDbContext(TestContext testContext)
        {
            var result = testContext.CreateLinqToDbContext();
            result.GetTraceSwitch().Level = TraceLevel.Verbose;
            return result;
        }

        readonly TestContext _efContext;

        [Theory]
        [InlineData("test insert")]
        public void TestInsertWithoutTracker(string name) 
            => _efContext
                .Arrange(c => CreateLinqToDbContext(c))
                .Act(c => c.Insert(new Entity { Name = name }))
                .Assert(id => _efContext.Entities.Single(e => e.Id == id).Name.Should().Be(name));

        [Theory]
        [InlineData("test insert")]
        public void TestInsertWithoutNew(string name) 
            => _efContext.Entities
                .Arrange(e => e.ToLinqToDBTable())
                .Act(e => e.InsertWithInt64Identity(() => new Entity {Name = name}))
                .Assert(id => _efContext.Entities.Single(e => e.Id == id).Name.Should().Be(name));

        [Theory]
        [InlineData("test insert ef")]
        public void TestInsertEfCore(string name) 
            => _efContext
                .Arrange(c => c.Entities.Add(new Entity {Name = "test insert ef"}))
                .Act(_ => _efContext.SaveChanges())
                .Assert(_ => _efContext.Entities.Single().Name.Should().Be(name));

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void TestIncludeDetails(bool l2db, bool tracking)
            => _efContext
                .Arrange(c => InsertDefaults(CreateLinqToDbContext(c)))
                .Act(c => c
                    .Entities
                    .Where(e => e.Name == "Alpha")
                    .Include(e => e.Details)
                    .ThenInclude(d => d.Details)
                    .Include(e => e.Children)
                    .AsLinqToDb(l2db)
                    .AsTracking(tracking)
                    .ToArray())
                .Assert(e => e.First().Details.First().Details.Count().Should().Be(2));

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestManyToManyIncludeTrackerPoison(bool l2db)
            => _efContext
                .Arrange(c => InsertDefaults(CreateLinqToDbContext(c)))
                .Act(c =>
                {
                    var q = c.Entities
                        .Include(e => e.Items)
                        .ThenInclude(x => x.Item);
                    var f = q.AsLinqToDb(l2db).AsTracking().ToArray();
                    var s = q.AsLinqToDb(!l2db).AsTracking().ToArray();
                    return (First: f, Second: s);
                })
                .Assert(r => r.First[0].Items.Count().Should().Be(r.Second[0].Items.Count()));
        
        
        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void TestManyToManyInclude(bool l2db, bool tracking)
            => _efContext
                .Arrange(c => InsertDefaults(CreateLinqToDbContext(c)))
                .Act(c => c.Entities
                    .Include(e => e.Items)
                    .ThenInclude(x => x.Item)
                    .AsLinqToDb(l2db)
                    .AsTracking(tracking)
                    .ToArray())
                .Assert(m => m[0].Items.First().Item.Should().BeSameAs(m[1].Items.First().Item));

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void TestMasterInclude(bool l2db, bool tracking)
            => _efContext
                .Arrange(c => InsertDefaults(CreateLinqToDbContext(c)))
                .Act(c => c
                    .Details
                    .Include(d => d.Master)
                    .AsLinqToDb(l2db)
                    .AsTracking(tracking)
                    .ToArray())
                .Assert(m => m[0].Master.Should().BeSameAs(m[1].Master));

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void TestMasterInclude2(bool l2db, bool tracking)
            => _efContext
                .Arrange(c => InsertDefaults(CreateLinqToDbContext(c)))
                .Act(c => c
                    .Details
                    .Include(d => d.Master)
                    .AsTracking(tracking)
                    .AsLinqToDb(l2db)
                    .ToArray())
                .Assert(m => m[0].Master.Should().BeSameAs(m[1].Master));

        void InsertDefaults(IDataContext dataContext)
        {
            var a = dataContext.Insert(new Entity {Name = "Alpha"});
            var b = dataContext.Insert(new Entity {Name = "Bravo"});
            var d = dataContext.Insert(new Detail {Name = "First", MasterId = a});
            var r = dataContext.Insert(new Item {Name = "Red"});
            var g = dataContext.Insert(new Item {Name = "Green"});
            var w = dataContext.Insert(new Item {Name = "White"});

            dataContext.Insert(new Detail {Name = "Second", MasterId = a});
            dataContext.Insert(new SubDetail {Name = "Plus", MasterId = d});
            dataContext.Insert(new SubDetail {Name = "Minus", MasterId = d});
            dataContext.Insert(new Child {Name = "One", ParentId = a});
            dataContext.Insert(new Child {Name = "Two", ParentId = a});
            dataContext.Insert(new Child {Name = "Three", ParentId = a});
            dataContext.Insert(new Entity2Item {EntityId = a, ItemId = r});
            dataContext.Insert(new Entity2Item {EntityId = a, ItemId = g});
            dataContext.Insert(new Entity2Item {EntityId = b, ItemId = r});
            dataContext.Insert(new Entity2Item {EntityId = b, ItemId = w});
        }

        public class TestContext : DbContext
        {
            public TestContext(DbContextOptions options) : base(options) { }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<Entity2Item>().HasKey(x => new { x.EntityId, x.ItemId});
                modelBuilder
                    .UseSnakeCase()
                    .UseIdAsKey()
                    .UseOneIdSequence<long>("test", sn => $"nextval('{sn}')");
            }


            public DbSet<Entity> Entities { get; set; } 
            public DbSet<Detail> Details { get; set; } 
            public DbSet<SubDetail> SubDetails { get; set; } 
            public DbSet<Item> Items { get; set; }
            public DbSet<Child> Children { get; set; }
            public DbSet<Linked> Linked { get; set; }
        }

        public void Dispose() => _efContext.Dispose();
    }
}