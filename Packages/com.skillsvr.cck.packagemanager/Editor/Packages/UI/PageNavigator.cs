using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public class PageNavigator : BaseField<int>
    {
        public override int value
        {
            get
            {
                return base.value;
            }
            set
            {
                if (value == base.value)
                {
                    return;
                }
                base.value = Mathf.Max(value, 0);
                RequestRefreshButtons();
            }
        }

        private int myLastPageIndex = 1;
        public int LastPageIndex
        {
            get
            {
                return myLastPageIndex;
            }
            set
            {
                if (value == myLastPageIndex)
                {
                    return;
                }
                myLastPageIndex = Mathf.Max(0, value);
                RequestRefreshButtons();
            }
        }

        public int PageCount
        {
            get
            {
                return Mathf.Max(0, LastPageIndex + 1);
            }
            set
            {
                LastPageIndex = value - 1;
            }
        }

        public PageNavigator(int initPageIndex = 0, int initPageCount = 1) : base("", null)
        {
            this.name = nameof(PageNavigator);
            LastPageIndex = initPageCount-1;
            value = initPageIndex;
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            RequestRefreshButtons();
        }

        public void CreateGUI()
        {
            RequestRefreshButtons();
        }

        IVisualElementScheduledItem refreshSchedule;
        protected void RequestRefreshButtons()
        {
            if (null == refreshSchedule)
            {
                this.schedule.Execute(RefreshButtons);
            }
        }

        protected void RefreshButtons()
        {
            refreshSchedule = null;
            this.Clear();
            if (1 >= PageCount)
            {
                return;
            }

            var prevButton = CreatePageButton(Mathf.Max(0, value - 1), "Previours");
            prevButton.SetEnabled(0 < value);
            this.Add(prevButton);


            if (PageCount > 5 )
            {
                var page1Button = CreatePageButton(0);
                this.Add(page1Button);
                if (value < 4)
                {
                    for (int i = 1; i < 4; i++)
                    {
                        this.Add(CreatePageButton(i));
                    }
                    this.Add(new Label("..."));
                }
                else if (value >= LastPageIndex - 4)
                {
                    this.Add(new Label("..."));
                    for (int i = LastPageIndex - 4; i < LastPageIndex; i++)
                    {
                        this.Add(CreatePageButton(i));
                    }
                }
                else
                {
                    this.Add(new Label("..."));

                    for (int i = -1; i < 2; i++)
                    {
                        var btn = CreatePageButton(value + i);
                        this.Add(btn);
                    }

                    this.Add(new Label("..."));
                }
                var pageLastButton = CreatePageButton(LastPageIndex);
                this.Add(pageLastButton);
            }
            else
            {
                for(int i = 0; i <= LastPageIndex; i++)
                {
                    this.Add(CreatePageButton(i));
                }
            }


            var nextButton = CreatePageButton(Mathf.Min(LastPageIndex, value + 1), "Next");
            nextButton.SetEnabled(value < LastPageIndex);
            this.Add(nextButton);
        }

        protected Button CreatePageButton(int index, string title = null)
        {
            Button pageButton = new Button();
            int displayIndex = index + 1;
            pageButton.text = string.IsNullOrWhiteSpace(title) ? displayIndex.ToString() : title;
            pageButton.clicked += () => { value = index; };
            pageButton.SetEnabled(value != index);
            return pageButton;
        }
    }
}