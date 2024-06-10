using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SkillsVR.Mechanic.Core;
using UnityEngine;
using UnityEngine.TestTools;


namespace SkillsVR.Mechanic.Test.MechanicSystemTests
{

    public class MechanicSystemUnitTestBase<T> where T : MonoBehaviour, IMechanicSystem
    {
        protected T testSystemComponent;

        [SetUp]
        public virtual void SetUpCreateNewSystemComponentInstance()
        {
            testSystemComponent = CreateNewSystemComponentInstance();
        }

        [TearDown]
        public virtual void TearDownDestroyInstance()
        {
            if (null == testSystemComponent)
            {
                return;
            }
            if (null != testSystemComponent.gameObject)
            {
                GameObject.Destroy(testSystemComponent.gameObject);
            }
            testSystemComponent = null;
        }

        protected virtual T CreateNewSystemComponentInstance()
        {
            var go = new GameObject(this.GetType().Name);
            return go.AddComponent<T>();
        }


        [Test]
        public virtual void T000_CreateMechanicComponent_DefaultParams_NotNull()
        {
            Assert.IsNotNull(testSystemComponent);
        }

        [Test]
        [TestCase(null, null)]
        [TestCase(MechSysEvent.None, null)]
        [TestCase("TestCase_str_int3", 3)]
        public virtual void T001_TriggerEventsWithData_CustomKeyData_ReceiveSameKeyData(object key, object data)
        {
            object receivedKey = (null == key ? new object() : null);
            object receivedData = null;
            testSystemComponent.AddListerner((eventArgs) =>
            {
                receivedKey = eventArgs.eventKey;
                receivedData = eventArgs.data;
            });

            testSystemComponent.TriggerEvent(key, data);
            Assert.AreEqual(key, receivedKey);
            Assert.AreEqual(data, receivedData);
        }

        [Test]
        public virtual void T002_1_StartMechanic_Default_ReceiveOnStartEventInTime()
        {
            bool eventReceived = false;
            testSystemComponent.AddOneTimeTestEventCallback(MechSysEvent.OnStart, (args) => { eventReceived = true; });
            testSystemComponent.StartMechanic();
            Assert.IsTrue(eventReceived);
        }

        [Test]
        public virtual void T002_2_StopMechanic_Default_ReceiveOnStopEventInTime()
        {
            bool eventReceived = false;
            testSystemComponent.AddOneTimeTestEventCallback(MechSysEvent.OnStop, (args) => { eventReceived = true; });
            testSystemComponent.StartMechanic();
            testSystemComponent.StopMechanic();
            Assert.IsTrue(eventReceived);
        }
    }
}
