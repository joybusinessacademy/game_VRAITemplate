using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SVRHAND_THUMB_BUTTON
{
    DEFAULT,
    HOME,
    BACK,
    BUTTON_ONE,
    BUTTON_TWO
}

public class SVRHandsAnimInteractorSimple : MonoBehaviour
{
    public float speedMulti;
    public bool flipThumbJoyX;
    [SerializeField] private List<Vector2> blendCoordsThumbButton = new List<Vector2>(); 
    private Animator animator;
    private ThumbRoutineStruct thumbButtonY, thumbButtonX;

    struct ThumbRoutineStruct
    {
        public SVRHAND_THUMB_BUTTON type;
        public Coroutine refRoutine;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetTriggerFinger(float value)
    {
        animator.SetFloat("BlendIndexFinger", value);
    }

    public void SetGripFinger(float value)
    {
        animator.SetFloat("BlendMiddleFinger", value);
    }

    public void SetThumbButton(SVRHAND_THUMB_BUTTON buttonType)
    {
       
        animator.SetBool("ThumbAnalog", false);
        animator.SetBool("ThumbButtons", true);

        float startValue = animator.GetFloat("BlendThumbButtonsX");
        float targetValue = blendCoordsThumbButton[(int)buttonType].x;

        if (buttonType != thumbButtonX.type)
        {
            if (thumbButtonX.refRoutine != null)
                StopCoroutine(thumbButtonX.refRoutine);
            thumbButtonX.refRoutine = StartCoroutine(LerpBlendFloat(targetValue, startValue, "BlendThumbButtonsX", (startValue <= targetValue)));
            thumbButtonX.type = buttonType;
        }

        startValue = animator.GetFloat("BlendThumbButtonsY");
        targetValue = blendCoordsThumbButton[(int)buttonType].y;

        if (buttonType != thumbButtonY.type)
        {
            if (thumbButtonY.refRoutine != null)
                StopCoroutine(thumbButtonY.refRoutine);
            thumbButtonY.refRoutine = StartCoroutine(LerpBlendFloat(targetValue, startValue, "BlendThumbButtonsY", (startValue <= targetValue)));
            thumbButtonY.type = buttonType;
        }

    }

    public void SetThumbAnalog(float x, float y)
    {
       

        animator.SetBool("ThumbAnalog", true);
        animator.SetBool("ThumbButtons", false);

        animator.SetFloat("BlendThumbAnalogX", flipThumbJoyX ? -x : x);
        animator.SetFloat("BlendThumbAnalogY", y);
    }

    IEnumerator LerpBlendFloat(float targetValue, float startValue, string paramenterName, bool isPositive)
    {
        if (isPositive)
        {
            while (animator.GetFloat(paramenterName) < targetValue)
            {
                animator.SetFloat(paramenterName, startValue += Time.deltaTime * speedMulti);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (animator.GetFloat(paramenterName) > targetValue)
            {
                animator.SetFloat(paramenterName, startValue -= Time.deltaTime * speedMulti);
                yield return new WaitForEndOfFrame();
            }
        }

        animator.SetFloat(paramenterName, targetValue);
    }
}
