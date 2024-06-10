using SkillsVR.VisualElements;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.VisualElements
{
    public class ConditionalElementBlock : VisualElement
    {
        readonly List<VisualElement> trueList;
        readonly List<VisualElement> falseList;
        Toggle conditionalToggle;

        VisualElement activeElementsContainer;
        VisualElement toggleContainer;

        public ConditionalElementBlock( List<VisualElement> trueElements, List<VisualElement> falseElements, Toggle toggle)
        {
            conditionalToggle = toggle;

            trueList = trueElements;
            falseList = falseElements;

            name = "conditional-elements-background";

            styleSheets.Add(Resources.Load<StyleSheet>("ConditionalElementBlock"));
            Refresh();
        }

        public void Refresh()
        {
            Clear();

            activeElementsContainer = new();
            toggleContainer = new();
            toggleContainer.name = "toggle-container";
            toggleContainer.AddToClassList("container");

            activeElementsContainer.name = "active-container";
            activeElementsContainer.AddToClassList("container");


            toggleContainer.Add(conditionalToggle);
            Add(toggleContainer);


            if (conditionalToggle.value)
            {
                foreach (VisualElement trueItem in trueList)
                {
                    activeElementsContainer.Add(trueItem);
                }

                if (trueList.Count == 0)
                    activeElementsContainer.AddToClassList("empty");
                else
                    Add(new Divider());
            }
            else
            {
                foreach (VisualElement falseItem in falseList)
                {
                    activeElementsContainer.Add(falseItem);
                }

                if (falseList.Count == 0)
                    activeElementsContainer.AddToClassList("empty");
                else
                    Add(new Divider());
            }

            Add(activeElementsContainer);

        }
    }
}
