using UnityEngine;

namespace JBA.XRPlayerPackage.XRDebug
{
    public class DebugCameraControl : MonoBehaviour
    {
        private Vector3 currentPlayerPosition;
        private float movementSpeed;
        
        public GameObject vrPlayer;
        public GameObject headPivot;
        public Transform hands;
        
        public float initialMovementSpeed = 1.5f;
        public float accelerationIncrement = 0.025f;
        public float maxMovementSpeed = 10;
        
        public KeyCode cameraKey = KeyCode.Mouse1;
        public KeyCode accelerationKey = KeyCode.LeftShift;

        private void Awake()
        {
            movementSpeed = initialMovementSpeed;
        }

        private void OnEnable()
        {
            currentPlayerPosition = vrPlayer.transform.position;
        }

        private void MoveCharacter()
        {
            float forwardKey = 0;
            float sidewaysKey = 0;
            float upwardKey = 0;

            if (Application.platform != RuntimePlatform.Android)
            {
                forwardKey = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
                sidewaysKey = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
                upwardKey = Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0;

                if (forwardKey < -0.9f || forwardKey > 0.9f || sidewaysKey < -0.9f || sidewaysKey > 0.9f || upwardKey < -0.9f || upwardKey > 0.9f)
                { // if we are moving at all, check shift key to accelerate
                    if (Input.GetKey(accelerationKey))
                    {
                        movementSpeed = (movementSpeed + accelerationIncrement) > maxMovementSpeed ? movementSpeed : movementSpeed + accelerationIncrement;
                    }
                    else
                    {
                        movementSpeed = initialMovementSpeed;
                    }
                }
            }
            else
            {
                forwardKey = Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickVertical");
                sidewaysKey = Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickHorizontal");
                upwardKey = Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical");
            }

            if (forwardKey < -0.9f || forwardKey > 0.9f || sidewaysKey < -0.9f || sidewaysKey > 0.9f || upwardKey < -0.9f || upwardKey > 0.9f)
            { // if we are moving at all, check shift key to accelerate
                if (Input.GetKey(accelerationKey) || Input.GetButton("Oculus_CrossPlatform_PrimaryThumbstick"))
                {
                    movementSpeed = (movementSpeed + accelerationIncrement) > maxMovementSpeed ? movementSpeed : movementSpeed + accelerationIncrement;
                }
                else
                {
                    movementSpeed = initialMovementSpeed;
                }
            }

            Vector3 fwdMove = (headPivot.transform.rotation * Vector3.forward) * forwardKey * Time.deltaTime * movementSpeed;
            Vector3 strafeMove = (headPivot.transform.rotation * Vector3.right) * sidewaysKey * Time.deltaTime * movementSpeed;
            Vector3 upwdMove = (headPivot.transform.rotation * Vector3.up) * upwardKey * Time.deltaTime * movementSpeed;

            vrPlayer.transform.position += fwdMove + strafeMove + upwdMove;
        }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        public void Update()
        {
            MoveCharacter();

            // Move Camera
            {
                float deltaX;
                float deltaY;

                if (Application.platform == RuntimePlatform.Android)
                {
                    deltaX = Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickHorizontal");
                    vrPlayer.transform.eulerAngles += new Vector3(0, deltaX, 0);
                    return;
                }

                if (Input.GetKey(cameraKey))
                {
                    deltaX = Input.GetAxis("Mouse X");
                    deltaY = Input.GetAxis("Mouse Y");

                    //xrRig.cameraFloorOffsetObject.transform.eulerAngles += new Vector3(-deltaY, 0);
                    headPivot.transform.eulerAngles += new Vector3(-deltaY, deltaX, 0);

                    if (hands)
                        hands.transform.eulerAngles = headPivot.transform.eulerAngles;
                }
            }

        }
#endif
    }
}

