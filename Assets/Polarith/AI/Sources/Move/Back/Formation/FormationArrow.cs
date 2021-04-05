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
    /// Builds an arrow-shaped formation, by computing the position for a specific individual (back-end class). The 
    /// shape is based on the number of agents, which increases with each additional layer. In case the formation is 
    /// <see cref="Solid"/>, the agents are placed with a filled center. Otherwise, the agents are placed on the 
    /// boundary only. In case the <see cref="Shape"/> is <see cref="ShapeType.Planar"/>, the formation is build up in 
    /// a 2D layer as a wedge. Otherwise, the formation is build in 3D as an arrow. Note that the last layer may 
    /// be sparse if the number of available agents does not exactly match the total number of agents that arise from 
    /// the growing number of agents in each layer.
    /// </summary>
    [Serializable]
    public sealed class FormationArrow : Formation
    {
        #region Fields =================================================================================================

        [Tooltip("Solid or boundary agent placement.")]
        [SerializeField]
        private bool solid = true;

        [Tooltip("Shape of the formation. Change the shape to build the formation in 2D or 3D")]
        [SerializeField]
        private ShapeType shape = ShapeType.Planar;

        [Tooltip("Axis-aligned up vector of the agent according to the attached AIMSensor. Note that the " + 
            "up vector becomes the forward vector if used with ShapeType.NonPlanar.")]
        [SerializeField]
        private Vector3 upVector;

        private Vector3 pos;
        private Vector3 layerStart;
        private Polarith.Utils.Tuple<int, int> agentLayerId = new Polarith.Utils.Tuple<int, int>(0, 0);
        private int agentLayer;
        private Polarith.Utils.Tuple<int, int> agentsPerLine = new Polarith.Utils.Tuple<int, int>(0, 0);
        private int agentsPerLayer;

        #endregion // Fields

        #region Enums ==================================================================================================

        /// <summary>
        /// Defines the visual shape of the formation. You select whether the formation should be build as 2D or 3D
        /// version, i.e., as a flat wedge or as an arrow.
        /// </summary>
        public enum ShapeType
        {
            /// <summary>
            /// 2-dimensional representation (wedge)
            /// </summary>
            Planar,

            /// <summary>
            /// 3-dimensional representation (arrow)
            /// </summary>
            NonPlanar
        } // enum ShapeType

        #endregion // Enums

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
        /// Shape of the formation. Change the shape to build the formation in 2D or 3D.
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
        /// up vector becomes the forward vector if used with a <see cref="ShapeType.NonPlanar"/> shape.
        /// </summary>
        public Vector3 UpVector
        {
            get { return upVector; }
            set { upVector = value; }
        }

        #endregion // Properties

        #region Methods ================================================================================================

        /// <summary>
        /// Overwritten method to build up a <see cref="FormationArrow"/>, i.e., placing agents as an arrow, by 
        /// computing the <see cref="AIMFormation.TargetPosition"/> of each agent. Units are placed equidistantly on 
        /// lines with <see cref="Formation.Spacing"/> as distance between them. The last layer may be sparse since 
        /// there are probably not enough units. This method computes only the position of a single agent with respect 
        /// to the whole formation.
        /// </summary>
        public override Vector3 ComputePosition()
        {
            layerStart = Vector3.zero;
            agentLayer = 0;
            layers = 0;
            agentsPerLayer = 1;

            return solid ? ComputeSolid() : ComputeNonSolid();
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputeSolid()
        {
            int tmpFormationSize = 0;
            if (shape == ShapeType.NonPlanar)
            {
                while (tmpFormationSize < Size)
                {
                    layers++;
                    tmpFormationSize += layers * layers;
                }
                return ComputePositionSolid();
            }
            else
            {
                while (tmpFormationSize < Size)
                {
                    layers++;
                    tmpFormationSize += layers;
                }
                return ComputePositionSolid2D();
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionSolid2D()
        {
            int tmpFormationSize = 1;
            agentLayer = 1;    // 1-based

            while (PositionInFormation >= tmpFormationSize)
            {
                tmpFormationSize += agentLayer + 1;
                agentLayer++;
            }

            int newAgentsPerLineX = agentLayer;
            float localSpacingX = spacing;

            // Sparse layer
            if (tmpFormationSize > Size)
            {
                newAgentsPerLineX = Size - tmpFormationSize + agentLayer;
                localSpacingX = (Spacing * agentLayer) / (newAgentsPerLineX + 1);
            }

            // Normalize position in layer
            agentLayerId.X = PositionInFormation - tmpFormationSize + agentLayer;

            layerStart.x -= (newAgentsPerLineX - 1) / 2f * localSpacingX;
            layerStart.y += (layers + 1) / 2f * Spacing - agentLayer * Spacing;

            ComputeTargetPosition(layerStart, 1, 0, localSpacingX, spacing);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionSolid()
        {
            int tmpFormationSize = 1;
            agentLayer = 1;

            while (PositionInFormation >= tmpFormationSize)
            {
                agentsPerLayer = (agentLayer + 1) * (agentLayer + 1);
                tmpFormationSize += agentsPerLayer;
                agentLayer++;
            }

            float localSpacingX = spacing;
            float localSpacingY = spacing;
            int newAgentsPerLineX = 0;
            int newAgentsPerLineY = 0;
            newAgentsPerLineX = newAgentsPerLineY = agentLayer;
            agentsPerLine.X = newAgentsPerLineX;
            agentsPerLine.Y = newAgentsPerLineY;

            // Normalize position in layer
            int normalizedPosition = PositionInFormation - tmpFormationSize + agentsPerLayer;

            layerStart.z += (layers + 1) / 2f * Spacing - agentLayer * Spacing;

            // Sparse layer
            if (tmpFormationSize > Size)
            {
                ComputeSolidSparseLayer(ref newAgentsPerLineX, ref newAgentsPerLineY,
                    ref localSpacingX, ref localSpacingY, tmpFormationSize, normalizedPosition);
            }
            else
            {
                // Compute position in layer
                agentLayerId.X = normalizedPosition % newAgentsPerLineX;
                agentLayerId.Y = normalizedPosition / newAgentsPerLineY;

                layerStart.x -= (newAgentsPerLineX - 1) / 2f * localSpacingX;
                layerStart.y -= (newAgentsPerLineY - 1) / 2f * localSpacingY;
            }

            ComputeTargetPosition(layerStart, 1, 1, localSpacingX, localSpacingY);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeSolidSparseLayer(ref int newUnitsPerLineWidth, ref int newUnitsPerLineHeight,
            ref float localSpacingWidth, ref float localSpacingHeight, int tmpFormationSize, int normalizedPosition)
        {
            int agentsSparseLayer = Size - tmpFormationSize + agentsPerLayer;
            RecomputeAgentsPerLine(ref newUnitsPerLineWidth, ref newUnitsPerLineHeight, agentsSparseLayer,
                newUnitsPerLineHeight);

            // Compute sparse position and layerWidth/-Height
            agentLayerId.Y = normalizedPosition / newUnitsPerLineWidth;
            int sparsePosition = normalizedPosition - agentLayerId.Y * newUnitsPerLineWidth;

            // Adjust lineWidth to sparse line in sparse layer
            if (newUnitsPerLineWidth * (agentLayerId.Y + 1) > agentsSparseLayer)
                newUnitsPerLineWidth = agentsSparseLayer - newUnitsPerLineWidth * (agentLayerId.Y);

            agentLayerId.X = sparsePosition % newUnitsPerLineWidth;

            localSpacingWidth = (Spacing * agentsPerLine.X) / (newUnitsPerLineWidth);
            layerStart.x -= (newUnitsPerLineWidth - 1) / 2f * localSpacingWidth;

            localSpacingHeight = (Spacing * agentsPerLine.Y) / (newUnitsPerLineHeight + 1);
            layerStart.y -= (newUnitsPerLineHeight - 1) / 2f * localSpacingHeight;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void RecomputeAgentsPerLine(ref int newX, ref int newY, int agentsSparseLayer, int newAgentsPerLineY)
        {
            newY = Mathf.Min(Mathf.FloorToInt(Mathf.Sqrt(agentsSparseLayer)), newAgentsPerLineY);
            newX = Mathf.Min(agentsSparseLayer / newY, agentsPerLine.X);
            newY = Mathf.Min(agentsSparseLayer / newX, agentsPerLine.Y);

            // Safely increase to cover all agents
            if (newX * newY < agentsSparseLayer)
                newX = Mathf.Min(newX + 1, agentsPerLine.X);
            if (newX * newY < agentsSparseLayer)
                newY = Mathf.Min(newY + 1, agentsPerLine.Y);
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputeNonSolid()
        {
            if (shape == ShapeType.NonPlanar)
            {
                layers = 1;
                int tmpFormationSize = 1;
                while (tmpFormationSize < Size)
                {
                    layers++;
                    tmpFormationSize += layers * 4 - 4;
                }
                return ComputePositionNonSolid();
            }
            else
            {
                layers = 1 + Mathf.CeilToInt((Size - 1) / 2f);
                return ComputePositionNonSolid2D();
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionNonSolid2D()
        {
            int newAgentsPerLineX = 0;

            // Setup params for layer
            if (PositionInFormation == 0)
            {
                agentLayer = 1;
                agentLayerId.X = 0;
                newAgentsPerLineX = 1;
            }
            else
            {
                agentLayer = Mathf.CeilToInt((PositionInFormation) / 2f) + 1;
                agentLayerId.X = (PositionInFormation + 1) % 2;
                newAgentsPerLineX = 2;
            }

            float localSpacingX = (agentLayer - 1) * Spacing;

            // Set left position of layer
            layerStart.x -= (newAgentsPerLineX - 1) / 2f * localSpacingX;
            layerStart.y += (layers + 1) / 2f * Spacing - agentLayer * Spacing;

            ComputeTargetPosition(layerStart, 1, 0, localSpacingX, spacing);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionNonSolid()
        {
            int tmpFormationSize = 1;
            agentLayer = 1;

            while (PositionInFormation >= tmpFormationSize)
            {
                agentsPerLayer = (agentLayer + 1) * 4 - 4;
                tmpFormationSize += agentsPerLayer;
                agentLayer++;
            }

            float localSpacingX = spacing;
            float localSpacingY = spacing;
            int newAgentsPerLineX;
            int newAgentsPerLineY;
            agentsPerLine.X = agentsPerLine.Y = agentLayer;
            newAgentsPerLineX = newAgentsPerLineY = agentLayer;

            // Normalize position in layer (map into first layer)
            int normalizedPosition = PositionInFormation - tmpFormationSize + agentsPerLayer;

            layerStart.z += (layers + 1) / 2f * Spacing - agentLayer * Spacing;

            // Sparse layer
            if (tmpFormationSize > Size)
            {
                ComputeNonSolidSparseLayer(normalizedPosition, newAgentsPerLineX, newAgentsPerLineY,
                    ref localSpacingX, ref localSpacingY, tmpFormationSize);
            }
            else
            {
                ComputeNonSolidDenseLayer(normalizedPosition, newAgentsPerLineX, newAgentsPerLineY,
                    ref localSpacingX);
            }

            layerStart.x -= (agentsPerLine.X - 1) / 2f * Spacing;
            layerStart.y -= (agentsPerLine.Y - 1) / 2f * Spacing;

            ComputeTargetPosition(layerStart, 1, 1, localSpacingX, localSpacingY);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeNonSolidSparseLayer(int normalizedPosition, int newAgentsPerLineX,
            int newAgentsPerLineY, ref float localSpacingX, ref float localSpacingY, int tmpFormationSize)
        {
            int agentsSparseLayer = Size - tmpFormationSize + agentsPerLayer;

            newAgentsPerLineY = Mathf.CeilToInt(Mathf.Sqrt(agentsSparseLayer));
            newAgentsPerLineX = Mathf.CeilToInt((agentsSparseLayer - 2 * newAgentsPerLineY + 4) / 2f);

            // Bottom
            if (normalizedPosition < newAgentsPerLineX)
            {
                agentLayerId.Y = 0;
                agentLayerId.X = normalizedPosition % newAgentsPerLineX;

                // Adjust lineWidth to sparse line in sparse layer
                if (agentsSparseLayer < 3)
                {
                    newAgentsPerLineX = 1;
                    layerStart.x += (agentsPerLine.X - 1) * Spacing / (newAgentsPerLineX + 1);
                }

                localSpacingX = (Spacing * (agentsPerLine.X - 1)) / Mathf.Max(newAgentsPerLineX - 1, 1);
            }
            // Top
            else if (normalizedPosition - newAgentsPerLineX - (newAgentsPerLineY - 2) * 2 >= 0)
            {
                normalizedPosition -= (newAgentsPerLineX + (newAgentsPerLineY - 2) * 2);
                agentLayerId.Y = newAgentsPerLineY - 1;

                // Adjust lineWidth to sparse line in sparse layer
                if (PositionInFormation + newAgentsPerLineX >= Size)
                    newAgentsPerLineX = agentsSparseLayer - newAgentsPerLineX - 2 * (agentLayerId.Y - 1);

                if (newAgentsPerLineX == 1)
                    layerStart.x += (agentsPerLine.X - 1) * Spacing / (newAgentsPerLineX + 1);

                agentLayerId.X = normalizedPosition % newAgentsPerLineX;

                localSpacingX = (Spacing * (agentsPerLine.X - 1)) / Mathf.Max(newAgentsPerLineX - 1, 1);
            }
            // Between
            else
            {
                // Fix height
                agentLayerId.Y = (normalizedPosition - newAgentsPerLineX) / 2 + 1;
                localSpacingX = Spacing * (agentsPerLine.X - 1);
                agentLayerId.X = (normalizedPosition - newAgentsPerLineX) % 2;
            }

            localSpacingY = (Spacing * (agentsPerLine.Y - 1)) / Mathf.Max(newAgentsPerLineY - 1, 1);
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeNonSolidDenseLayer(int normalizedPosition, int newAgentsPerLineX,
            int newAgentsPerLineY, ref float localSpacingX)
        {
            // Bottom
            if (normalizedPosition < newAgentsPerLineX)
            {
                agentLayerId.Y = 0;
                agentLayerId.X = normalizedPosition % newAgentsPerLineX;
            }
            // Top
            else if (normalizedPosition > newAgentsPerLineX + 2 * (newAgentsPerLineY - 2))
            {
                agentLayerId.Y = agentsPerLine.Y - 1;
                agentLayerId.X = (normalizedPosition - newAgentsPerLineX - (newAgentsPerLineY - 2) * 2) %
                    newAgentsPerLineX;
            }
            // Between
            else
            {
                agentLayerId.Y = (normalizedPosition - newAgentsPerLineX) / 2 + 1;
                newAgentsPerLineX = 2;
                localSpacingX = Spacing * (agentsPerLine.X - 1);
                agentLayerId.X = (normalizedPosition - agentsPerLine.X) % newAgentsPerLineX;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeTargetPosition(Vector3 layerStart, float xScale, float yScale,
            float localSpacingX, float localSpacingY)
        {
            pos = layerStart;
            pos.x += agentLayerId.X * localSpacingX * xScale;
            pos.y += agentLayerId.Y * localSpacingY * yScale;

            ChangeOrientation(upVector, ref pos);
        }

        #endregion // Methods
    } // class FormationArrow
} // namespace Polarith.AI.Move
