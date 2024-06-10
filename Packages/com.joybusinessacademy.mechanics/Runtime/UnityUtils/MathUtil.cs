using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SkillsVR.UnityExtenstion
{
	public static class MathUtil
	{
		#region Directions

		/// <summary>
		/// Calculate if <paramref name="targetPosition"/> is in FRONT of <paramref name="originTargetPosition"/> relative to <paramref name="relativeTransform"./>
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="originTargetPosition"></param>
		/// <param name="relativeTransform"></param>
		/// <returns></returns>
		public static bool IsInFront(Vector3 targetPosition, Vector3 originTargetPosition, Transform relativeTransform)
		{
			return AngleDirection(targetPosition, originTargetPosition, -relativeTransform.right, relativeTransform.up) > 0;
		}

		/// <summary>
		/// Calculate if <paramref name="targetPosition"/> is ABOVE <paramref name="originTargetPosition"/> relative to <paramref name="relativeTransform"./>
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="originTargetPosition"></param>
		/// <param name="relativeTransform"></param>
		/// <returns></returns>
		public static bool IsAbove(Vector3 targetPosition, Vector3 originTargetPosition, Transform relativeTransform)
		{
			return AngleDirection(targetPosition, originTargetPosition, relativeTransform.forward, -relativeTransform.right) > 0;
		}

		/// <summary>
		/// Calculate if <paramref name="targetPosition"/> is to the RIGHT of <paramref name="originTargetPosition"/> relative to <paramref name="relativeTransform"./>
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="originTargetPosition"></param>
		/// <param name="relativeTransform"></param>
		/// <returns></returns>
		public static bool IsToTheRight(Vector3 targetPosition, Vector3 originTargetPosition, Transform relativeTransform)
		{
			return AngleDirection(targetPosition, originTargetPosition, relativeTransform.forward, relativeTransform.up) > 0;
		}

		/// <summary>
		/// Calculate if <paramref name="targetPosition"/> is to the LEFT of <paramref name="originTargetPosition"/> relative to <paramref name="relativeTransform"./>
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="originTargetPosition"></param>
		/// <param name="relativeTransform"></param>
		/// <returns></returns>
		public static bool IsToTheLeft(Vector3 targetPosition, Vector3 originTargetPosition, Transform relativeTransform)
		{
			return !IsToTheRight(targetPosition, originTargetPosition, relativeTransform);
		}

		/// <summary>
		/// Calculate if <paramref name="targetPosition"/> is BEHIND <paramref name="originTargetPosition"/> relative to <paramref name="relativeTransform"./>
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="originTargetPosition"></param>
		/// <param name="relativeTransform"></param>
		/// <returns></returns>
		public static bool IsBehind(Vector3 targetPosition, Vector3 originTargetPosition, Transform relativeTransform)
		{
			return !IsInFront(targetPosition, originTargetPosition, relativeTransform);
		}

		/// <summary>
		/// Calculate if <paramref name="targetPosition"/> is BELOW <paramref name="originTargetPosition"/> relative to <paramref name="relativeTransform"./>
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="originTargetPosition"></param>
		/// <param name="relativeTransform"></param>
		/// <returns></returns>
		public static bool IsBelow(Vector3 targetPosition, Vector3 originTargetPosition, Transform relativeTransform)
		{
			return !IsAbove(targetPosition, originTargetPosition, relativeTransform);
		}

		private static float AngleDirection(Vector3 targetPosition, Vector3 originTargetPosition, Vector3 forward, Vector3 up)
		{
			var targetDirection = targetPosition - originTargetPosition;

			Vector3 perpendicular = Vector3.Cross(forward, targetDirection);

			return Vector3.Dot(perpendicular, up);
		}

		#endregion

		/// <summary>
		/// Returns true if given number is approximately 0.
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static bool ApproxZero(float number)
		{
			return Mathf.Approximately(number, 0);
		}

		/// <summary>
		/// Like Mathf.Repeat but allows an alternative start index.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="length"></param>
		/// <param name="startIndex"></param>
		/// <returns></returns>
		public static int Repeat(int value, int length, int startIndex = 0)
		{
			// Make sure length >= 1.
			length = Mathf.Max(length, 1);

			return (int)Mathf.Repeat(value - (startIndex % length), length) + startIndex;
		}

		/// <summary>
		/// Like Mathf.Repeat but allows a specific range.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="upper"></param>
		/// <param name="lower"></param>
		/// <returns></returns>
		public static int RepeatRange(int value, int upper, int lower = 0)
		{
			return MathUtil.Repeat(value, Mathf.Abs(upper - lower), Mathf.Min(lower, upper));
		}

		public static void RoundToDecimal(ref Vector2 source, int decimals)
		{
			source.Set((float)Math.Round(source.x, decimals), (float)Math.Round(source.y, decimals));
		}

		public static void RoundToDecimal(ref float source, int decimals)
		{
			source = (float)Math.Round(source, decimals);
		}

		public static void RoundToInteger(ref Vector2 source)
		{
			source.Set((float)Math.Round(source.x, 0), (float)Math.Round(source.y, 0));
		}

		/// <summary>
		/// Checks if 2 <see cref="Vector2"/> are equal, within the supplied tolerance value
		/// </summary>
		/// <param name="vector1"></param>
		/// <param name="vector2"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public static bool CheckEquality(Vector2 vector1, Vector2 vector2, float tolerance = Vector2.kEpsilon)
		{
			return Math.Abs(vector1.x - vector2.x) <= tolerance && Math.Abs(vector1.y - vector2.y) <= tolerance;
		}

		public static bool IsEven(int value)
		{
			return value % 2 == 0;
		}

		public static bool IsOdd(int value)
		{
			return !IsEven(value);
		}

		/// <summary>
		/// Checks if 2 <see cref="Vector3"/> are equal, within the supplied tolerance value
		/// </summary>
		/// <param name="vector1"></param>
		/// <param name="vector2"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public static bool CheckEquality(Vector3 vector1, Vector3 vector2, float tolerance = Vector3.kEpsilon)
		{
			return Math.Abs(vector1.x - vector2.x) <= tolerance &&
				   Math.Abs(vector1.y - vector2.y) <= tolerance &&
				   Math.Abs(vector1.z - vector2.z) <= tolerance;
		}

		/// <summary>
		/// Checks if 2 <see cref="float"/> are equal, within the supplied tolerance value
		/// </summary>
		/// <param name="float1"></param>
		/// <param name="float2"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public static bool CheckEquality(float float1, float float2, float tolerance = float.Epsilon)
		{
			return Math.Abs(float1 - float2) <= tolerance;
		}

		/// <summary>
		/// Takes a value within a supplied range and outputs the equivalent value based on a targetRange
		/// </summary>
		/// <param name="value"></param>
		/// <param name="valueRangeMin"></param>
		/// <param name="valueRangeMax"></param>
		/// <param name="targetRangeMin"></param>
		/// <param name="targetRangeMax"></param>
		/// <returns></returns>
		public static float ConvertValueRange(float value, float valueRangeMin, float valueRangeMax, float targetRangeMin, float targetRangeMax, bool clamp = false)
		{
			if (clamp)
				value = Mathf.Clamp(value, valueRangeMin, valueRangeMax);

			return (value - valueRangeMin) / (valueRangeMax - valueRangeMin) * (targetRangeMax - targetRangeMin) + targetRangeMin;
		}

		/// <summary>
		/// Checks if a supplied value is within the supplied min/max range
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static bool InRange(int value, int min, int max)
		{
			return (value >= min) && (value <= max);
		}

		/// <summary>
		/// Checks if a supplied value is within the supplied min/max range
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static bool Contains(this Vector2 vector, float value)
		{
			return (vector.x <= value) && (value <= vector.y);
		}

		/// <summary>
		/// Checks if a supplied value is within the supplied min/max range
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static bool InRange(float value, float min, float max)
		{
			return (value >= min) && (value <= max);
		}

		public static Vector3 ConvertRawInputToCameraRelative(Vector3 vector, Vector3 orient)
		{
			var input = Vector3.ClampMagnitude(vector, 1f);
			input = Quaternion.Euler(orient) * input;

			input = Vector3.ProjectOnPlane(input, Vector3.up).normalized;

			return input;
		}

		public static Vector3 ConvertRawInputToCameraRelative(Vector3 vector, Transform relativity)
		{
			var input = Vector3.ClampMagnitude(vector, 1f);
			input = relativity.TransformDirection(input);

			input = Vector3.ProjectOnPlane(input, Vector3.up).normalized;

			return input;
		}

		public static Vector3 RotateAround(Vector3 target, Vector3 pivot, Vector3 axis, float delta)
		{

			// Convert theta to distance.

			return Vector3.zero;
		}

		public static bool Vector3Approx(Vector3 a, Vector3 b)
		{
			return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
		}

		public static Transform ClosestTransform(List<Transform> points, Transform target)
		{
			Transform tMin = null;
			float minDist = Mathf.Infinity;
			Vector3 currentPos = target.position;
			foreach (Transform t in points)
			{
				float dist = Vector3.Distance(t.position, currentPos);
				if (dist < minDist)
				{
					tMin = t;
					minDist = dist;
				}
			}
			return tMin;
		}

		public static Vector3 GetClosestPoint(Vector3 point, List<Vector3> points)
		{
			return points.First(p => CheckEquality((p - point).magnitude, points.Min(pp => (pp - point).magnitude)));
		}

		public static Vector3 GetClosestPoint(Vector3 point, params Vector3[] points)
		{
			return points.First(p => CheckEquality((p - point).magnitude, points.Min(pp => (pp - point).magnitude)));
		}

		public static Vector3 GetRandomPositionWithinCollider(BoxCollider boxCollider)
		{
			float startX = -(boxCollider.size.x / 2.0f) + boxCollider.center.x;
			float startY = -(boxCollider.size.y / 2.0f) + boxCollider.center.y;
			float startZ = -(boxCollider.size.z / 2.0f) + boxCollider.center.z;

			float xSize = boxCollider.size.x;
			float ySize = boxCollider.size.y;
			float zSize = boxCollider.size.z;

			float x = UnityEngine.Random.Range(startX, xSize);
			float y = UnityEngine.Random.Range(startY, ySize);
			float z = UnityEngine.Random.Range(startZ, zSize);

			return boxCollider.transform.position + new Vector3(x, y, z);
		}

		static public float WrapAngle(float angle)
		{
			while (angle > 180f) angle -= 360f;
			while (angle < -180f) angle += 360f;
			return angle;
		}

		public static Vector2 ScreenPointFromAngle(Vector2 aDirection)
		{
			aDirection.Normalize();
			var e = new Vector3(Screen.width / 2f, Screen.height / 2f);
			var v = aDirection;
			float y = e.x * v.y / v.x;

			if (Mathf.Abs(y) < e.y)
				return new Vector2(e.x, y);

			return new Vector2(e.y * v.x / v.y, e.y);
		}

		public static Vector2 PointOnBounds(Bounds bounds, Vector2 aDirection)
		{
			aDirection.Normalize();
			var e = bounds.extents;
			var v = aDirection;
			float y = e.x * v.y / v.x;
			if (Mathf.Abs(y) < e.y)
				return new Vector2(e.x, y);
			return new Vector2(e.y * v.x / v.y, e.y);
		}

		public static Vector2 PointOnBounds(Bounds bounds, float aAngle)
		{
			float a = aAngle * Mathf.Deg2Rad;
			return PointOnBounds(bounds, new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
		}

		public static Vector3 WorldToScreenPointClamped(this Camera camera, Vector3 position, float indicatorOffscreenBorder = 0.1f)
		{
			// First, get the screen pos of our world object.
			Vector3 screenPos = camera.WorldToScreenPoint(position);

			if (camera.IsInView(position))
				return screenPos;

			// Flip if indicator is behind us
			if (screenPos.z < 0f)
			{
				screenPos.x = Screen.width - screenPos.x;
				screenPos.y = Screen.height - screenPos.y;
			}

			// Calculate off-screen position/rotation.
			Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0f) / 2f;
			screenPos -= screenCenter;
			float angle = Mathf.Atan2(screenPos.y, screenPos.x);
			angle -= 90f * Mathf.Deg2Rad;
			float cos = Mathf.Cos(angle);
			float sin = -Mathf.Sin(angle);
			float cotangent = cos / sin;
			screenPos = screenCenter + new Vector3(sin * 50f, cos * 50f, 0f);

			// Is indicator inside the defined bounds?
			float offset = Mathf.Min(screenCenter.x, screenCenter.y);
			offset = Mathf.Lerp(0f, offset, indicatorOffscreenBorder);
			Vector3 screenBounds = screenCenter - new Vector3(offset, offset, 0f);
			float boundsY = (cos > 0f) ? screenBounds.y : -screenBounds.y;
			screenPos = new Vector3(boundsY / cotangent, boundsY, 0f);

			// When out of bounds, get point on appropriate side.
			if (screenPos.x > screenBounds.x) // out => right
				screenPos = new Vector3(screenBounds.x, screenBounds.x * cotangent, 0f);
			else if (screenPos.x < -screenBounds.x) // out => left
				screenPos = new Vector3(-screenBounds.x, -screenBounds.x * cotangent, 0f);
			screenPos += screenCenter;

			return screenPos;
		}

		public static Vector3 GetScreenPositionBorder(Camera camera, Vector3 position, RectTransform parentRect)
		{
			position = camera.WorldToScreenPointClamped(position, 0.1f);
			return new Vector3(position.x + parentRect.localPosition.x, position.y + parentRect.localPosition.y, 0f);
		}

		public static Vector3 WorldToScreenClamped(this Camera cam, Vector3 pos, float xEdge = 0, float yEdge = 0)
		{
			pos = cam.WorldToViewportPoint(pos);

			if (pos.z < 0)
			{
				pos.x = 1f - pos.x;
				pos.y = 1f - pos.y;
				pos.z = 0;
				pos = pos.Maximise();
			}

			pos = cam.ViewportToScreenPoint(pos);

			var center = new Vector3(Screen.width / 2f, Screen.height / 2f);
			var dir = (pos - center).magnitude;

			pos.x = Mathf.Clamp(pos.x, xEdge, Screen.width - xEdge);
			pos.y = Mathf.Clamp(pos.y, yEdge, Screen.height - yEdge);

			return pos;
		}

		public static Vector3 Maximise(this Vector3 vector)
		{
			Vector3 returnVector = vector;

			float max = 0;
			max = vector.x > max ? vector.x : max;
			max = vector.y > max ? vector.y : max;
			max = vector.z > max ? vector.z : max;

			returnVector /= max;

			return returnVector;
		}

		public static bool IsInView(this Camera cam, Vector3 p, float offscreenIndicator = 0f)
		{
			return cam.IsInView(p,
				new Vector2(offscreenIndicator, 1f - offscreenIndicator),
				new Vector2(offscreenIndicator, 1f - offscreenIndicator));
		}

		public static bool IsInView(this Camera cam, Vector3 p, Vector2 xRange, Vector2 yRange)
		{
			return IsInView(p, cam, xRange, yRange);
		}

		public static bool IsInView(this Camera cam, Transform t, Vector2 xRange, Vector2 yRange)
		{
			return IsInView(t, cam, xRange, yRange);
		}

		public static bool IsInView(Vector3 p, Camera cam, Vector2 xRange, Vector2 yRange)
		{
			var vector = cam.WorldToViewportPoint(p);
			return (vector.z > 0 && IsInRange(vector.x, xRange.x, xRange.y) && IsInRange(vector.y, yRange.x, yRange.y));
		}

		public static bool IsInView(Transform t, Camera cam, Vector2 xRange, Vector2 yRange)
		{
			return IsInView(t.position, cam, xRange, yRange);
		}

		public static bool IsInRange01(float num, bool inclusive = true)
		{
			return IsInRange(num, 0, 1);
		}

		public static bool IsInRange(float num, float min, float max, bool inclusive = true)
		{
			if (inclusive)
				return num >= min && num <= max;
			else
				return num > min && num < max;
		}

		public static Vector3 ClosestPointOnPath(Vector3 pos, List<Vector3> chunkPath)
		{
			List<Vector3> closestPointInLines = new List<Vector3>();

			for (int i = 0; i < chunkPath.Count - 1; i++)
				closestPointInLines.Add(ClosestPointOnLine(chunkPath[i], chunkPath[i + 1], pos));

			int indexFound = -1;
			return GetClosestIndexedPointOnPath(pos, closestPointInLines, ref indexFound);
		}

		/// <summary>
		/// get closest point from an array of points
		/// </summary>
		public static Vector3 GetClosestIndexedPointOnPath(Vector3 posEntity, List<Vector3> points, ref int indexFound)
		{
			float sqrDist = 0;
			indexFound = -1;

			int firstIndex = 0;

			for (int i = 0; i < points.Count; i++)
			{
				float dist = (posEntity - points[i]).sqrMagnitude;
				if (firstIndex == 0)
				{
					indexFound = i;
					sqrDist = dist;
				}
				else if (dist < sqrDist)
				{
					sqrDist = dist;
					indexFound = i;
				}
				firstIndex++;
			}

			return (points[indexFound]);
		}

		public static float GetPercentageFromPosToChunkPath(Vector3 pos, List<Vector3> chunkPath)
		{
			List<Vector3> closestPointInLines = new List<Vector3>();

			for (int i = 0; i < chunkPath.Count - 1; i++)
				closestPointInLines.Add(ClosestPointOnLine(chunkPath[i], chunkPath[i + 1], pos));

			int indexFound = -1;
			Vector3 closestPoint = GetClosestIndexedPointOnPath(pos, closestPointInLines, ref indexFound);

			var pathSubset = chunkPath.GetRange(indexFound + 1, chunkPath.Count - (indexFound + 1));
			pathSubset.Insert(0, closestPoint);

			var d1 = GetPathLength(pathSubset);
			var d2 = GetPathLength(chunkPath);

			//Debug.Log("index: " + indexFound + "  |  normalized: " + (d1/d2).ToDp(1) + "  |  distance 1: " + d1.ToDp(1) + " / distance 2: " + d2.ToDp(1));

			return d1 / d2;
		}

		public static float GetPathLength(List<Vector3> points)
		{
			float d = 0;

			for (int i = 1; i < points.Count; i++)
				d += Vector3.Distance(points[i - 1], points[i]);

			return d;
		}

		public static Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
		{
			var vVector1 = vPoint - vA;
			var vVector2 = (vB - vA).normalized;

			var d = Vector3.Distance(vA, vB);
			var t = Vector3.Dot(vVector2, vVector1);

			if (t <= 0)
				return vA;

			if (t >= d)
				return vB;

			var vVector3 = vVector2 * t;

			var vClosestPoint = vA + vVector3;

			return vClosestPoint;
		}

		public static Vector3 NearestPointOnLine(Vector3 lineStart, Vector3 lineDir, Vector3 point)
		{
			// Get vector from point on line to point in space
			Vector3 startToPoint = point - lineStart;
			float t = Vector3.Dot(startToPoint, lineDir);

			return lineStart + lineDir * t;
		}


		/// <summary>
		/// [Use Euler angles]
		/// Returns a bool when in range (inclusive)
		/// </summary>
		public static bool IsRotatedWithinInRange(float target, float input, float range)
		{
			bool isWithinRange = false;

			// Get the range on the left and right sides off the range
			float rightRange = target + range;
			float leftRange = target - range;

			// When the checking range is greater than 360 and 0
			if (rightRange >= 360 || leftRange < 0)
			{
				rightRange = WrapAngle(rightRange);
				leftRange = WrapAngle(leftRange);
				input = WrapAngle(input);
			}

			if ((input >= leftRange && input <= rightRange))
			{
				isWithinRange = true;
			}

			return isWithinRange;
		}

		/// <summary>
		/// [Use Euler angles]
		/// Returns a bool when in range (inclusive)
		/// </summary>
		public static bool IsRotatedWithinInRange(float target, float input, float maxRange, float minRange)
		{

			bool isWithinRange = false;

			// Get the range on the left and right sides off the range
			float rightRange = target + maxRange;
			float leftRange = target - minRange;


			// When the checking range is greater than 360 and 0
			if (rightRange >= 360 || leftRange < 0)
			{
				rightRange = rightRange >= 360 ? rightRange -= 360 : rightRange;
				leftRange = leftRange < 0 ? leftRange += 360 : leftRange;
			}

			if (input >= leftRange && input <= rightRange)
			{
				isWithinRange = true;
			}

			return isWithinRange;
		}


		/*
		 * Checks if a object is parked in a space using 2D coordinates
		 */
		public static bool IsObjectInBounds3DTo2D(Collider boundingCollider, Collider TestCollider)
		{
			bool isInBounds = false;

			if (TestCollider.bounds.max.x <= boundingCollider.bounds.max.x && TestCollider.bounds.min.x >= boundingCollider.bounds.min.x)
			{
				if (TestCollider.bounds.max.z <= boundingCollider.bounds.max.z && TestCollider.bounds.min.z >= boundingCollider.bounds.min.z)
				{
					isInBounds = true;
				}
			}

			return isInBounds;
		}

		/*
	 * Checks if a object is parked in a space using 3D coordinates
	 */
		public static bool IsObjectInBounds3D(Collider boundingCollider, Collider TestCollider)
		{
			bool isInBounds = false;

			if (TestCollider.bounds.max.x <= boundingCollider.bounds.max.x && TestCollider.bounds.min.x >= boundingCollider.bounds.min.x)
			{
				if (TestCollider.bounds.max.y <= boundingCollider.bounds.max.y && TestCollider.bounds.min.y >= boundingCollider.bounds.min.y)
				{
					if (TestCollider.bounds.max.z <= boundingCollider.bounds.max.z && TestCollider.bounds.min.z >= boundingCollider.bounds.min.z)
					{
						isInBounds = true;
					}
				}
			}

			return isInBounds;
		}

		/// <summary>
		/// Maps a values bewteen a given range (Inculsive)
		/// </summary>
		/// <param name="value"></param>
		/// <param name="inputMin"></param>
		/// <param name="inputMax"></param>
		/// <param name="outputMin"></param>
		/// <param name="outputMax"></param>
		/// <returns></returns>
		public static double Map(double value, double inputMin, double inputMax, double outputMin, double outputMax)
			=> outputMin + (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin);

		public static Vector3 ProjectVector(Vector3 grabPoint, Transform transform)
		{
			return ProjectPointOnPlane(transform.forward, transform.position, grabPoint);
		}

		public static Vector3 GetNearestPointToArc(Vector3 grabPoint, Transform transform, float arcRadius)
		{
			var projectedVector = ProjectVector(grabPoint, transform);

			return transform.position + (projectedVector - transform.position).normalized * arcRadius;
		}

		public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
		{
			float distance;
			Vector3 translationVector;
			//First calculate the distance from the point to the plane:
			distance = SignedDistancePlanePoint(planeNormal, planePoint, point);
			//Reverse the sign of the distance
			distance *= -1;
			//Get a translation vector
			translationVector = SetVectorLength(planeNormal, distance);
			//Translate the point to form a projection
			return point + translationVector;
		}
		//Get the shortest distance between a point and a plane. The output is signed so it holds information
		//as to which side of the plane normal the point is.
		public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
		{
			return Vector3.Dot(planeNormal, (point - planePoint));
		}
		//create a vector of direction "vector" with length "size"
		public static Vector3 SetVectorLength(Vector3 vector, float size)
		{
			//normalize the vector
			Vector3 vectorNormalized = Vector3.Normalize(vector);
			//scale the vector
			return vectorNormalized *= size;
		}

		public static float AngleOffAroundAxis(Vector3 v, Vector3 forward, Vector3 axis)
		{
			Vector3 right = Vector3.Cross(forward, axis);
			forward = Vector3.Cross(axis, right);

			return Mathf.Atan2(Vector3.Dot(v, right), Vector3.Dot(v, forward)) * Mathf.Rad2Deg;
		}
	}
}
