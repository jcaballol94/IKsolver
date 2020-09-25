using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Bone")]
    public class Bone : MonoBehaviour
    {
        public Vector3 Position { get; set; }

        public Bone Parent { get; set; }
        public readonly List<Bone> children = new List<Bone>();

        public IOrientationProvider orientationProvider;

        protected float _length;
        private Quaternion _realBoneRotation;

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
                    ProcessChild(childBone);
                }
                else
                {
                    ExploreHierarchy(trans.GetChild(i)/*, constraints*/);
                }
            }
        }

        protected virtual void ProcessChild (Bone child)
        {
            children.Add(child);
        }

        public virtual void Initialize()
        {
            // Setup the position
            Position = transform.position;

            // Initialize the children first
            for (int i = 0; i < children.Count; ++i)
            {
                children[i].Initialize();
                children[i]._length = Vector3.Distance(children[i].Position, Position);
            }

            InitializeRotation();
        }

        protected virtual void InitializeRotation()
        {
            var rotation = Quaternion.LookRotation(GetForwardVector(), GetUpVector());
            _realBoneRotation = Quaternion.Inverse(rotation) * transform.rotation;
        }

        public virtual void PullEnds()
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

        public virtual void PullRoot()
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
            transform.rotation = Quaternion.LookRotation(GetForwardVector(), GetUpVector()) * _realBoneRotation;

            // Allow the children to apply their values to their bones
            for(int i = 0; i < children.Count; ++i)
            {
                children[i].ApplyTransform();
            }
        }

        public Vector3 GetUpVector()
        {
            if (orientationProvider != null)
            {
                return orientationProvider.UpVector;
            }
            return Vector3.forward;
        }

        public virtual Vector3 GetForwardVector()
        {
            if (children.Count > 0)
            {
                var targetPoint = Vector3.zero;
                for (int i = 0; i < children.Count; ++i)
                {
                    targetPoint += children[i].Position;
                }
                targetPoint /= children.Count;
                return (targetPoint - Position).normalized;
            }
            return Parent.GetForwardVector();
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            if (Application.isPlaying)
            {
                Gizmos.DrawWireSphere(Position, 0.01f);

                if (Parent)
                {
                    Gizmos.DrawLine(Position, Parent.Position);
                }

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(Position, Position + GetUpVector() * 0.1f);
                Gizmos.DrawLine(Position, Position + GetForwardVector() * 0.1f);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, 0.01f);
            }
        }
    }
}