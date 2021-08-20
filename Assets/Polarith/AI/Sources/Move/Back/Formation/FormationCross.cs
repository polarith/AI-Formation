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
    /// Builds a cross-shaped formation, by computing the position for a specific individual (back-end class). Each 
    /// agent inside the formation is placed equidistantly along crossing lines. Note that the last layer may be sparse 
    /// if the number of available agents does not match the total number of agents that are needed for a symmetric 
    /// shape.
    /// </summary>
    [Serializable]
    public sealed class FormationCross : Formation
    {
        #region Fields =================================================================================================

        [Tooltip("Shape of the formation. Change the shape to be build in 2D or 3D.")]
        [SerializeField]
        private ShapeType shape = ShapeType.Planar;

        [Tooltip("Axis-aligned up vector of the agent according to the attached AIMSensor. Note that the " +
            "up vector becomes the forward vector if used with ShapeType.Spatial.")]
        [SerializeField]
        private Vector3 upVector;

        private Vector3 pos;
        private Vector3 layerStart;

        #endregion // Fields

        #region Properties =============================================================================================

        /// <summary>
        /// Shape of the formation. Change the shape to be build in 2D or 3D.
        /// </summary>
        public ShapeType Shape
        {
            get
            {
                return shape;
            }
            set
            {
                shape = value;
                UpdateResultPosition();
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Axis-aligned up vector of the agent according to the attached <see cref="AIMSensor"/>. Note that the
        /// up vector becomes the forward vector if used with <see cref="ShapeType.NonPlanar"/>.
        /// </summary>
        public Vector3 UpVector
        {
            get { return upVector; }
            set { upVector = value; }
        }

        #endregion // Properties

        #region Methods ================================================================================================

        /// <summary>
        /// Overwritten method to build up a <see cref="FormationCross"/>, i.e., placing agents in a line by computing
        /// the <see cref="AIMFormation.TargetPosition"/> of each agent. Units are placed equidistantly on crossing 
        /// lines with <see cref="Formation.Spacing"/> as distance between them. This method computes only the position 
        /// of a single agent with respect to the whole formation.
        /// </summary>
        public override Vector3 ComputePosition()
        {
            layerStart = Vector3.zero;

            if (shape == ShapeType.Planar)
            {
                layers = Mathf.CeilToInt(size / 2f);
                int centerLayer = Mathf.FloorToInt(layers / 2f);
                layers = size - 2 * centerLayer;

                return ComputePositionSolid2D(centerLayer);
            }
            else
            {
                layers = Mathf.CeilToInt((size-1) / 3f);
                int centerLayer = Mathf.CeilToInt(layers / 2f);
                layers = size - 4 * centerLayer;

                return ComputePositionSolid(centerLayer);
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionSolid2D(int centerLayer)
        {
            int normalizedPosition = 0;

            // Upper part ^
            if (positionInFormation < centerLayer)
            {
                layerStart.y += (centerLayer - positionInFormation) * spacing;
            }
            // Center axis < >
            else if (positionInFormation < 3 * centerLayer + 1)
            {
                normalizedPosition = positionInFormation - centerLayer;
                layerStart.x -= centerLayer * spacing;
            }
            // Lower part v
            else
            {
                layerStart.y -= (positionInFormation - 3 * centerLayer) * spacing;
            }

            pos = layerStart;
            pos.x += normalizedPosition * spacing;

            ChangeOrientation(upVector, ref pos);
            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionSolid(int centerLayer)
        {
            int normalizedPosition = 0;

            // Front part ^
            if (positionInFormation < centerLayer)
            {
                layerStart.z += (centerLayer - positionInFormation) * spacing;
                pos = layerStart;
                pos.x += normalizedPosition * spacing;
            }
            // Center axis <>
            else if (positionInFormation < 3 * centerLayer + 1)
            {
                normalizedPosition = positionInFormation - centerLayer;
                layerStart.x -= centerLayer * spacing;
                pos = layerStart;
                pos.x += normalizedPosition * spacing;
            }
            // Center axis top ||
            else if (positionInFormation < centerLayer * 4 + 1)
            {
                normalizedPosition = positionInFormation - 3 * centerLayer - 1;
                layerStart.y += centerLayer * spacing;
                pos = layerStart;
                pos.y -= normalizedPosition * spacing;
            }
            // Center axis bot ||
            else if (positionInFormation < centerLayer * 5 + 1)
            {
                normalizedPosition = positionInFormation - 4 * centerLayer - 1;
                layerStart.y -= spacing;
                pos = layerStart;
                pos.y -= normalizedPosition * spacing;
            }
            // Backward part v
            else
            {
                layerStart.z -= (positionInFormation - 5 * centerLayer) * spacing;
                pos = layerStart;
                pos.x += normalizedPosition * spacing;
            }

            ChangeOrientation(upVector, ref pos);
            return pos;
        }

        #endregion // Methods
    } // class FormationCross
} // namespace Polarith.AI.Move
