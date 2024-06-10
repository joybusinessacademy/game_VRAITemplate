using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DialogExporter;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SkillsVR.Mechanic.Test.MechanicSystemTests
{
    public class MultipleChoiceSystemTest : MechanicSystemUnitTestBase<MultipleChoiceSystem>
    {
        protected override MultipleChoiceSystem CreateNewSystemComponentInstance()
        {
            var component = base.CreateNewSystemComponentInstance();
            CreateDefaultParamsForTestMCQComponent(component);
            return component;
        }

        protected void CreateDefaultParamsForTestMCQComponent(MultipleChoiceSystem component)
        {
            Assert.IsNotNull(component);
            component.mechanicData = ScriptableObject.CreateInstance<MultipleChoiceQuestionScriptable>();
            component.questionText = component.AddComponentOnNewChildGameObject<TextMeshProUGUI>();
            component.canvasGroup = component.gameObject.AddComponent<CanvasGroup>();
            // question item template
            component.multipleChoiceQuestionItem = component.AddComponentOnNewChildGameObject<MultipleChoiceQuestionItem>();
            component.multipleChoiceQuestionItem.questionButton = component.multipleChoiceQuestionItem.gameObject.AddComponent<Button>();
            component.multipleChoiceQuestionItem.SetupButton();
            component.multipleChoiceQuestionItem.questionText = component.multipleChoiceQuestionItem.gameObject.AddComponent<TextMeshProUGUI>();
            component.questionButtonContainer = component.AddChildGameObject(nameof(component.questionButtonContainer)).transform;
        }

        [Test]
        [TestCase(MCQEvent.OnChoiceSelected)]
        [TestCase(MCQEvent.FinishedMCQ)]
        [TestCase(MCQEvent.CorrectButton)]
        [TestCase(MCQEvent.InCorrectButton)]
        public void T101_MCQEventCallback_MCQEvent_ReceiveSameKey(MCQEvent eventKey)
        {
            Assert.IsNotNull(testSystemComponent);
            MCQEvent receivedKey = MCQEvent.None;
            testSystemComponent.AddListerner((eventArgs) => { receivedKey = (MCQEvent)eventArgs.eventKey; });
            testSystemComponent.TriggerEvent(eventKey);
            Assert.AreEqual(eventKey, receivedKey);
        }


        protected void InitFalseTrueFalseAnswers()
        {
            testSystemComponent.mechanicData.questions = new List<MultipleChoiceAnswer>();
            testSystemComponent.mechanicData.questions.Add(new MultipleChoiceAnswer() { answerText = "1", isCorrectAnswer = false });
            testSystemComponent.mechanicData.questions.Add(new MultipleChoiceAnswer() { answerText = "2", isCorrectAnswer = true });
            testSystemComponent.mechanicData.questions.Add(new MultipleChoiceAnswer() { answerText = "3", isCorrectAnswer = false });
        }

        protected void ClickOptionByIndex(int index)
        {
            var buttons = testSystemComponent.questionButtonContainer.GetComponentsInChildren<Button>();
            buttons[index].onClick.Invoke();
        }


        [UnityTest]
        public virtual IEnumerator T102_OnChoiceSelected_FalseTrueFalseAnswersClickIndex2_ReceiveEventWithIndex2()
        {
            Assert.IsNotNull(testSystemComponent);
            // Create test options
            InitFalseTrueFalseAnswers();


            // Register on select event
            int receivedSelectedIndex = -1;
            testSystemComponent.AddOneTimeTestEventCallback(MCQEvent.OnChoiceSelected, (args) => { receivedSelectedIndex = args.GetData<int>(-1); });
            // Set click index
            int inputSelectIndex = 2;
            // simluate click button
            testSystemComponent.StartMechanic();
            yield return null;
            ClickOptionByIndex(inputSelectIndex);
            yield return null;
            // Assert
            Assert.AreEqual(receivedSelectedIndex, inputSelectIndex);
        }

        [UnityTest]
        public virtual IEnumerator T103_CorrectButtonEvent_FalseTrueFalseAnswersClickTrue_ReceiveCorrectButtonEvent()
        {
            Assert.IsNotNull(testSystemComponent);
            // Create test options
            InitFalseTrueFalseAnswers();
            Assert.AreEqual(1, testSystemComponent.mechanicData.questions.Where(x=> x.isCorrectAnswer).Count());

            // Register on select event
            bool receiveCorrectButtonEvent = false;
            testSystemComponent.AddOneTimeTestEventCallback(MCQEvent.CorrectButton, (args) => { receiveCorrectButtonEvent = true; });

            // Get test button index
            int inputSelectIndex = testSystemComponent.mechanicData.questions.FindIndex(x=> x.isCorrectAnswer);
            Assert.GreaterOrEqual(inputSelectIndex, 0);
            Assert.Less(inputSelectIndex, testSystemComponent.mechanicData.questions.Count);

            // test click button
            testSystemComponent.StartMechanic();
            yield return null;
            ClickOptionByIndex(inputSelectIndex);
            yield return null;

            // Assert
            Assert.IsTrue(receiveCorrectButtonEvent);
        }

        [UnityTest]
        public virtual IEnumerator T104_InCorrectButtonEvent_FalseTrueFalseAnswersClickFalse1_ReceiveInCorrectButtonEvent()
        {
            Assert.IsNotNull(testSystemComponent);
            // Create test options
            InitFalseTrueFalseAnswers();
            Assert.GreaterOrEqual(testSystemComponent.mechanicData.questions.Where(x => !x.isCorrectAnswer).Count(), 1);

            // Register on select event
            bool receiveInCorrectButtonEvent = false;
            testSystemComponent.AddOneTimeTestEventCallback(MCQEvent.InCorrectButton, (args) => { receiveInCorrectButtonEvent = true; });

            int inputSelectIndex = testSystemComponent.mechanicData.questions.FindIndex(x => !x.isCorrectAnswer);

            // test click button
            testSystemComponent.StartMechanic();
            yield return null;
            ClickOptionByIndex(inputSelectIndex);
            yield return null;

            // Assert
            Assert.IsTrue(receiveInCorrectButtonEvent);
        }

        [UnityTest]
        public virtual IEnumerator T105_FinishedMCQEvent_FalseTrueFalseAnswersClickFalse1_ReceiveFinishedMCQEvent()
        {
            Assert.IsNotNull(testSystemComponent);
            // Create test options
            InitFalseTrueFalseAnswers();

            // Register on select event
            bool receiveEvent = false;
            testSystemComponent.AddOneTimeTestEventCallback(MCQEvent.FinishedMCQ, (args) => { receiveEvent = true; });

            int inputSelectIndex = testSystemComponent.mechanicData.questions.FindIndex(x => !x.isCorrectAnswer);

            // test click button
            testSystemComponent.StartMechanic();
            yield return null;
            ClickOptionByIndex(inputSelectIndex);
            yield return null;

            // Assert
            Assert.IsTrue(receiveEvent);
        }
    }
}
