using NUnit.Framework;
using UnityEngine;
using Asteroids.Events;

namespace Asteroids.Tests
{
    public class EventChannelTests
    {
        [Test]
        public void IntChannel_DeliversTheRaisedValueToEverySubscriber()
        {
            var channel = ScriptableObject.CreateInstance<IntEventChannelSO>();
            int a = 0, b = 0;
            channel.OnEventRaised += value => a = value;
            channel.OnEventRaised += value => b = value * 2;

            channel.RaiseEvent(21);

            Assert.AreEqual(21, a);
            Assert.AreEqual(42, b);
            Object.DestroyImmediate(channel);
        }

        [Test]
        public void IntChannel_StopsDeliveringAfterUnsubscribe()
        {
            var channel = ScriptableObject.CreateInstance<IntEventChannelSO>();
            int received = 0;
            System.Action<int> handler = value => received = value;
            channel.OnEventRaised += handler;
            channel.RaiseEvent(1);

            channel.OnEventRaised -= handler;
            channel.RaiseEvent(2);

            Assert.AreEqual(1, received);
            Object.DestroyImmediate(channel);
        }

        [Test]
        public void VoidChannel_RaisesOncePerCall()
        {
            var channel = ScriptableObject.CreateInstance<VoidEventChannelSO>();
            int calls = 0;
            channel.OnEventRaised += () => calls++;

            channel.RaiseEvent();
            channel.RaiseEvent();

            Assert.AreEqual(2, calls);
            Object.DestroyImmediate(channel);
        }

        [Test]
        public void Vector3Channel_DeliversThePosition()
        {
            var channel = ScriptableObject.CreateInstance<Vector3EventChannelSO>();
            Vector3 received = Vector3.zero;
            channel.OnEventRaised += position => received = position;

            channel.RaiseEvent(new Vector3(1f, 2f, 3f));

            Assert.AreEqual(new Vector3(1f, 2f, 3f), received);
            Object.DestroyImmediate(channel);
        }

        [Test]
        public void RaisingWithNoSubscribers_DoesNotThrow()
        {
            var channel = ScriptableObject.CreateInstance<IntEventChannelSO>();

            Assert.DoesNotThrow(() => channel.RaiseEvent(5));
            Object.DestroyImmediate(channel);
        }
    }
}
