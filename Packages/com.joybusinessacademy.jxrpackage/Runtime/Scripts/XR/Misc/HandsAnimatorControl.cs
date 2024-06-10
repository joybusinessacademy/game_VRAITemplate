#if PHOTON_UNITY_NETWORKING && PUN_2_OR_NEWER
using Photon.Pun;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBA.XRPlayerPackage
{
	public class HandsAnimatorControl : MonoBehaviour
	{
		public enum HandPose { Default, Fist, Point, Metal, ThumbsUp, Spread, HoldFlat, HoldComms }

		[SerializeField] private HandPose currentToPose;
		private Animator animator;

		public string inputControl;

		private Dictionary<HandPose, System.Action> handPoseOnUpdateFunctions = new Dictionary<HandPose, System.Action>();
		private float polledPose = 0;

		private float networkedInput = 0;
#if UNITY_EDITOR
		private void TestHandPoseEditor(HandPose pose)
		{
			SetPose(pose);
		}
#endif

#if PHOTON_UNITY_NETWORKING && PUN_2_OR_NEWER
		private PhotonView photonView;
#endif

		public bool IsPlayerAndLocal { get; set; } = true;
		protected void Awake()
		{

            animator = GetComponent<Animator>();

            handPoseOnUpdateFunctions.Add(HandPose.Default, FistPoseUpdateFunction);
            handPoseOnUpdateFunctions.Add(HandPose.Fist, FistPoseUpdateFunction);
			handPoseOnUpdateFunctions.Add(HandPose.Point, PoseUpdateFunction);
			handPoseOnUpdateFunctions.Add(HandPose.HoldFlat, PoseUpdateFunction);
			handPoseOnUpdateFunctions.Add(HandPose.HoldComms, PoseUpdateFunction);
			handPoseOnUpdateFunctions.Add(HandPose.ThumbsUp, PoseUpdateFunction);
			handPoseOnUpdateFunctions.Add(HandPose.Spread, PoseUpdateFunction);

			animator.SetFloat("Pose", (int)currentToPose);

#if PHOTON_UNITY_NETWORKING && PUN_2_OR_NEWER
			photonView = GetComponent<PhotonView>();
            IsPlayerAndLocal =  PhotonNetwork.IsConnected == false || PhotonNetwork.OfflineMode || photonView.IsMine;
#endif
        }

		private void FistPoseUpdateFunction()
		{
			float input = IsPlayerAndLocal ? Input.GetAxis(inputControl) : networkedInput;
			animator.SetFloat("Trigger", input);

		}

		private void PoseUpdateFunction()
		{
			polledPose += Time.deltaTime * 3;
			polledPose = Mathf.Clamp01(polledPose);
			animator.SetFloat("Trigger", polledPose);
		}

		protected void Update()
		{
			if (handPoseOnUpdateFunctions.ContainsKey(currentToPose))
				handPoseOnUpdateFunctions[currentToPose]?.Invoke();


			if (IsPlayerAndLocal)
			{
				var input = Input.GetAxis(inputControl);
				if(input != networkedInput)
				{
					networkedInput = input;
#if PHOTON_UNITY_NETWORKING && PUN_2_OR_NEWER
					if (IsPlayerAndLocal && PhotonNetwork.IsConnected && photonView != null)
					{
						photonView.RPC(nameof(RPC_SetInputControlData), RpcTarget.Others, networkedInput);
					}
#endif
				}
			}
		}

		public HandPose GetPose()
		{
			return currentToPose;
		}

		public void SetPose(HandPose targetPose)
		{
			if (targetPose == this.currentToPose)
				return;

			polledPose = 0;
			this.currentToPose = targetPose;
			animator.SetFloat("Pose", (int)targetPose);

#if PHOTON_UNITY_NETWORKING && PUN_2_OR_NEWER
			if (IsPlayerAndLocal && PhotonNetwork.IsConnected && photonView != null)
			{
				Debug.Log("Sync Pose: " + (int)targetPose);
				string handPoseJson = JsonUtility.ToJson(targetPose);
				photonView.RPC(nameof(RPC_SetPostData), RpcTarget.Others, new object[] { handPoseJson });
			}
#endif
		}

#if PHOTON_UNITY_NETWORKING && PUN_2_OR_NEWER
		[PunRPC]
		private void RPC_SetPostData(string jsonPoseData)
		{
			HandPose pose = JsonUtility.FromJson<HandPose>(jsonPoseData);

			SetPose(pose);
		}

		[PunRPC]
		private void RPC_SetInputControlData(float input)
		{
			networkedInput = input;
		}
#endif

	}
}