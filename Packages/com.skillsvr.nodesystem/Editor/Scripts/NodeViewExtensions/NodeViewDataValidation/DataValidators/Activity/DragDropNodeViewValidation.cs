using Props;
using Props.PropInterfaces;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;


namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
    [CustomDataValidation(typeof(DragDropNodeView))]
    public class DragDropNodeViewValidation : AbstractNodeViewValidation<DragDropNodeView>
    {
        public override VisualElement OnGetVisualSourceFromPath(string path)
        {


            return null;
        }

        public override void OnValidate()
        {
            var node = TargetNodeView.AttachedNode<DragDropNode>();

            string socketDataKey= "Socket Props Issue";
            string socketDataPropFilterKey = "Socket Props Filter Issue";

            string ObjectDataKey = "Interactable Props Issue";
            string ObjectDataPropFilterKey = "Interactable Props Filter Issue";
            string completionTitle = "Completion Issue";

            ErrorIf(0 == node.interactableObjectDatas.Count, ObjectDataKey, "Reference at least one Interactable Prop.");

            List<PropGUID<IPropGrabInteractable>> doubleRefTest = new List<PropGUID<IPropGrabInteractable>>();
            bool doubleRefFailed = false;

            foreach (var item in node.objectInteractableInterfaces)
            {
                if (PropManager.GetProp<InteractableProp>(item.propGUID) == null)
                 {
                    ErrorIf(true, ObjectDataKey, "Ensure all referenced interactable prop entries are targeting an interactable in the scene.");
                }

                if (doubleRefTest.Contains(item))
                    doubleRefFailed = true;
                else
                    doubleRefTest.Add(item);
            }

            ErrorIf(doubleRefFailed, ObjectDataKey, "You have referenced the same prop twice. This will create unwated behavior.");



            foreach (var item in node.interactableObjectDatas)
            {
                if(item.tag == null)
                {
                    ErrorIf(true, ObjectDataPropFilterKey, "Ensure all referenced interactable prop entries are using a valid filter layer.");
                }
            }


            ErrorIf(0 == node.interactorSocketDatas.Count, socketDataKey, "Reference at least one socket prop.");

            List<PropGUID<IPropSocketInteractor>> doubleRefTestSocket = new List<PropGUID<IPropSocketInteractor>>();
            doubleRefFailed = false;

            foreach (var item in node.socketInteractorInterfaces)
            {
                if (PropManager.GetProp<SocketProp>(item.propGUID) == null)
                {
                    ErrorIf(true, socketDataKey, "Ensure all referenced Socket Prop entries are targeting socket in the scene.");
                }

                if (doubleRefTestSocket.Contains(item))
                    doubleRefFailed = true;
                else
                    doubleRefTestSocket.Add(item);
            }

            bool atleastOneCorrect = false;
            foreach (var item in node.interactorSocketDatas)
            {
                if(item.tagsFiltered.Count == 0)
                {
                    ErrorIf(true, socketDataPropFilterKey, "Ensure all referenced Socket Prop entries are using a valid filter layer");
                }

                foreach (var filtered in item.tagsFiltered)
                {
                    if(filtered.isCorrect)
                        atleastOneCorrect = true;
                }
                ErrorIf((!atleastOneCorrect && (node.currentSelectedCompletionType == 3 || node.correctOnlyInOrder)), socketDataKey, "Ensure at least one referenced Socket Prop entry is tagged as correct.");
                atleastOneCorrect = false;
            }

            ErrorIf(doubleRefFailed, socketDataPropFilterKey, "You have referenced the same socket twice. This will create unwated behavior.");
            
          


            //completion
            if (node.currentSelectedCompletionType == 0)
            {
                WarningIf(node.timer == 0, completionTitle, "Timer is set to zero.");
            }else if(node.currentSelectedCompletionType == 2 || node.currentSelectedCompletionType == 3)
            {
                ErrorIf((node.socketInteractorInterfaces.Count > node.objectInteractableInterfaces.Count), completionTitle, "There is not enough interactable objects to complete all sockets.");
            }

        }
    }
}