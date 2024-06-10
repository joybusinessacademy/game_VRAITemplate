using UnityEngine;

namespace SkillsVR.Mechanic.MechanicSystems.Gesture
{
	public class RangeAnimUI: MonoBehaviour
    {
        public float range;
        public Vector3 targetPos;

        public float outLineThickness = 30.0f;
        public float nearInlineThickness = 10.0f;
        public float farInlineThickness = 4.0f;
        private float cachedRange;

        void Update()
        {
            SetRange(range);
            UpdateTargetPos(targetPos);
        }

        public void SetRange(float newRange)
        {
            if (Mathf.Abs(newRange - cachedRange) < 0.001f)
            {
                return;
            }

            cachedRange = newRange;
            range = newRange;
            this.transform.localScale = Vector3.one * newRange * 2;
        }

        public void UpdateTargetPos(Vector3 newTargetPos)
        {
            float sqrDist = (newTargetPos - transform.position).sqrMagnitude;
            float sqrRange = range * range;
            if (0.0f == sqrRange)
            {
                sqrRange = 0.00001f;
            }
            float factor = sqrDist > sqrRange ? 0.0f : (sqrRange - sqrDist) / sqrRange;
            SetFactor(factor);
        }

        void SetFactor(float factor)
        {
            factor = Mathf.Clamp01(factor);
            var material = GetComponent<Renderer>().sharedMaterial;
            bool inRange = factor > 0.001;
            material.SetFloat("_INZone", inRange  ? 0.5f + factor * 0.5f : 0.0f);
            material.SetFloat("_INLineThickness", inRange ? Mathf.Lerp(farInlineThickness, nearInlineThickness , factor) : farInlineThickness);
            material.SetFloat("_OUTLineThickness", inRange ? 0.0f : outLineThickness);
        }
    }
}
