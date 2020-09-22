using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("")]
    public class FabrikBone : MonoBehaviour
    {
        public Transform RealBone { get; set; }
        public FabrikBone Parent { get; set; }
        public List<FabrikBone> Children { get; private set; } = new List<FabrikBone>();
        public Transform Target { get; set; }

        public struct TargetPose
        {
            public Vector3 position;
            public Vector3 up;
        }

        private Quaternion _realBoneRotation;
        private Vector3[] _toChild;
        private Vector3 _toReference;

        public void Initialize()
        {
            // Calculate our orientation
            var desiredPos = CalculateTargetPoint();
            transform.forward = desiredPos - transform.position;
            _toReference = transform.InverseTransformPoint(desiredPos);

            // The rotation to apply when translating it to the real bone
            _realBoneRotation = Quaternion.Inverse(transform.rotation) * RealBone.transform.rotation;

            // Calculate the local position of the children
            _toChild = new Vector3[Children.Count];
            for (int i = 0; i < Children.Count; ++i)
            {
                _toChild[i] = transform.InverseTransformPoint(Children[i].transform.position);
            }

            // Initialize all the children
            foreach (var child in Children)
            {
                child.Initialize();
            }
        }

        public TargetPose PullEnds (Vector3 parentPos, Vector3 toMe, Vector3 parentForward, Vector3 parentUp, Vector3 parentRight)
        {
            // First we need to pull all the children
            TargetPose target = new TargetPose { position = Vector3.zero, up = Vector3.up };
            for (int i = 0; i < Children.Count; ++i)
            {
                // Get up vector for this case
                var toChild = transform.TransformVector(_toChild[i]);
                // Get the desired pose from the child pulling us
                var newTarget = Children[i].PullEnds(transform.position, toChild, transform.forward, transform.up, transform.right);

                // Add the pose for the average
                target.position += newTarget.position;
                target.up += newTarget.up;
            }
            if (Children.Count != 0f)
            {
                // Do the average
                target.position /= Children.Count;
                target.up /= Children.Count;
            }
            if (Mathf.Approximately(target.up.magnitude, 0f))
            {
                // Avoid null vector
                target.up = transform.up;
            }

            if (Target)
            {
                // If we are an end pull ourselves
                transform.position = Target.position;
                transform.rotation = Target.rotation;
            }
            else
            {
                // If we are not an end let the children pull us
                transform.position = target.position;
                var desiredPos = CalculateTargetPoint();
                transform.forward = desiredPos - transform.position;
            }

            // Calculate the position of the parent
            var parentDistance = toMe.magnitude;
            var newToMe = transform.position - parentPos;
            newToMe.Normalize();
            var rotation = Quaternion.FromToRotation(toMe, newToMe);

            return new TargetPose
            {
                position = transform.position - newToMe * parentDistance,
                up = rotation * parentUp
            };
        }

        public void PullRoot(Vector3 parentForward, Vector3 parentUp, Vector3 parentRight)
        {
            var target = transform.position + transform.forward;

            // Adjust myself to face the children
            if (Children.Count > 0)
            {
                target = CalculateTargetPoint();
            }
            transform.rotation = Quaternion.LookRotation(target - transform.position);

            // Pull the children
            for (int i = 0; i < Children.Count; ++i)
            {
                Children[i].transform.position = transform.TransformPoint(_toChild[i]);
                Children[i].PullRoot(transform.forward, transform.up, transform.right);
            }
        }
        public void ApplyTransform()
        {
            RealBone.position = transform.position;
            RealBone.rotation = transform.rotation * _realBoneRotation;

            foreach (var child in Children)
            {
                child.ApplyTransform();
            }
        }

        private Vector3 CalculateTargetPoint()
        {
            if (Children.Count == 0) return transform.position + (Parent ? Parent.transform.forward : transform.parent.forward);

            Vector3 pos = Vector3.zero;
            foreach (var child in Children)
            {
                pos += child.transform.position;
            }
            pos /= Children.Count;
            return pos;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            if (Parent)
            {
                Gizmos.DrawLine(transform.position, Parent.transform.position);
            }
            Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.2f);
            Gizmos.DrawLine(transform.position, transform.position + transform.right * 0.2f);
        }
    }
}