using System;
using System.Collections;
using System.Collections.Generic;

namespace VRMechanics.Mechanics.GestureDetection
{
	public enum DetectionEvents
	{
		None,
		StartDetect,
		StopDetect,
		AlreadyStart,
		NotStart,
		StateChanged,
		BeforeDetect,
		OnDetect,
	}

	public interface IGestureDetector
	{
		bool isRunning { get; }

		bool Detect(IGestureBody one, IGestureBody other);
		
		IEnumerator StartDetectCoroutine(IGestureBody one, IGestureBody other, Action<DetectionEvents, bool> eventCallback, float timeout = -1, float detectionInterval = 0.2F);
		void StopDetectCoroutine();
		IEnumerable<GestureDetectStepData> GetAllDetectStepData();
	}
}