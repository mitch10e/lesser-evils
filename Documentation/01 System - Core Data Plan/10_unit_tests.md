# Part 6: Creating Unit Tests

Now let's write tests to verify our state classes work correctly.

## Step 6.1: ResourceState Tests

Create `Assets/Tests/EditMode/ResourceStateTests.cs`:

```csharp
using NUnit.Framework;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Tests.EditMode
{
    [TestFixture]
    public class ResourceStateTests
    {
        private ResourceState _state;

        [SetUp]
        public void SetUp()
        {
            _state = new ResourceState();
        }

        [Test]
        public void Constructor_InitializesWithStartingValues()
        {
            Assert.AreEqual(GameConstants.STARTING_CURRENCY, _state.GetResource(ResourceType.Currency));
            Assert.AreEqual(GameConstants.STARTING_FUEL, _state.GetResource(ResourceType.Fuel));
        }

        [Test]
        public void AddResource_IncreasesAmount()
        {
            int initial = _state.GetResource(ResourceType.Currency);
            _state.AddResource(ResourceType.Currency, 100);
            Assert.AreEqual(initial + 100, _state.GetResource(ResourceType.Currency));
        }

        [Test]
        public void SpendResource_DecreasesAmount_WhenSufficient()
        {
            _state.SetResource(ResourceType.Currency, 500);
            bool result = _state.SpendResource(ResourceType.Currency, 200);

            Assert.IsTrue(result);
            Assert.AreEqual(300, _state.GetResource(ResourceType.Currency));
        }

        [Test]
        public void SpendResource_ReturnsFalse_WhenInsufficient()
        {
            _state.SetResource(ResourceType.Currency, 100);
            bool result = _state.SpendResource(ResourceType.Currency, 200);

            Assert.IsFalse(result);
            Assert.AreEqual(100, _state.GetResource(ResourceType.Currency)); // Unchanged
        }

        [Test]
        public void HasResource_ReturnsTrue_WhenSufficient()
        {
            _state.SetResource(ResourceType.Fuel, 50);
            Assert.IsTrue(_state.HasResource(ResourceType.Fuel, 50));
            Assert.IsTrue(_state.HasResource(ResourceType.Fuel, 25));
        }

        [Test]
        public void HasResource_ReturnsFalse_WhenInsufficient()
        {
            _state.SetResource(ResourceType.Fuel, 50);
            Assert.IsFalse(_state.HasResource(ResourceType.Fuel, 100));
        }

        [Test]
        public void AddResource_PreventsNegativeValues()
        {
            _state.SetResource(ResourceType.Currency, 100);
            _state.AddResource(ResourceType.Currency, -200);
            Assert.AreEqual(0, _state.GetResource(ResourceType.Currency));
        }

        [Test]
        public void Reset_RestoresToStartingValues()
        {
            _state.SetResource(ResourceType.Currency, 9999);
            _state.Reset();
            Assert.AreEqual(GameConstants.STARTING_CURRENCY, _state.GetResource(ResourceType.Currency));
        }
    }
}
```

## Step 6.2: SquadState Tests

Create `Assets/Tests/EditMode/SquadStateTests.cs`:

```csharp
using NUnit.Framework;
using Game.Core.Data;
using Game.Core.States;

namespace Game.Tests.EditMode
{
    [TestFixture]
    public class SquadStateTests
    {
        private SquadState _state;

        [SetUp]
        public void SetUp()
        {
            _state = new SquadState();
        }

        [Test]
        public void AddUnit_IncreasesRosterCount()
        {
            var unit = UnitData.CreateDefault("u1", "Test Unit");
            _state.AddUnit(unit);

            Assert.AreEqual(1, _state.Roster.Count);
        }

        [Test]
        public void AddUnit_PreventsDuplicateIds()
        {
            var unit1 = UnitData.CreateDefault("u1", "Unit 1");
            var unit2 = UnitData.CreateDefault("u1", "Unit 2");

            _state.AddUnit(unit1);
            _state.AddUnit(unit2);

            Assert.AreEqual(1, _state.Roster.Count);
        }

        [Test]
        public void RemoveUnit_DecreasesRosterCount()
        {
            var unit = UnitData.CreateDefault("u1", "Test");
            _state.AddUnit(unit);
            _state.RemoveUnit("u1");

            Assert.AreEqual(0, _state.Roster.Count);
        }

        [Test]
        public void GetActiveUnits_ExcludesInjuredAndDead()
        {
            _state.AddUnit(UnitData.CreateDefault("u1", "Active"));
            _state.AddUnit(UnitData.CreateDefault("u2", "Injured"));
            _state.AddUnit(UnitData.CreateDefault("u3", "Dead"));

            _state.UpdateUnitStatus("u2", UnitStatus.Injured);
            _state.UpdateUnitStatus("u3", UnitStatus.Dead);

            var active = _state.GetActiveUnits();
            Assert.AreEqual(1, active.Count);
            Assert.AreEqual("u1", active[0].Id);
        }

        [Test]
        public void AddExperience_TriggersLevelUp()
        {
            var unit = UnitData.CreateDefault("u1", "Test");
            _state.AddUnit(unit);

            // Add enough XP to level up (default is 100 XP to level)
            _state.AddExperience("u1", 150);

            var updated = _state.GetUnit("u1");
            Assert.AreEqual(2, updated.Value.Level);
        }

        [Test]
        public void CanDeploy_ReturnsFalse_WhenUnitInjured()
        {
            var unit = UnitData.CreateDefault("u1", "Test");
            _state.AddUnit(unit);
            _state.UpdateUnitStatus("u1", UnitStatus.Injured);

            Assert.IsFalse(_state.CanDeploy("u1"));
        }

        [Test]
        public void DeployUnit_AddsToDeployedList()
        {
            var unit = UnitData.CreateDefault("u1", "Test");
            _state.AddUnit(unit);

            bool result = _state.DeployUnit("u1");

            Assert.IsTrue(result);
            Assert.Contains("u1", _state.DeployedUnitIds);
        }

        [Test]
        public void DeployUnit_FailsWhenSquadFull()
        {
            _state.MaxSquadSize = 1;
            _state.AddUnit(UnitData.CreateDefault("u1", "Unit 1"));
            _state.AddUnit(UnitData.CreateDefault("u2", "Unit 2"));

            _state.DeployUnit("u1");
            bool result = _state.DeployUnit("u2");

            Assert.IsFalse(result);
        }

        [Test]
        public void ProcessInjuryRecovery_DecrementsRecoveryTurns()
        {
            var unit = UnitData.CreateDefault("u1", "Test");
            _state.AddUnit(unit);
            _state.UpdateUnitStatus("u1", UnitStatus.Injured);

            int initialTurns = _state.GetUnit("u1").Value.InjuryRecoveryTurns;
            _state.ProcessInjuryRecovery();

            Assert.AreEqual(initialTurns - 1, _state.GetUnit("u1").Value.InjuryRecoveryTurns);
        }
    }
}
```

## Running the Tests

1. In Unity, go to `Window > General > Test Runner`
2. Click on the `EditMode` tab
3. Click `Run All` to run your tests
4. All tests should pass (green checkmarks)

If any tests fail, read the error message and fix the corresponding code.
