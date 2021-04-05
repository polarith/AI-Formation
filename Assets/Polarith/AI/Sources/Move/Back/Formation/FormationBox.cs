//
// Copyright (c) 2016-2021 Polarith. All rights reserved.
// Licensed under the Polarith Software and Source Code License Agreement.
// See the LICENSE file in the project root for full license information.
//

using System;
using UnityEngine;
using Polarith.Utils;

namespace Polarith.AI.Move
{
    /// <summary>
    /// Builds a box-shaped formation, by computing the position for a specific individual (back-end class). The shape 
    /// is primarily based on the number of <see cref="AgentsPerLine"/>. The first value defines the number of 
    /// neighboring agents along the x-axis, i.e., the width, while the second value defines the number neighboring 
    /// agents along the y-axis, i.e., the height. In case the formation is <see cref="Solid"/>, the agents are placed 
    /// with a filled center. Otherwise, the agents are placed on the boundary only. In case the <see cref="Shape"/> is 
    /// <see cref="ShapeType.Planar"/>, the formation is build up in a 2D layer, and thus, the second value of 
    /// <see cref="AgentsPerLine"/> is ignored. Otherwise, the formation is build in 3D. Note that the last layer may 
    /// be sparse if the number of available agents does not exactly match the total number of agents defined by 
    /// <see cref="AgentsPerLine"/>.
    /// </summary>
    [Serializable]
    public sealed class FormationBox : Formation
    {
        #region Fields =================================================================================================

        [Tooltip("Number of neighboring agents in width and height of the box-shape")]
        [SerializeField]
        private TupleInt agentsPerLine = new TupleInt(3, 3);

        [Tooltip("Solid or boundary agent placement.")]
        [SerializeField]
        private bool solid = true;

        [Tooltip("Shape of the formation. Change the shape to build the formation in 2D or 3D.")]
        [SerializeField]
        private ShapeType shape = ShapeType.Planar;

        [Tooltip("Axis-aligned up vector of the agent according to the attached AIMSensor. Note that the " +
            "up vector becomes the forward vector if used with ShapeType.NonPlanar.")]
        [SerializeField]
        private Vector3 upVector;

        private Vector3 pos;
        private Vector3 layerStart;
        private int agentLayer;
        private Polarith.Utils.Tuple<int,int> agentLayerId = new Polarith.Utils.Tuple<int,int>(0, 0);
        private int agentsPerLayer;

        #endregion // Fields

        #region Enums ==================================================================================================

        /// <summary>
        /// Defines the visual shape of the formation. You select whether the formation should be build as 2D or 3D
        /// version, i.e., as a flat rectangle or as a cuboid.
        /// </summary>
        public enum ShapeType
        {
            /// <summary>
            /// 2-dimensional representation (rectangle)
            /// </summary>
            Planar,

            /// <summary>
            /// 3-dimensional representation (cuboid)
            /// </summary>
            NonPlanar
        } // enum ShapeType

        #endregion // Enums

        #region Properties =============================================================================================

        /// <summary>
        /// Number of neighboring agents in width and height of the box-shape.
        /// </summary>
        public TupleInt AgentsPerLine
        {
            get
            {
                return agentsPerLine;
            }
            set
            {
                if (value.X < 1)
                {
                    value.X = 1;
                    Debug.LogWarning("AgentsPerLine.X needs to be at least 1! Value has been set to 1!");
                }
                if (value.Y < 1)
                {
                    value.Y = 1;
                    Debug.LogWarning("AgentsPerLine.Y needs to be at least 1! Value has been set to 1!");
                }
                agentsPerLine = value;
                UpdateResultPosition();
            }
        }

        //--------------------------------------------------------------------------------------------------------------

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
        /// Overwritten method to build up a <see cref="FormationBox"/>, i.e., placing agents as a box, by computing the
        /// <see cref="AIMFormation.TargetPosition"/> of each agent. <see cref="AgentsPerLine"/> are placed
        /// equidistantly on lines with <see cref="Formation.Spacing"/> as distance between them. The last(bottom) layer
        /// may be sparse, since there are probably not enough agents. This method computes only the position of a 
        /// single agent with respect to the whole formation.
        /// </summary>
        public override Vector3 ComputePosition()
        {
            layerStart = Vector3.zero;
            agentsPerLayer = agentsPerLine.X * agentsPerLine.Y;

            if (solid)
                return shape == ShapeType.NonPlanar ? ComputePositionSolid() : ComputePositionSolid2D();
            else
                return shape == ShapeType.NonPlanar ? ComputePositionNonSolid() : ComputePositionNonSolid2D();
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionSolid2D()
        {
            layers = Mathf.CeilToInt(size / (float)agentsPerLine.X);

            agentLayer = positionInFormation / agentsPerLine.X;  // zero-based
            agentLayerId.X = positionInFormation - agentLayer * agentsPerLine.X;    // zero-based
            int agentsSparseLayer = size % agentsPerLine.X;
            float localSpacingX = spacing;

            // Left position of layer
            layerStart.y += (layers - 1) / 2f * Spacing - agentLayer * Spacing;

            // Sparse layer
            if (size - positionInFormation < agentsPerLine.X && agentLayer * agentsPerLine.X > size - agentsPerLine.X)
            {
                localSpacingX = (Spacing * agentsPerLine.X) / (agentsSparseLayer + 1);
                layerStart.x -= (agentsSparseLayer - 1) / 2f * localSpacingX;
            }
            // Dense Layer
            else
            {
                layerStart.x -= (agentsPerLine.X - 1) / 2f * Spacing;
            }
            ComputeTargetPosition(layerStart, 1, 0, localSpacingX, spacing);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionSolid()
        {
            layers = Mathf.CeilToInt(size / (float)(agentsPerLine.X * agentsPerLine.Y));

            int numberFullLayers = size / (agentsPerLayer);
            float localSpacingX = spacing;
            float localSpacingY = spacing;
            agentLayer = positionInFormation / (agentsPerLayer);

            // Left position of layer
            layerStart.z += (layers - 1) / 2f * Spacing - agentLayer * Spacing;

            // Dense layer
            if (agentLayer < numberFullLayers)
            {
                // "normalize" position (map into 0-layer) and compute heightLayer
                agentLayerId.Y = (positionInFormation - agentLayer * agentsPerLayer) / agentsPerLine.X;
                agentLayerId.X = positionInFormation % agentsPerLine.X;

                layerStart.x -= (agentsPerLine.X - 1) / 2f * Spacing;
                layerStart.y -= (agentsPerLine.Y - 1) / 2f * Spacing;
            }
            // Sparse Layer
            else
            {
                Polarith.Utils.Tuple<int,int> newAgentsPerLine = 
                    new Polarith.Utils.Tuple<int, int>(agentsPerLine.X, agentsPerLine.Y);
                int agentsSparseLayer = size - agentsPerLayer * numberFullLayers;
                // Set importance to bigger line (keep bigger)
                RecomputeAgentsPerLine(ref newAgentsPerLine.X, ref newAgentsPerLine.Y, agentsSparseLayer);

                // Compute sparse position and layerHeight
                int sparsePosition =
                    Mathf.Abs(positionInFormation - agentsPerLayer * numberFullLayers);
                agentLayerId.Y = sparsePosition / newAgentsPerLine.X;

                // Set sparse position to position on layer
                sparsePosition -= agentLayerId.Y * newAgentsPerLine.X;

                // Adjust lineWidth to sparse line in sparse layer
                if (newAgentsPerLine.X * (agentLayerId.Y + 1) > agentsSparseLayer)
                    newAgentsPerLine.X = agentsSparseLayer - newAgentsPerLine.X * agentLayerId.Y;

                agentLayerId.X = sparsePosition % newAgentsPerLine.X;

                localSpacingX = (Spacing * agentsPerLine.X) / (newAgentsPerLine.X + 1);
                layerStart.x -= (newAgentsPerLine.X - 1) / 2f * localSpacingX;

                localSpacingY = (Spacing * agentsPerLine.Y) / (newAgentsPerLine.Y + 1);
                layerStart.y -= (newAgentsPerLine.Y - 1) / 2f * localSpacingY;
            }

            ComputeTargetPosition(layerStart, 1, 1, localSpacingX, localSpacingY);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void RecomputeAgentsPerLine(ref int newX, ref int newY, int agentsSparseLayer)
        {
            // Set importance to bigger line (keep bigger)
            if (agentsPerLine.Y <= agentsPerLine.X && agentsPerLine.Y > 1)
            {
                newY = Mathf.Min(Mathf.CeilToInt(Mathf.Sqrt(agentsSparseLayer)), agentsPerLine.Y);
                newX = agentsSparseLayer / newY;

                // Safely increase to cover all agents
                if (newX * newY < agentsSparseLayer)
                    newX = Mathf.Min(newX + 1, agentsPerLine.X);
                if (newX * newY < agentsSparseLayer)
                    newY = Mathf.Min(newY + 1, agentsPerLine.Y);
            }
            else if (agentsPerLine.Y > 1)
            {
                newX = Mathf.Min(Mathf.CeilToInt(Mathf.Sqrt(agentsSparseLayer)), agentsPerLine.X);
                newY = agentsSparseLayer / newX;

                // Safely increase to cover all agents
                if (newX * newY < agentsSparseLayer)
                    newY = Mathf.Min(newY + 1, agentsPerLine.Y);
                if (newX * newY < agentsSparseLayer)
                    newX = Mathf.Min(newX + 1, agentsPerLine.X);
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputeNonSolid()
        {
            if (shape == ShapeType.NonPlanar)
                return ComputePositionNonSolid();
            else
                return ComputePositionNonSolid2D();
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionNonSolid2D()
        {
            // Check if two full layers are possible
            if (size - 2 * agentsPerLine.X > 0)
                layers = Mathf.CeilToInt((size - 2f * agentsPerLine.X) / 2f) + 2;
            else
                layers = size - agentsPerLine.X > 0 ? 2 : 1;

            int agentsSparseLayer = 0;
            // Set agents in last layer
            if (layers < 2)
            {
                agentsSparseLayer = agentsPerLine.X;
                if ((size - 2 * agentsPerLine.X) % 2 == 1)
                    agentsSparseLayer--;
            }
            else if (layers == 2)
            {
                agentsSparseLayer = size - agentsPerLine.X;
            }
            else
            {
                if ((size - 2 * agentsPerLine.X) % 2 == 0)
                    agentsSparseLayer = agentsPerLine.X;
                else
                    agentsSparseLayer = agentsPerLine.X - 1;
            }

            ComputeNonSolid2DLayers(agentsSparseLayer);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeNonSolid2DLayers(int agentsSparseLayer)
        {
            int newAgentsPerLineX = agentsPerLine.X;
            float localSpacingX = spacing;

            // Layer 0
            if (positionInFormation < agentsPerLine.X)
            {
                agentLayer = 0;
                agentLayerId.X = positionInFormation;

                layerStart.x -= (agentsPerLine.X - 1) / 2f * Spacing;
                layerStart.y = (layers - 1) / 2f * Spacing - agentLayer * Spacing;

                pos = layerStart;
                pos.x += agentLayerId.X * Spacing;
            }
            // Layers of linewidth 2
            else if (positionInFormation < size - agentsSparseLayer)
            {
                newAgentsPerLineX = 2;
                agentLayer = 1 + (positionInFormation - agentsPerLine.X) / newAgentsPerLineX;
                agentLayerId.X =
                    (positionInFormation - (agentLayer - 1) * newAgentsPerLineX - agentsPerLine.X) %
                    newAgentsPerLineX;

                layerStart.x -= (agentsPerLine.X - 1) / 2f * Spacing;
                layerStart.y = (layers - 1) / 2f * Spacing - agentLayer * Spacing;

                pos = layerStart;
                pos.x += agentLayerId.X * (agentsPerLine.X - 1) * Spacing;
            }
            // Last layer
            else
            {
                newAgentsPerLineX = agentsSparseLayer;
                agentLayer = layers - 1;
                agentLayerId.X = Mathf.Abs(Mathf.Abs(positionInFormation - size) - agentsSparseLayer);

                localSpacingX = (Spacing * agentsPerLine.X) / newAgentsPerLineX;

                layerStart.x -= (newAgentsPerLineX - 1) / 2f * localSpacingX;
                layerStart.y = (layers - 1) / 2f * Spacing - agentLayer * Spacing;

                pos = layerStart;
                pos.x += agentLayerId.X * localSpacingX;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionNonSolid()
        {
            // Check if two full layers are possible
            if (size - 2 * agentsPerLayer > 0)
                layers = Mathf.CeilToInt((size - 2f * agentsPerLayer) /
                (2f * agentsPerLine.X + 2f * agentsPerLine.Y - 4)) + 2;    // -4: count corners only once
            else
                layers = size - agentsPerLayer > 0 ? 2 : 1;

            int agentsSparseLayer = ComputeNonSolidSparseAgents();

            float localSpacingX = spacing;
            float localSpacingY = spacing;

            // Layer 0
            if (positionInFormation < agentsPerLayer)
            {
                // Check if first layer is sparse (less agents in formation than size of first layer)
                if (size < agentsPerLayer)
                    return ComputePositionSolid();

                // "Normalize" position (map into 0-layer) and compute heightLayer
                agentLayer = 0;
                agentLayerId.Y = positionInFormation / agentsPerLine.X;
                agentLayerId.X = positionInFormation % agentsPerLine.X;

                layerStart.x -= (agentsPerLine.X - 1) / 2f * Spacing;
                layerStart.y -= (agentsPerLine.Y - 1) / 2f * Spacing;
            }
            // Boundary layers
            else if (positionInFormation < size - agentsSparseLayer)
            {
                ComputeNonSolidBoundaryLayer(ref localSpacingX, ref localSpacingY);
            }
            // Sparse layer
            else
            {
                ComputeNonSolidSparseLayer(agentsSparseLayer, ref localSpacingX, ref localSpacingY);
            }

            layerStart.z += (layers - 1) / 2f * Spacing - agentLayer * Spacing;

            ComputeTargetPosition(layerStart, 1, 1, localSpacingX, localSpacingY);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private int ComputeNonSolidSparseAgents()
        {
            int agentsSparseLayer = 0;
            // Set units in last layer
            if (layers < 2)
            {
                agentsSparseLayer = (size - 2 * agentsPerLayer) % (2 * agentsPerLine.X + 2 * agentsPerLine.Y - 4);
                if (agentsSparseLayer > 0)
                    agentsSparseLayer = agentsPerLayer - agentsSparseLayer;
                else
                    agentsSparseLayer = agentsPerLayer;
            }
            else
            {
                agentsSparseLayer = (size - agentsPerLayer) % (2 * agentsPerLine.X + 2 * agentsPerLine.Y - 4);
            }

            return agentsSparseLayer;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeNonSolidBoundaryLayer(ref float localSpacingX, ref float localSpacingY)
        {
            // "Normalize" position (map into 0-layer)
            agentLayer = 1 + (positionInFormation - agentsPerLayer) /
                (2 * agentsPerLine.X + 2 * agentsPerLine.Y - 4);

            int normalizedPosition = positionInFormation - agentsPerLayer -
                (2 * agentsPerLine.X + 2 * agentsPerLine.Y - 4) * (agentLayer - 1);

            int newAgentsPerLineX = agentsPerLine.X;

            // Bottom
            if (normalizedPosition < agentsPerLine.X)
            {
                agentLayerId.Y = 0;
                agentLayerId.X = positionInFormation % newAgentsPerLineX;
            }
            // Top
            else if (normalizedPosition > agentsPerLine.X + 2 * (agentsPerLine.Y - 2))
            {
                agentLayerId.Y = agentsPerLine.Y - 1;
                agentLayerId.X = (normalizedPosition - agentsPerLine.X - (agentsPerLine.Y - 2) * 2) %
                    agentsPerLine.X;
            }
            // Between
            else
            {
                agentLayerId.Y = (normalizedPosition - agentsPerLine.X) / 2 + 1;
                newAgentsPerLineX = 2;
                localSpacingX = Spacing * (agentsPerLine.X - 1);
                agentLayerId.X = (normalizedPosition - agentsPerLine.X) % newAgentsPerLineX;
            }

            layerStart.x -= (agentsPerLine.X - 1) / 2f * Spacing;
            layerStart.y -= (agentsPerLine.Y - 1) / 2f * Spacing;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeNonSolidSparseLayer(int agentsSparseLayer, ref float localSpacingX, ref float localSpacingY)
        {
            int newAgentsPerLineX = agentsPerLine.X;
            int newAgentsPerLineY = agentsPerLine.Y;

            RecomputeAgentsPerLine(ref newAgentsPerLineX, ref newAgentsPerLineY, agentsSparseLayer);

            // Compute sparse position and layerX/Y
            int sparsePosition = Mathf.Abs(Mathf.Abs(positionInFormation - size) - agentsSparseLayer);
            agentLayer = 1 + (positionInFormation - agentsPerLayer - sparsePosition) /
                (2 * agentsPerLine.X + 2 * agentsPerLine.Y - 4);
            agentLayerId.Y = sparsePosition / newAgentsPerLineX;

            // Adjust lineWidth and sparsePosition to sparse line in sparse layer
            if (newAgentsPerLineX * (agentLayerId.Y + 1) > agentsSparseLayer)
            {
                newAgentsPerLineX = agentsSparseLayer - newAgentsPerLineX * agentLayerId.Y;
                sparsePosition = Mathf.Abs(Mathf.Abs(sparsePosition - agentsSparseLayer) - newAgentsPerLineX);
            }

            agentLayerId.X = sparsePosition % newAgentsPerLineX;

            localSpacingX = (Spacing * agentsPerLine.X) / (newAgentsPerLineX + 1);
            layerStart.x -= (newAgentsPerLineX - 1) / 2f * localSpacingX;

            localSpacingY = (Spacing * agentsPerLine.Y) / (newAgentsPerLineY + 1);
            layerStart.y -= (newAgentsPerLineY - 1) / 2f * localSpacingY;
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
    } // class FormationBox
} // namespace Polarith.AI.Move
