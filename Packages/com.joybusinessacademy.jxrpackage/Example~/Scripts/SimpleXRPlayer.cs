using JBA.XRPlayerPackage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBA.XRPlayerPackageexamples
{
    public class SimpleXRPlayer : BaseXRPlayer
    {
        public void SetPointHand()
        {
            // on the example, this function is called on wrapper command onhover which is called inside forloop xrinternaction manager
            // we are not suppose to modify any contents on interaction on current frame hence we do it next frame
            StartCoroutine(DoCommandNextFrame(() =>
            {
                ControllerManager.SetController(0, ControllerManager.ControllerStates.Point);
                ControllerManager.SetController(1, ControllerManager.ControllerStates.Point);
            }));
        }

        public void SetDefaultHand()
        {
            // on the example, this function is called on wrapper command onhover which is called inside forloop xrinternaction manager
            // we are not suppose to modify any contents on interaction on current frame hence we do it next frame
            StartCoroutine(DoCommandNextFrame(() =>
            {
                ControllerManager.SetController(0, ControllerManager.ControllerStates.Select);
                ControllerManager.SetController(1, ControllerManager.ControllerStates.Select);
            }));
        }

        private IEnumerator DoCommandNextFrame(System.Action callback)
        {
            yield return new WaitForEndOfFrame();
            callback();
        }

    }
}
