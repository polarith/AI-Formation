//
// MIT License
// Copyright (c) 2021 Polarith.
// See the LICENSE file in the project root for full license information.
//

using System;
using UnityEngine;

namespace Polarith.AI.Move
{
    /// <summary>
    /// Builds a line-shaped formation, by computing the position for a specific individual (back-end class). Each 
    /// agent inside the formation is placed equidistantly along a line.
    /// </summary>
    [Serializable]
    public sealed class FormationLine : Formation
    {
        #region Fields =================================================================================================

        [Tooltip("Axis-aligned up vector of the agent according to the attached AIMSensor.")]
        [SerializeField]
        private Vector3 upVector;

        private Vector3 pos;
        private Vector3 layerStart;

        #endregion // Fields

        #region Properties =============================================================================================

        /// <summary>
        /// Axis-aligned up vector of the agent according to the attached <see cref="AIMSensor"/>.
        /// </summary>
        public Vector3 UpVector
        {
            get { return upVector; }
            set { upVector = value; }
        }

        #endregion // Properties

        #region Methods ================================================================================================

        /// <summary>
        /// Overwritten method to build up a <see cref="FormationLine"/>, i.e., placing agents in a line, by computing
        /// the <see cref="AIMFormation.TargetPosition"/> of each agent. Units are placed equidistantly on a line with
        /// <see cref="Formation.Spacing"/> as distance between them. This method computes only the position of a single
        /// agent with respect to the whole formation.
        /// </summary>
        public override Vector3 ComputePosition()
        {
            layers = 1;
            layerStart = Vector3.zero;
            layerStart.x -= (size - 1) / 2f * spacing;

            pos = layerStart;
            pos.x += positionInFormation * spacing;

            ChangeOrientation(upVector, ref pos);
            return pos;
        }

        #endregion // Methods
    } // class FormationLine
} // namespace Polarith.AI.Move
