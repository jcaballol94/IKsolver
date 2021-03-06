﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    public abstract class IKSolver : MonoBehaviour
    {
        public bool runAutomatically = true;
        public bool runNormally;
        public bool solveOnce;
        public bool runFullIteration;
        public bool runPositionIteration;
        public bool runRotationIteration;

        public Transform target;
        public int numIterations = 10;

        protected IKBone _rootBone;
        protected IKBone _tipBone;

        public static Transform GetChildThatLeadsToTip (Transform root, Transform tip)
        {
            for (int i = 0; i < root.childCount; ++i)
            {
                var child = root.GetChild(i);
                if (tip.IsChildOf(child))
                {
                    return child;
                }
            }
            return null;
        }

        public void DoIteration()
        {
            if (runNormally || solveOnce || runFullIteration || runRotationIteration)
            {
                _tipBone.IterateTargetRotation(_tipBone.transform, target);
                runRotationIteration = false;
            }
            if (runNormally || solveOnce || runFullIteration || runPositionIteration)
            {
                _tipBone.IterateTargetPosition(_tipBone.transform, target);
                runPositionIteration = false;
            }

            _rootBone.ApplyPose();
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