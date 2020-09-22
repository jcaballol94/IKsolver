using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Solver")]
    public class IKSolver : MonoBehaviour
    {
        public bool runAutomatically = true;
        public bool runNormally = true;
        public bool solveOnce;
        public bool runFullIteration;
        public bool pullEnds;
        public bool pullRoots;

        public int numIterations = 10;
        [Range(0f, 1f)] public float _pinRoot = 1f;

        private FabrikBone _rootBone;
        private Vector3 _rootPosition;

        private void Start()
        {
            var rootDefinition = GetComponentInChildren<FabrikBoneDefinition>();
            _rootBone = rootDefinition.CreateBones(transform);
            _rootBone.Initialize();
            _rootPosition = _rootBone.transform.localPosition;
        }

        public void DoIteration()
        {
            if (runNormally || solveOnce || runFullIteration || pullEnds)
            {
                _rootBone.PullEnds(transform.position, _rootBone.transform.localPosition, transform.forward, transform.up, transform.up);
                pullEnds = false;
            }
            if (runNormally || solveOnce || runFullIteration || pullRoots)
            {
                _rootBone.transform.localPosition = Vector3.Lerp(_rootBone.transform.localPosition, _rootPosition, _pinRoot);
                _rootBone.PullRoot(transform.forward, transform.up, transform.right);
                pullRoots = false;
            }

            _rootBone.ApplyTransform();
            runFullIteration = false;
        }

        public void Solve ()
        {
            for (int i = 0; i < numIterations; ++i)
            {
                DoIteration();

                if (!runNormally && !solveOnce)
                {
                    break;
                }
            }
            solveOnce = false;
        }

        private void LateUpdate()
        {
            if (runAutomatically)
            {
                Solve();
            }
        }
    }
}