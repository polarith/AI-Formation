//
// Copyright (c) 2016-2021 Polarith. All rights reserved.
// Licensed under the Polarith Software and Source Code License Agreement.
// See the LICENSE file in the project root for full license information.
//

using Polarith.UnityUtils;
using Polarith.Utils;
using System;
using UnityEngine;

namespace Polarith.AI.Move
{
    /// <summary>
    /// Builds a V-shaped formation, by computing the position for a specific individual (back-end class). The shape is
    /// primarily based on the number of <see cref="AgentsPerLine"/>. The first value defines the number of neighboring
    /// agents along the x-axis, i.e., the width, while the second value defines the number neighboring agents along
    /// the y-axis, i.e., the height. In case the formation is <see cref="Solid"/>, the agents are placed with a filled
    /// center. Otherwise, the agents are placed on the boundary only. In case the <see cref="Shape"/> is
    /// <see cref="ShapeType.Planar"/>, the formation is build up in a 2D layer, and thus, the second value of
    /// <see cref="AgentsPerLine"/> is ignored. Otherwise, the formation is build in 3D. Note that the last layer may
    /// be sparse if the number of available agents does not match the total number of agents defined by
    /// <see cref="AgentsPerLine"/>.
    /// </summary>
    [Serializable]
    public sealed class FormationV : Formation
    {
        #region Fields =================================================================================================

        [Tooltip("Number of neighboring agents in width and height of the V-shape.")]
        [SerializeField]
        private TupleInt agentsPerLine = new TupleInt(1, 1);

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

        private int sparseLayers = 0;

        private Vector3 pos;
        private Vector3 layerStart;
        private int agentLayer;
        private Polarith.Utils.Tuple<int, int> agentLayerId = new Polarith.Utils.Tuple<int, int>(0, 0);

        #endregion // Fields

        #region Enums ==================================================================================================

        /// <summary>
        /// Defines the visual shape of the formation. You select whether the formation should be build as 2D or 3D
        /// version, i.e., as a flat V or a stacked V.
        /// </summary>
        public enum ShapeType
        {
            /// <summary>
            /// 2-dimensional representation (flat V)
            /// </summary>
            Planar,

            /// <summary>
            /// 3-dimensional representation (stacked V)
            /// </summary>
            NonPlanar
        } // enum ShapeType

        #endregion // Enums

        #region Properties =============================================================================================

        /// <summary>
        /// Number of neighboring agents in width and height of the V-shape.
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

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Number of sparse layers behind the V-shaped front. Used to set the dimensions of the <see cref="VGizmo"/>.
        /// </summary>
        public int SparseLayers
        {
            get { return sparseLayers; }
        }

        #endregion // Properties

        #region Methods ================================================================================================

        /// <summary>
        /// Overwritten method to build up a <see cref="FormationV"/>, i.e., placing agents as a V, by computing
        /// the <see cref="AIMFormation.TargetPosition"/> of each agent. Units are placed equidistantly on lines with
        /// <see cref="AgentsPerLine"/> as thickness of the V-shape, and <see cref="Formation.Spacing"/> as distance
        /// between them. The last layer may be sparse since there are probably not enough units. This method computes
        /// only the position of a single agent with respect to the whole formation.
        /// </summary>
        public override Vector3 ComputePosition()
        {
            layerStart = Vector3.zero;
            agentLayer = 0;
            layers = 0;

            if (solid)
                return shape == ShapeType.NonPlanar ? ComputePositionSolid() : ComputePositionSolid2D();
            else
                return shape == ShapeType.NonPlanar ? ComputePositionNonSolid() : ComputePositionNonSolid2D();
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionSolid2D()
        {
            if (agentsPerLine.X == 1)
            {
                layers = 1 + Mathf.CeilToInt((size - 1) / 2f);
                return ComputePositionNonSolid2D();
            }

            agentLayer = 0;

            int newAgentsPerLineX = 2 * agentsPerLine.X;
            int idealBackLayerSize = agentsPerLine.X * (2 * agentsPerLine.X - 1);
            int frontLayers = Mathf.CeilToInt((size - idealBackLayerSize) / (2f * agentsPerLine.X));
            if (frontLayers < 1)
                frontLayers = 1;
            int frontLayerSize = frontLayers * 2 * agentsPerLine.X;

            // Front layers
            if (positionInFormation < frontLayerSize)
                ComputeSolidFrontLayers2D(newAgentsPerLineX, frontLayers);

            // Back layers
            int backLayerSize = size - frontLayerSize;
            int tmpSize = 0;
            int backLayers = 0;

            ComputeSolidBackLayers2D(frontLayers, frontLayerSize, ref backLayers,
                backLayerSize, ref tmpSize, ref newAgentsPerLineX);

            layers = frontLayers + backLayers;
            sparseLayers = backLayers;

            // Back layer start
            if (positionInFormation >= frontLayerSize)
            {
                // Sparse layer
                if (agentLayer - frontLayers + 1 == backLayers && backLayerSize != idealBackLayerSize)
                    newAgentsPerLineX -= tmpSize - backLayerSize;

                layerStart.x -= (newAgentsPerLineX - 1) / 2f * spacing;
            }

            agentLayerId.Y = 0;
            layerStart.y = (layers - 1) / 2f * spacing - agentLayer * spacing;

            ComputeTargetPosition(layerStart, 1, 0, spacing, spacing);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeSolidFrontLayers2D(int newAgentsPerLineX, int frontLayers)
        {
            agentLayer = positionInFormation / newAgentsPerLineX;
            agentLayerId.X = positionInFormation % newAgentsPerLineX;

            if (agentLayerId.X < agentsPerLine.X)
            {
                layerStart.x -= ((frontLayers - agentLayer) / 2f + agentsPerLine.X - 1) * spacing;
            }
            else
            {
                agentLayerId.X -= agentsPerLine.X;
                layerStart.x += (frontLayers - agentLayer) / 2f * spacing;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeSolidBackLayers2D(int frontLayers, int frontLayerSize, ref int backLayers,
            int backLayerSize, ref int tmpSize, ref int newAgentsPerLineX)
        {
            int normalizedPosition = positionInFormation - frontLayerSize;
            while (tmpSize < backLayerSize)
            {
                normalizedPosition = positionInFormation - tmpSize - frontLayerSize;
                if (normalizedPosition >= 0 && normalizedPosition < 2 * agentsPerLine.X - backLayers)
                {
                    agentLayer = frontLayers + backLayers;
                    agentLayerId.X = normalizedPosition;
                    newAgentsPerLineX = 2 * agentsPerLine.X - backLayers - 1;
                }

                backLayers++;
                tmpSize += 2 * agentsPerLine.X - backLayers;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionSolid()
        {
            if (agentsPerLine.X == 1)
                return ComputePositionNonSolid();

            agentLayer = 0;

            int newAgentsPerLineX = 2 * agentsPerLine.X;
            int layerSize = newAgentsPerLineX * agentsPerLine.Y;
            int idealBackLayerSize = (2 * agentsPerLine.X - 1) * agentsPerLine.X * agentsPerLine.Y;
            int frontLayers = Mathf.CeilToInt((size - idealBackLayerSize) / (float)layerSize);
            if (frontLayers < 1)
                frontLayers = 1;
            int frontLayerSize = frontLayers * layerSize;
            int normalizedPosition = positionInFormation;

            // Front layers
            if (positionInFormation < frontLayerSize)
                ComputeSolidFrontLayers(frontLayers, layerSize, ref normalizedPosition, newAgentsPerLineX);

            // Back layers
            int backLayerSize = size - frontLayerSize;
            int backLayers = 1;

            ComputeSolidBackLayers(ref normalizedPosition, layerSize, frontLayers,
                frontLayerSize, ref backLayers, backLayerSize, ref newAgentsPerLineX);

            layers = frontLayers + --backLayers;
            sparseLayers = backLayers;

            // Back layer start
            if (positionInFormation >= frontLayerSize)
            {
                int newAgentsPerLineY = agentsPerLine.Y;

                // Sparse layer
                if (agentLayer - frontLayers + 1 == backLayers && backLayerSize != idealBackLayerSize)
                {
                    ComputeSolidSparseLayer(normalizedPosition, backLayers, backLayerSize,
                        idealBackLayerSize, ref newAgentsPerLineX, ref newAgentsPerLineY);
                }

                layerStart.x -= (newAgentsPerLineX - 1) / 2f * spacing;
                layerStart.y += (agentsPerLine.Y - newAgentsPerLineY) / 2f * spacing;
            }

            layerStart.z = (layers - 1) / 2f * spacing - agentLayer * spacing;

            ComputeTargetPosition(layerStart, 1, 1, spacing, spacing);
            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeSolidFrontLayers(int frontLayers, int layerSize, ref int normalizedPosition,
            int newAgentsPerLineX)
        {
            agentLayer = positionInFormation / layerSize;
            normalizedPosition = positionInFormation - agentLayer * layerSize;
            agentLayerId.X = normalizedPosition % newAgentsPerLineX;
            agentLayerId.Y = normalizedPosition / newAgentsPerLineX;

            if (agentLayerId.X < agentsPerLine.X)
            {
                layerStart.x -= ((frontLayers - agentLayer) / 2f + agentsPerLine.X - 1) * spacing;
            }
            else
            {
                agentLayerId.X -= agentsPerLine.X;
                layerStart.x += (frontLayers - agentLayer) / 2f * spacing;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeSolidBackLayers(ref int normalizedPosition, int layerSize, int frontLayers,
            int frontLayerSize, ref int backLayers, int backLayerSize, ref int newAgentsPerLineX)
        {
            int tmpSize = 0;
            normalizedPosition = positionInFormation - frontLayerSize;
            while (tmpSize < backLayerSize)
            {
                normalizedPosition = positionInFormation - tmpSize - frontLayerSize;
                if (normalizedPosition >= 0 && normalizedPosition < layerSize - backLayers * agentsPerLine.Y)
                {
                    newAgentsPerLineX = 2 * agentsPerLine.X - backLayers;
                    agentLayer = frontLayers + backLayers - 1;
                    agentLayerId.X = normalizedPosition % newAgentsPerLineX;
                    agentLayerId.Y = normalizedPosition / newAgentsPerLineX;
                }

                tmpSize += layerSize - backLayers++ * agentsPerLine.Y;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeSolidSparseLayer(int normalizedPosition, int backLayers,
            int backLayerSize, int idealBackLayerSize, ref int newAgentsPerLineX, ref int newAgentsPerLineY)
        {
            int maxAgentsLastLine = agentsPerLine.Y * (2 * agentsPerLine.X - backLayers);
            int outlierAgents = (int)(agentsPerLine.Y * (2 * agentsPerLine.X - backLayers - 1) *
                (agentsPerLine.X - backLayers / 2f));
            int backAgents = idealBackLayerSize - outlierAgents;
            int sparseAgents = backLayers > 1 ? maxAgentsLastLine - backAgents + backLayerSize : backLayerSize;

            newAgentsPerLineY = Mathf.CeilToInt(sparseAgents / (1f * newAgentsPerLineX));
            if (normalizedPosition >= (newAgentsPerLineY - 1) * newAgentsPerLineX)
                newAgentsPerLineX = sparseAgents - (newAgentsPerLineY - 1) * newAgentsPerLineX;
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionNonSolid2D()
        {
            agentLayer = Mathf.FloorToInt(PositionInFormation / 2f);
            agentLayerId.X = PositionInFormation % 2;

            if (size % 2 == 0)
                sparseLayers = 0;
            else
                sparseLayers = 1;

            layers = Mathf.FloorToInt(size / 2f) + sparseLayers;

            if (agentLayerId.X < 1)
            {
                layerStart.x -= ((layers - sparseLayers - agentLayer) / 2f) * spacing;
            }
            else
            {
                agentLayerId.X -= 1;
                layerStart.x += (layers - sparseLayers - agentLayer) / 2f * spacing;
            }

            layerStart.y = (layers - 1) / 2f * spacing - agentLayer * spacing;

            ComputeTargetPosition(layerStart, 1, 0, spacing, spacing);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private Vector3 ComputePositionNonSolid()
        {
            layers = sparseLayers = 0;
            int tmpSize = 0;
            while (tmpSize < size)
            {
                layers++;
                tmpSize += 2 * agentsPerLine.Y;
            }

            int newAgentsPerLineX = 2;
            int newAgentsPerLineY = agentsPerLine.Y;
            int sparseUnits = size % (2 * agentsPerLine.Y);
            int internalLayers = layers;

            PrepareNonSolidSparseLayers(sparseUnits, ref internalLayers, ref newAgentsPerLineX);

            int normalizedPosition = positionInFormation;

            // Adjust position in layer
            if (positionInFormation >= size - sparseUnits)
            {
                ComputeNonSolidSparseLayers(sparseUnits, ref normalizedPosition,
                    ref newAgentsPerLineY, ref newAgentsPerLineX);
            }
            else
            {
                agentLayer = positionInFormation / (2 * agentsPerLine.Y);
                normalizedPosition = positionInFormation - agentLayer * 2 * agentsPerLine.Y;
            }

            agentLayerId.X = normalizedPosition % newAgentsPerLineX;
            agentLayerId.Y = normalizedPosition / newAgentsPerLineX;

            // Place agent on left or right wing
            if (agentLayerId.X < 1)
            {
                layerStart.x -= (internalLayers - agentLayer - 1) / 2f * spacing;
            }
            else
            {
                agentLayerId.X -= newAgentsPerLineX / 2;
                layerStart.x += (internalLayers - agentLayer - 1) / 2f * spacing;
            }

            layerStart.y = (agentsPerLine.Y - newAgentsPerLineY) / 2f * spacing;
            layerStart.z = (layers - 1) / 2f * spacing - agentLayer * spacing;

            ComputeTargetPosition(layerStart, 1, 1, spacing, spacing);

            return pos;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void PrepareNonSolidSparseLayers(int sparseUnits, ref int internalLayers, ref int newAgentsPerLineX)
        {
            if (sparseUnits > agentsPerLine.Y || sparseUnits == 0)
                internalLayers++;
            else if (positionInFormation >= size - sparseUnits)
                newAgentsPerLineX = 1;

            if (sparseUnits > agentsPerLine.Y && sparseUnits % 2 == 1)
            {
                layers++;
                sparseLayers = 1;
            }
            else if (sparseUnits <= agentsPerLine.Y)
            {
                sparseLayers = 1;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void ComputeNonSolidSparseLayers(int sparseAgents, ref int normalizedPosition,
            ref int newAgentsPerLineX, ref int newAgentsPerLineY)
        {
            normalizedPosition = positionInFormation - size + sparseAgents;
            agentLayer = positionInFormation / (2 * agentsPerLine.Y);

            if (sparseAgents > agentsPerLine.Y)
            {
                if (positionInFormation == size - 1 && sparseLayers == 1)
                {
                    normalizedPosition = 0;
                    newAgentsPerLineX = newAgentsPerLineY = 1;
                    agentLayer = layers - 1;
                }
                else if (sparseLayers == 0)
                {
                    newAgentsPerLineX = Mathf.CeilToInt(sparseAgents / newAgentsPerLineY);
                }
                else
                {
                    newAgentsPerLineX = Mathf.CeilToInt((sparseAgents - 1) / newAgentsPerLineY);
                }
            }
            else
            {
                newAgentsPerLineX = sparseAgents;
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
    } // class FormationV
} // namespace Polarith.AI.Move
