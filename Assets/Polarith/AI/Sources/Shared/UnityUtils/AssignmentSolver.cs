//
// Copyright (c) 2016-2021 Polarith. All rights reserved.
// Licensed under the Polarith Software and Source Code License Agreement.
// See the LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using UnityEngine;
using Polarith.AI.Move;

namespace Polarith.UnityUtils
{
    /// <summary>
    /// Provides different logics to solve the <a href="https://en.wikipedia.org/wiki/Assignment_problem">assignment
    /// problem</a>.
    /// </summary>
    public static class AssignmentSolver
    {
        /// <summary>
        /// Finds assignments for a list of agents and target positions in a greedy manner. The optimization looks
        /// iteratively for the best available assignment to the current agent.
        /// </summary>
        /// <param name="agents">
        /// A list of the agents <see cref="AIMFormation"/> that should be assigned to positions inside the formation.
        /// </param>
        /// <param name="targetPositions">
        /// A list of <see cref="Vector3"/> positions inside the formation that are available to the agents.
        /// </param>
        /// <param name="maxTargets">
        /// Determines the maximum number of available targets. Note that this must be less or equal to the real number
        /// of available targets to split computation into a greedy and an optimal part.
        /// </param>
        /// <returns>
        /// An array of assignments; element <em>i</em> is the index of the assigned position from the targets list for
        /// the agent on position <em>i</em> of the agents list.
        /// </returns>
        public static int[] FindGreedy(List<AIMFormation> agents, List<Vector3> targetPositions, int maxTargets = 0)
        {
            int[] assignments = new int[agents.Count];

            // Init available targets
            List<int> available = new List<int>();
            for (int i = 0; i < agents.Count; i++)
                available.Add(i);

            // Iterate over all targets
            for (int i = 0; i < maxTargets; i++)
            {
                float shortest = float.MaxValue;
                int candidate = -1;
                int candidatePos = -1;

                // Iterate over all available agents to find nearest agent for target(i) -> greedy
                for (int j = 0; j < available.Count; j++)
                {
                    float dist = (agents[available[j]].Formation.Self.Position - targetPositions[i]).magnitude;
                    if (dist < shortest)
                    {
                        shortest = dist;
                        candidate = available[j];
                        candidatePos = j;
                    }
                }

                assignments[i] = candidate;
                available.RemoveAt(candidatePos);
            }

            return assignments;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Finds optimal assignments for a matrix of agents and costed tasks. The optimization looks for the best
        /// assignment of the first index from the input data using the
        /// <a href="https://en.wikipedia.org/wiki/Hungarian_algorithm">Hungarian Method</a>.
        /// </summary>
        /// <remarks>
        /// Note that the column/row model for agents and tasks is just a mental model. It depends on your input data,
        /// whether the first index or the second index represents the agent or the task. However, we will optimize for
        /// the first index.
        /// </remarks>
        /// <param name="costs">
        /// A cost matrix; each row contains elements that represent the associated costs of each task for the agent.
        /// </param>
        /// <returns>
        /// An array of assignments; element <em>i</em> is the index of the assigned task/position (column) for an agent
        /// (row).
        /// </returns>
        public static int[] FindOptimal(int[,] costs)
        {
            int height = costs.GetLength(0);
            int width = costs.GetLength(1);

            // First step of row optimization: adjust rows to have zero as the smallest value
            for (int i = 0; i < height; i++)
            {
                int min = int.MaxValue;
                for (int j = 0; j < width; j++)
                    min = Math.Min(min, costs[i, j]);
                for (int j = 0; j < width; j++)
                    costs[i, j] -= min;
            }

            byte[,] masks = new byte[height, width];
            bool[] rowsCovered = new bool[height];
            bool[] colsCovered = new bool[width];

            // Mark elements that are ready to be assigned
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (costs[i, j] == 0 && !rowsCovered[i] && !colsCovered[j])
                    {
                        masks[i, j] = 1;
                        rowsCovered[i] = true;
                        colsCovered[j] = true;
                    }
                }
            }
            ClearCovers(rowsCovered, colsCovered, width, height);

            ElementPosition[] path = new ElementPosition[width * height];
            ElementPosition pathStart = default(ElementPosition);
            int step = 1;
            while (step != -1)
            {
                switch (step)
                {
                    case 1:
                        step = RowOptimization(masks, colsCovered, width, height);
                        break;

                    case 2:
                        step = ColumnOptimization(costs, masks, rowsCovered, colsCovered, width, height, ref pathStart);
                        break;

                    case 3:
                        step = MarkingStep(masks, rowsCovered, colsCovered, width, height, path, pathStart);
                        break;

                    case 4:
                        step = MinimumOperation(costs, rowsCovered, colsCovered, width, height);
                        break;
                }
            }

            int[] agentsTasks = new int[height];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (masks[i, j] == 1)
                    {
                        agentsTasks[i] = j;
                        break;
                    }
                }
            }
            return agentsTasks;
        }

        //--------------------------------------------------------------------------------------------------------------

        private static int RowOptimization(byte[,] masks, bool[] colsCovered, int width, int height)
        {
            // Check for assigned elements, columns respectively
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (masks[i, j] == 1)
                        colsCovered[j] = true;
                }
            }
            int colsCoveredCount = 0;
            for (int j = 0; j < width; j++)
            {
                if (colsCovered[j])
                    colsCoveredCount++;
            }

            // Finish or perform column optimization
            if (colsCoveredCount == height)
                return -1;
            else
                return 2;
        }

        //--------------------------------------------------------------------------------------------------------------

        private static int ColumnOptimization(int[,] costs, byte[,] masks, bool[] rowsCovered, bool[] colsCovered,
            int width, int height, ref ElementPosition pathStart)
        {
            ElementPosition pos;
            while (true)
            {
                // Find a zero element that is not yet assigned
                pos = FindZero(costs, rowsCovered, colsCovered, width, height);
                if (pos.Row == -1)
                {
                    return 4;
                }
                else
                {
                    masks[pos.Row, pos.Column] = 2;
                    int starCol = FindStarInRow(masks, width, pos.Row);
                    if (starCol != -1)
                    {
                        // Mark row that has a new assignment
                        rowsCovered[pos.Row] = true;
                        colsCovered[starCol] = false;
                    }
                    else
                    {
                        // Mark row that has a no assignments at all
                        pathStart = pos;
                        return 3;
                    }
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private static int MarkingStep(byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int width,
            int height, ElementPosition[] path, ElementPosition pathStart)
        {
            int pathIndex = 0;
            path[0] = pathStart;
            while (true)
            {
                // Find row in unmarked column, that has an assignment
                int row = FindStarInColumn(masks, height, path[pathIndex].Column);
                if (row == -1)
                    break;
                pathIndex++;
                path[pathIndex] = new ElementPosition(row, path[pathIndex - 1].Column);

                // Find column that is crossing an newly marked row
                int col = FindNewZeroInRow(masks, width, path[pathIndex].Row);
                pathIndex++;
                path[pathIndex] = new ElementPosition(path[pathIndex - 1].Row, col);
            }

            // Update masks to perform row optimization
            ConvertPath(masks, path, pathIndex + 1);
            ClearCovers(rowsCovered, colsCovered, width, height);
            ClearNewZeros(masks, width, height);
            return 1;
        }

        //--------------------------------------------------------------------------------------------------------------

        private static int MinimumOperation(int[,] costs, bool[] rowsCovered, bool[] colsCovered, int width, int height)
        {
            // Find minimum of all elements that have not been marked in the previous steps
            // Subtract minimum from every unmarked element, add it to every element covered
            // by a marked column and marked row
            int minValue = FindMinimum(costs, rowsCovered, colsCovered, width, height);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (rowsCovered[i])
                        costs[i, j] += minValue;
                    if (!colsCovered[j])
                        costs[i, j] -= minValue;
                }
            }
            return 2;
        }

        //--------------------------------------------------------------------------------------------------------------

        private static void ConvertPath(byte[,] masks, ElementPosition[] path, int pathLength)
        {
            for (int i = 0; i < pathLength; i++)
            {
                if (masks[path[i].Row, path[i].Column] == 1)
                    masks[path[i].Row, path[i].Column] = 0;
                else if (masks[path[i].Row, path[i].Column] == 2)
                    masks[path[i].Row, path[i].Column] = 1;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private static ElementPosition FindZero(int[,] costs, bool[] rowsCovered, bool[] colsCovered,
            int width, int height)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (costs[i, j] == 0 && !rowsCovered[i] && !colsCovered[j])
                        return new ElementPosition(i, j);
                }
            }
            return new ElementPosition(-1, -1);
        }

        //--------------------------------------------------------------------------------------------------------------

        private static int FindMinimum(int[,] costs, bool[] rowsCovered, bool[] colsCovered, int width, int height)
        {
            int minValue = int.MaxValue;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (!rowsCovered[i] && !colsCovered[j])
                        minValue = Math.Min(minValue, costs[i, j]);
                }
            }
            return minValue;
        }

        //--------------------------------------------------------------------------------------------------------------

        private static int FindStarInRow(byte[,] masks, int width, int row)
        {
            for (int j = 0; j < width; j++)
            {
                if (masks[row, j] == 1)
                    return j;
            }
            return -1;
        }

        //--------------------------------------------------------------------------------------------------------------

        private static int FindStarInColumn(byte[,] masks, int height, int col)
        {
            for (int i = 0; i < height; i++)
            {
                if (masks[i, col] == 1)
                    return i;
            }
            return -1;
        }

        //--------------------------------------------------------------------------------------------------------------

        private static int FindNewZeroInRow(byte[,] masks, int width, int row)
        {
            for (int j = 0; j < width; j++)
            {
                if (masks[row, j] == 2)
                    return j;
            }
            return -1;
        }

        //--------------------------------------------------------------------------------------------------------------

        private static void ClearCovers(bool[] rowsCovered, bool[] colsCovered, int width, int height)
        {
            for (int i = 0; i < height; i++)
                rowsCovered[i] = false;
            for (int j = 0; j < width; j++)
                colsCovered[j] = false;
        }

        //--------------------------------------------------------------------------------------------------------------

        private static void ClearNewZeros(byte[,] masks, int width, int height)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (masks[i, j] == 2)
                        masks[i, j] = 0;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private struct ElementPosition
        {
            public int Row;
            public int Column;

            public ElementPosition(int row, int col)
            {
                Row = row;
                Column = col;
            }
        }
    } // class AssignmentSolver
} // namespace Polarith.AI.Move
