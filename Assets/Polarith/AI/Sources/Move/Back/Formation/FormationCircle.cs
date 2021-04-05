//
// Copyright (c) 2016-2021 Polarith. All rights reserved.
// Licensed under the Polarith Software and Source Code License Agreement.
// See the LICENSE file in the project root for full license information.
//

using System;
using UnityEngine;

namespace Polarith.AI.Move
{
    /// <summary>
    /// Builds a circle-shaped formation, by computing the position for a specific individual (back-end class). The 
    /// radius is based on the number of agents and the <see cref="Formation.Spacing"/>. In case the formation is 
    /// <see cref="Solid"/>, the agents are placed with a filled center, i.e., stacked circles. Otherwise, the agents 
    /// are placed on the boundary only. Note that agents with a smaller position in the formaiton are placed on the 
    /// outside and larger numbers continously on the inside, and thus, the last agent on the center. In contrast to
    /// other formations, the first layer (outer) may be sparse if the number of available agents does not match the 
    /// total number of agents that are needed for the complete shape.
    /// </summary>
    [Serializable]
    public sealed class FormationCircle : Formation
    {
        #region Fields =================================================================================================

        [Tooltip("Solid or boundary agent placement.")]
        [SerializeField]
        private bool solid = true;

        [Tooltip("Axis-aligned up vector of the agent according to the attached AIMSensor. Note that the " +
             "up vector becomes the forward vector if used with ShapeType.NonPlanar.")]
        [SerializeField]
        private Vector3 upVector;

        private Vector3 pos;
        private float radius;

        #endregion // Fields

        #region Properties =============================================================================================

        /// <summary>
        /// Solid or boundary agent placement.
        /// </summary>
        public bool Solid
        {
            get
            {
                return solid;
            }
            set
            {
                solid = value;
                UpdateResultPosition();
            }
        }

        //--------------------------------------------------------------------------------------------------------------

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
        /// Overwritten method to build up a <see cref="FormationCircle"/>, i.e., placing agents as a circle by
        /// computing the <see cref="AIMFormation.TargetPosition"/> of each agent. The units are placed equidistantly on
        /// circles with <see cref="Formation.Spacing"/> as arc length distance between them. The outer layer may be
        /// sparse, since there are probably not enough units. This method computes only the position of a single agent
        /// with respect to the whole formation.
        /// </summary>
        public override Vector3 ComputePosition()
        {
            pos = Vector3.zero;
            layers = 0;
            radius = spacing;

            return solid ? ComputePositionSolid() : ComputePositionNonSolid();
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionSolid()
        {
            int tmpFormationSize = 1; // Center agent
            int tmpCurrentSize = 1;
            int agentsPerLayer = 0;
            float circumcircle = 0;

            // Compute inverted position to build reversed
            int inversePositionInFormation = Mathf.Abs(positionInFormation - size + 1);

            // Compute layer of agent (first/outer = sparse), start from center (inside to outside)
            while (tmpFormationSize < size)
            {
                circumcircle = 2f * spacing * (layers + 1) * Mathf.PI;
                agentsPerLayer = Mathf.FloorToInt(circumcircle / spacing);
                tmpFormationSize += agentsPerLayer;

                if (tmpFormationSize <= inversePositionInFormation)
                {
                    tmpCurrentSize += agentsPerLayer;
                    radius += spacing;
                }
                layers++;
            }

            radius = inversePositionInFormation == 0 ? 0 : radius;
            agentsPerLayer = inversePositionInFormation == 0 ? 1 : Mathf.FloorToInt(2f * radius * Mathf.PI / spacing);

            // Adjust sparse layer
            if (tmpCurrentSize + agentsPerLayer > size)
                agentsPerLayer = Math.Abs(tmpCurrentSize - size);

            // Map position to layer position
            int layerId = Mathf.Abs(inversePositionInFormation - tmpCurrentSize + 1);

            ComputeTargetPosition(agentsPerLayer, layerId);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionNonSolid()
        {
            layers = 1;

            int agentsPerLayer = size;
            int layerId = positionInFormation;
            float circumcircle = spacing * agentsPerLayer;
            radius = circumcircle / (Mathf.PI * 2f);

            ComputeTargetPosition(agentsPerLayer, layerId);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeTargetPosition(int agentsPerLayer, int layerId)
        {
            float theta = (2f * Mathf.PI) / agentsPerLayer;

            // Compute position
            pos.x += radius * Mathf.Cos(layerId * theta + Mathf.PI / 2f);
            pos.y += radius * Mathf.Sin(layerId * theta + Mathf.PI / 2f);

            ChangeOrientation(upVector, ref pos);
        }

        #endregion // Methods
    } // class FormationCircle
} // namespace Polarith.AI.Move
