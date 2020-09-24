﻿using System.Collections;
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
        public Quaternion Rotation { get; set; }

        public Bone Parent { get; set; }
        public readonly List<Bone> children = new List<Bone>();

        private float _length;
        private Quaternion _realBoneRotation;

        public void ExploreHierarchy ()
        {
            ExploreHierarchy(transform);
        }

        private void ExploreHierarchy (Transform trans)
        {
            for (int i = 0; i < trans.childCount; ++i)
            {
                var childBone = trans.GetChild(i).GetComponent<Bone>();
                if (childBone)
                {
                    childBone.ExploreHierarchy();
                    childBone.Parent = this;
                    children.Add(childBone);
                }
                else
                {
                    ExploreHierarchy(trans.GetChild(i));
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
            Rotation = Quaternion.LookRotation(target - Position, Rotation * Vector3.forward);

            _realBoneRotation = Quaternion.Inverse(Rotation) * transform.rotation;
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

                var target = GetTargetPoint();
                Rotation = Quaternion.LookRotation(target - Position, Rotation * Vector3.up);
            }

            if (target)
            {
                // If I have a target, go there
                Position = target.position;
                Rotation = target.rotation;
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
            for (int i = 0; i < children.Count; ++i)
            {
                // Pull all the children towards me
                var toChild = children[i].Position - Position;
                toChild.Normalize();
                children[i].Position = Position + toChild * children[i]._length;
                var childTarget = children[i].GetTargetPoint();
                children[i].Rotation = Quaternion.LookRotation(childTarget - children[i].Position, children[i].Rotation * Vector3.up);
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
            transform.rotation = Rotation * _realBoneRotation;

            // Allow the children to apply their values to their bones
            for(int i = 0; i < children.Count; ++i)
            {
                children[i].ApplyTransform();
            }
        }

        public Vector3 GetTargetPoint()
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            if (Application.isPlaying)
            {
                Gizmos.DrawWireSphere(Position, 0.01f);
                Gizmos.DrawLine(Position, Position + Rotation * Vector3.up * 0.1f);
                Gizmos.DrawLine(Position, Position + Rotation * Vector3.right * 0.1f);

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