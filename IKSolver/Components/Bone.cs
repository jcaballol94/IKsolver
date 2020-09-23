using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Bone")]
    public class Bone : MonoBehaviour
    {
        public Transform target;

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public Bone Parent { get; set; }
        public readonly List<Bone> children = new List<Bone>();

        private float _length;
        private Quaternion _realBoneRotation;

        public void ExploreHierarchy ()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                var childBone = transform.GetChild(i).GetComponentInChildren<Bone>();
                if (childBone)
                {
                    childBone.ExploreHierarchy();
                    childBone.Parent = this;
                    children.Add(childBone);
                }
            }
        }

        public void Initialize()
        {
            // Setup the position
            Position = transform.position;

            // Initialize the children first
            for (int i = 0; i < children.Count; ++i)
            {
                children[i].Initialize();
                children[i]._length = Vector3.Distance(children[i].Position, Position);
            }

            // Orient myself based on the position of the children
            var target = GetTargetPoint();
            Rotation = Quaternion.LookRotation(target - Position);

            _realBoneRotation = Quaternion.Inverse(Rotation) * transform.rotation;
        }

        public Vector3 PullEnds()
        {
            if (target)
            {
                // If I have a target, go there
                Position = target.position;
                Rotation = target.rotation;
            }
            else if (children.Count > 0)
            {
                // If I have children, let them pull me
                var newPosition = Vector3.zero;
                for (int i = 0; i < children.Count; ++i)
                {
                    newPosition += children[i].PullEnds();
                }
                Position = newPosition / children.Count;

                var target = GetTargetPoint();
                Rotation = Quaternion.LookRotation(target - Position);
            }

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
            for (int i = 0; i < children.Count; ++i)
            {
                // Pull all the children towards me
                var toChild = children[i].Position - Position;
                toChild.Normalize();
                children[i].Position = Position + toChild * children[i]._length;

                // Allow them to pull their own children
                children[i].PullRoot();
            }
        }

        public void ApplyTransform()
        {
            // Apply my values to the real bone
            transform.position = Position;
            transform.rotation = Rotation * _realBoneRotation;

            // Allow the children to apply their values to their bones
            for(int i = 0; i < children.Count; ++i)
            {
                children[i].ApplyTransform();
            }
        }

        private Vector3 GetTargetPoint()
        {
            var target = Vector3.zero;
            for (int i = 0; i < children.Count; ++i)
            {
                target += children[i].Position;
            }

            if (children.Count > 0f)
            {
                return target / children.Count;
            }
            return Position + Rotation * Vector3.forward;
        }
    }
}