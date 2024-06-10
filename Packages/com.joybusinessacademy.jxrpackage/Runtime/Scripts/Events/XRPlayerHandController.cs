using JBA.XRPlayerPackage;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class XRPlayerHandController : MonoBehaviour
{
	public GameObject baseLeftController, pointerLeftController;
	public GameObject baseRightController, pointerRightController;

	private XRBaseInteractor pointerLeftControllerInteractor, pointerRightControllerInteractor;
	private XRController pointerLeftControllerXR, pointerRightControllerXR;
	private XRInteractorLineVisual leftLineVisual, rightLineVisual;

	public HandsAnimatorControl leftHandAnimator, rightHandAnimator;
	public Gradient onClickColor;
	private Gradient cachedClickColor;

	private void Awake()
	{
		pointerLeftControllerInteractor = pointerLeftController.GetComponent<XRBaseInteractor>();
		pointerRightControllerInteractor = pointerRightController.GetComponent<XRBaseInteractor>();

		pointerLeftControllerXR = pointerLeftController.GetComponentInParent<XRController>();
		pointerRightControllerXR = pointerRightController.GetComponentInParent<XRController>();

		leftLineVisual = pointerLeftControllerXR.GetComponent<XRInteractorLineVisual>();
		rightLineVisual = pointerRightControllerXR.GetComponent<XRInteractorLineVisual>();
		cachedClickColor = leftLineVisual.validColorGradient;
	}

	private IEnumerator Start()
	{
		yield return new WaitForEndOfFrame();

		SetPointerState();		
	}

	public void SetPointerState()
	{
		leftHandAnimator.SetPose(HandsAnimatorControl.HandPose.Point);
		rightHandAnimator.SetPose(HandsAnimatorControl.HandPose.Point);

		pointerLeftControllerInteractor.enabled = true;
		pointerRightControllerInteractor.enabled = true;
	}

	public void SetDefaultState()
	{
		pointerLeftControllerInteractor.enabled = false;
		pointerRightControllerInteractor.enabled = false;

		leftHandAnimator.SetPose(HandsAnimatorControl.HandPose.Default);
		rightHandAnimator.SetPose(HandsAnimatorControl.HandPose.Default);
	}

    private void Update()
    {
		SetRayColour();
    }

    private void SetRayColour()
    {
		if (pointerLeftControllerXR.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerTargetL))
		{
			leftLineVisual.validColorGradient = triggerTargetL > 0 ? onClickColor : cachedClickColor;
		}

		if (pointerRightControllerXR.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerTargetR))
		{
			rightLineVisual.validColorGradient = triggerTargetR > 0 ? onClickColor : cachedClickColor;
		}

#if UNITY_EDITOR
		leftLineVisual.validColorGradient = Input.GetKey(KeyCode.Space) ? onClickColor : cachedClickColor;
		rightLineVisual.validColorGradient = Input.GetKey(KeyCode.Space) ? onClickColor : cachedClickColor;
#endif

	}
}
