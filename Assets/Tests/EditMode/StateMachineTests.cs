using System.Collections.Generic;
using NUnit.Framework;
using Asteroids.Enemies.AI;

namespace Asteroids.Tests
{
    public class StateMachineTests
    {
        private class RecordingState : IState
        {
            private readonly string name;
            private readonly List<string> log;

            public RecordingState(string name, List<string> log)
            {
                this.name = name;
                this.log = log;
            }

            public void Enter() => log.Add($"{name}.Enter");
            public void Update() => log.Add($"{name}.Update");
            public void FixedUpdate() => log.Add($"{name}.FixedUpdate");
            public void Exit() => log.Add($"{name}.Exit");
        }

        private List<string> log;
        private StateMachine machine;
        private RecordingState first;
        private RecordingState second;

        [SetUp]
        public void SetUp()
        {
            log = new List<string>();
            machine = new StateMachine();
            first = new RecordingState("A", log);
            second = new RecordingState("B", log);
        }

        [Test]
        public void Initialize_EntersTheStartingState()
        {
            machine.Initialize(first);

            Assert.AreSame(first, machine.CurrentState);
            CollectionAssert.AreEqual(new[] { "A.Enter" }, log);
        }

        [Test]
        public void ChangeState_ExitsTheOldStateBeforeEnteringTheNewOne()
        {
            machine.Initialize(first);
            log.Clear();

            machine.ChangeState(second);

            Assert.AreSame(second, machine.CurrentState);
            CollectionAssert.AreEqual(new[] { "A.Exit", "B.Enter" }, log);
        }

        [Test]
        public void ChangeState_ToTheCurrentState_DoesNothing()
        {
            machine.Initialize(first);
            log.Clear();

            machine.ChangeState(first);

            Assert.IsEmpty(log);
        }

        [Test]
        public void ChangeState_ToNull_IsIgnored()
        {
            machine.Initialize(first);
            log.Clear();

            machine.ChangeState(null);

            Assert.AreSame(first, machine.CurrentState);
            Assert.IsEmpty(log);
        }

        [Test]
        public void Update_AndFixedUpdate_GoToTheCurrentStateOnly()
        {
            machine.Initialize(first);
            machine.ChangeState(second);
            log.Clear();

            machine.Update();
            machine.FixedUpdate();

            CollectionAssert.AreEqual(new[] { "B.Update", "B.FixedUpdate" }, log);
        }

        [Test]
        public void Update_BeforeInitialize_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => machine.Update());
            Assert.DoesNotThrow(() => machine.FixedUpdate());
        }
    }
}
