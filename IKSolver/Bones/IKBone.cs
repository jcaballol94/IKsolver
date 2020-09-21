using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    public class IKBone : MonoBehaviour
    {
        public enum RotationConstraintType
        {
            NONE,
            HINGE,
            BALL
        }

        public Transform realBone;
        public IKBone parent;
        public IKBone child;
        public RotationConstraintType constraintType = RotationConstraintType.NONE;
        public Vector3 constraintAxis = Vector3.up;
        public Vector2 constraintRotationLimits = Vector2.zero;

        private Quaternion _realBoneRotation;

        private void Start()
        {
            _realBoneRotation = Quaternion.Inverse(transform.rotation) * realBone.transform.rotation;
        }

        public void ApplyPose ()
        {
            realBone.rotation = transform.rotation * _realBoneRotation;

            if (child)
            {
                child.ApplyPose();
            }
        }

        public void IterateTargetPosition (Transform tip, Transform target)
        {
            if (transform != tip)
            {
                var toTip = tip.position - transform.position;
                var toTarget = target.position - transform.position;
                var desiredRotation = Quaternion.FromToRotation(toTip, toTarget);

                var desiredForward = desiredRotation * transform.forward;
                var desiredUp = desiredRotation * transform.up;
                var desiredRight = desiredRotation * transform.right;

                ApplyRotationConstraint(ref desiredForward);

                transform.rotation = Quaternion.LookRotation(desiredForward, desiredUp);
            }

            if (parent)
            {
                parent.IterateTargetPosition(tip, target);
            }
        }

        public void IterateTargetRotation (Transform tip, Transform target)
        {
            var desiredRotation = target.rotation * Quaternion.Inverse(tip.rotation);

            var desiredForward = desiredRotation * transform.forward;
            var desiredUp = desiredRotation * transform.up;
            var desiredRight = desiredRotation * transform.right;

            ApplyRotationConstraint(ref desiredForward);

            transform.rotation = Quaternion.LookRotation(desiredForward, desiredUp);

            if (parent)
            {
                parent.IterateTargetRotation(tip, target);
            }
        }

        private void ApplyRotationConstraint(ref Vector3 forward)
        {
            switch (constraintType)
            {
                case RotationConstraintType.HINGE:
                    {
                        var axis = transform.parent.rotation * constraintAxis;
                        var hingeAngle = AngleInPlane(transform.parent.forward, forward, axis);

                        hingeAngle = ConstrainAngle(hingeAngle, constraintRotationLimits);

                        var hingeRotation = Quaternion.AngleAxis(hingeAngle, axis);
                        forward = hingeRotation * transform.parent.forward;
                    }
                    break;

                case RotationConstraintType.BALL:
                    {
                        var axis = transform.parent.rotation * constraintAxis;

                        var desiredRotation = Quaternion.FromToRotation(axis, forward);
                        var constrainedRotation = Quaternion.RotateTowards(Quaternion.identity, desiredRotation, constraintRotationLimits.x);
                        forward = constrainedRotation * axis;
                    }
                    break;
            }
        }

        public static float AngleInPlane(Vector3 from, Vector3 to, Vector3 axis)
        {
            var projectedFrom = ProjectVector(from, axis);
            var projectedTo = ProjectVector(to, axis);
            return Vector3.SignedAngle(projectedFrom, projectedTo, axis);
        }

        public static Vector3 ProjectVector(Vector3 vector, Vector3 axis)
        {
            var correction = Vector3.Dot(axis, vector);
            var projected = vector - axis * correction;
            return projected.normalized;
        }

        public static float ConstrainAngle (float angle, Vector2 limits)
        {
            var center = (limits.x + limits.y) * 0.5f;
            angle = CenterAngle(angle, center);
            return Mathf.Clamp(angle, limits.x, limits.y);
        }

        public static float CenterAngle (float angle, float center)
        {
            while (angle > center + 180f)
            {
                angle -= 360f;
            }
            while (angle < center - 180f)
            {
                angle += 360f;
            }

            return angle;
        }
    }
}