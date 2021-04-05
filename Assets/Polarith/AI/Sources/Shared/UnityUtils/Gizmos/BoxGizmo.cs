//
// Copyright (c) 2016-2021 Polarith. All rights reserved.
// Licensed under the Polarith Software and Source Code License Agreement.
// See the LICENSE file in the project root for full license information.
//

using System;
using UnityEngine;

namespace Polarith.UnityUtils
{
    /// <summary>
    /// Represents a box gizmo to be drawn within the scene view. This class is serializable so that it is suitable for
    /// the direct use in <see cref="MonoBehaviour"/> instances supporting Unity's inspector.
    /// </summary>
    [Serializable]
    public sealed class BoxGizmo
    {
        #region Fields =================================================================================================

        /// <summary>
        /// Determines if this gizmo is enabled.
        /// </summary>
        [Tooltip("Determines whether this gizmo is enabled.")]
        public bool Enabled = false;

        /// <summary>
        /// The color of the drawn box.
        /// </summary>
        [Tooltip("The color of the drawn box.")]
        public Color Color = new Color(1f, 235f / 255f, 4f / 255f, 50f / 255f);

        /// <summary>
        /// Enables diagonals on the front and back face for orientation in 3-dimensional space.
        /// </summary>
        [Tooltip("Enables diagonals on the front and back face for orientation in 3-dimensional space.")]
        public bool Markers = true;

        #endregion // Fields

        #region Methods ================================================================================================

        /// <summary>
        /// Displays the gizmo in the scene view.
        /// </summary>
        /// <param name="center">The center of the box in world coordinates.</param>
        /// <param name="sizeX">The size for the x-axis.</param>
        /// <param name="sizeY">The size for the y-axis.</param>
        /// <param name="sizeZ">The size for the z-axis.</param>
        /// <param name="rotation">The local rotation.</param>
        public void Draw(
            Vector3 center,
            float sizeX, float sizeY, float sizeZ,
            Quaternion rotation)
        {
            if (!Enabled)
                return;

            Vector3 bottomFL = new Vector3(-sizeX, -sizeY, sizeZ);
            Vector3 bottomFR = new Vector3(sizeX, -sizeY, sizeZ);
            Vector3 bottomBL = new Vector3(-sizeX, -sizeY, -sizeZ);
            Vector3 bottomBR = new Vector3(sizeX, -sizeY, -sizeZ);
            Vector3 topFL = new Vector3(-sizeX, sizeY, sizeZ);
            Vector3 topFR = new Vector3(sizeX, sizeY, sizeZ);
            Vector3 topBL = new Vector3(-sizeX, sizeY, -sizeZ);
            Vector3 topBR = new Vector3(sizeX, sizeY, -sizeZ);

            Gizmos.color = Color;

            // Bottom rectangle
            Gizmos.DrawLine(rotation * bottomFL + center, rotation * bottomFR + center);
            Gizmos.DrawLine(rotation * bottomFR + center, rotation * bottomBR + center);
            Gizmos.DrawLine(rotation * bottomBR + center, rotation * bottomBL + center);
            Gizmos.DrawLine(rotation * bottomBL + center, rotation * bottomFL + center);
            // Top rectangle
            Gizmos.DrawLine(rotation * topFL + center, rotation * topFR + center);
            Gizmos.DrawLine(rotation * topFR + center, rotation * topBR + center);
            Gizmos.DrawLine(rotation * topBR + center, rotation * topBL + center);
            Gizmos.DrawLine(rotation * topBL + center, rotation * topFL + center);
            // Connection between top and bottom rectangle
            Gizmos.DrawLine(rotation * bottomFL + center, rotation * topFL + center);
            Gizmos.DrawLine(rotation * bottomFR + center, rotation * topFR + center);
            Gizmos.DrawLine(rotation * bottomBR + center, rotation * topBR + center);
            Gizmos.DrawLine(rotation * bottomBL + center, rotation * topBL + center);

            // Orientation markers
            if (Markers)
            {
                Gizmos.DrawLine(rotation * bottomFR + center, rotation * topFL + center);
                Gizmos.DrawLine(rotation * bottomBL + center, rotation * topBR + center);
            }
        }

        #endregion // Methods
    } // class BoxGizmo
} // namespace Polarith.UnityUtils
