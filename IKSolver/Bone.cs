using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Bone")]
    public class Bone : MonoBehaviour
    {
        public Transform target;

        public Vector3 Position { get; set; }

        public Bone Parent { get; set; }
        public readonly List<Bone> children = new List<Bone>();

        private float _length;
        private Quaternion _realBoneRotation;
        private Quaternion _targetLocalRotation;

        public void ExploreHierarchy (/*List<Constraint> constraints*/)
        {
            ExploreHierarchy(transform/*, constraints*/);
            //constraints.AddRange(GetComponents<Constraint>());
        }

        private void ExploreHierarchy (Transform trans/*, List<Constraint> constraints*/)
        {
            for (int i = 0; i < trans.childCount; ++i)
            {
                var childBone = trans.GetChild(i).GetComponent<Bone>();
                if (childBone)
                {
                    childBone.ExploreHierarchy(/*constraints*/);
                    childBone.Parent = this;
                    children.Add(childBone);
                }
                else
                {
                    ExploreHierarchy(trans.GetChild(i)/*, constraints*/);
                }
            }
        }

        public void Initialize(Vector3 preferredUp)
        {
            // Setup the position
            Position = transform.position;

            // Initialize the children first
            for (int i = 0; i < children.Count; ++i)
            {
                children[i].Initialize(preferredUp);
                children[i]._length = Vector3.Distance(children[i].Position, Position);
            }

            InitializeRotation();
        }

        private void InitializeRotation()
        {
            Quaternion rotation;
            if (children.Count > 0)
            {
                var targetPoint = GetTargetPoint();
                rotation = Quaternion.LookRotation(targetPoint - Position, Vector3.forward);
            }
            else
            {
                var targetPoint = Parent.GetTargetPoint();
                rotation = Quaternion.LookRotation(targetPoint - Parent.Position, Vector3.forward);
            }

            if (target)
            {
                _targetLocalRotation = Quaternion.Inverse(target.rotation) * rotation;
            }

            _realBoneRotation = Quaternion.Inverse(rotation) * transform.rotation;
        }

        public void PullEnds()
        {
            // First update the children
            for (int i = 0; i < children.Count; ++i)
            {
                children[i].PullEnds();
            }

            if (children.Count > 0)
            {
                // If I have children, let them pull me
                var newPosition = Vector3.zero;
                for (int i = 0; i < children.Count; ++i)
                {
                    newPosition += children[i].GetDesiredParentPosition();
                }
                Position = newPosition / children.Count;
            }

            if (target)
            {
                // If I have a target, go there
                Position = target.position;
            }
        }

        public Vector3 GetDesiredParentPosition ()
        {
            if (Parent)
            {
                // Calculate the position I want my parent to be at
                var toParent = Parent.Position - Position;
                toParent.Normalize();

                return (Position + toParent * _length);
            }
            return Vector3.zero;
        }

        public void PullRoot()
        {
            if (Parent)
            {
                var fromParent = (Position - Parent.Position).normalized;
                Position = Parent.Position + fromParent * _length;
            }

            for (int i = 0; i < children.Count; ++i)
            {
                // Allow them to pull their own children
                children[i].PullRoot();
            }
        }

        public void ApplyTransform()
        {
            // Apply my values to the real bone
            if (target)
            {
                transform.rotation = target.rotation * _targetLocalRotation * _realBoneRotation;
            }
            else
            {
                var targetPoint = GetTargetPoint();
                transform.rotation = Quaternion.LookRotation(targetPoint, Vector3.forward) * _realBoneRotation;
            }

            // Allow the children to apply their values to their bones
            for(int i = 0; i < children.Count; ++i)
            {
                children[i].ApplyTransform();
            }
        }

        public Vector3 GetTargetPoint()
        {
            var targetPoint = Vector3.zero;
            for (int i = 0; i < children.Count; ++i)
            {
                targetPoint += children[i].Position;
            }

            if (children.Count > 0f)
            {
                return targetPoint / children.Count;
            }
            return Position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            if (Application.isPlaying)
            {
                Gizmos.DrawWireSphere(Position, 0.01f);
                if (Parent)
                {
                    Gizmos.DrawLine(Position, Parent.Position);
                }
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, 0.01f);
            }
            if (target)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(target.position, 0.05f);
            }
        }
    }
}