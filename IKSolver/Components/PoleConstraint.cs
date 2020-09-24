using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Constraints/Pole")]
    public class PoleConstraint : Constraint
    {
        [SerializeField] private Transform _pole;

        private Bone _parent;
        private Bone _child;

        private Quaternion _parentBaseRotation;

        public override void RegisterMessages()
        {
            _parent = _bone.Parent;
            _child = _bone.children[0];

            _parent.onPostInitialize.AddListener(CalculateBaseRotation);
            _parent.onPostPullEnds.AddListener(ConstraintToPlane);
            _child.onPrePullRoot.AddListener(ConstraintToPlane);
        }

        private Vector3 CalculateUp ()
        {
            var toPole = (_pole.position - _parent.Position).normalized;
            var toChild = (_child.Position - _parent.Position).normalized;

            return Vector3.Cross(toPole, toChild).normalized;
        }

        private void CalculateBaseRotation ()
        {
            _parentBaseRotation = Quaternion.Inverse(Quaternion.LookRotation(_parent.Rotation * Vector3.forward, CalculateUp())) * _parent.Rotation;
        }

        private void ConstraintToPlane ()
        {
            var toChild = (_child.Position - _parent.Position).normalized;
            var toPole = (_pole.position - _parent.Position).normalized;

            var parentToBone = (_bone.Position - _parent.Position);
            var angle = AngleOnPlane(parentToBone.normalized, toPole, toChild);

            parentToBone = Quaternion.AngleAxis(angle, toChild) * parentToBone;

            _bone.Position = _parent.Position + parentToBone;

            _parent.Rotation = Quaternion.LookRotation(_bone.Position - _parent.Position, CalculateUp()) * _parentBaseRotation;
            _bone.Rotation = Quaternion.LookRotation(_child.Position - _bone.Position, _bone.Rotation * Vector3.up);
        }

        private static float AngleOnPlane (Vector3 from, Vector3 to, Vector3 planeNormal)
        {
            var planeFrom = ProjectVectorToPlane(from, planeNormal);
            var planeTo = ProjectVectorToPlane(to, planeNormal);
            return Vector3.SignedAngle(planeFrom, planeTo, planeNormal);
        }

        private static Vector3 ProjectVectorToPlane (Vector3 vector, Vector3 planeNormal)
        {
            var height = Vector3.Dot(vector, planeNormal);
            var projectedVector = vector - planeNormal * height;
            return projectedVector.normalized;
        }

        private void OnDrawGizmos()
        {
            if (_pole)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_pole.position, 0.05f);
            }
        }
    }
}