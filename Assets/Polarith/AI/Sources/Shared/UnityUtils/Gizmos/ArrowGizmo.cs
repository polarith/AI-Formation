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
    /// Represents an arrow gizmo to be drawn within the scene view. This class is serializable so that it is suitable
    /// for the direct use in <see cref="MonoBehaviour"/> instances supporting Unity's inspector.
    /// </summary>
    [Serializable]
    public sealed class ArrowGizmo
    {
        #region Fields =================================================================================================

        /// <summary>
        /// Determines if this gizmo is enabled.
        /// </summary>
        [Tooltip("Determines if this gizmo is enabled.")]
        public bool Enabled = false;

        /// <summary>
        /// The color of the drawn arrow.
        /// </summary>
        [Tooltip("The color of the drawn arrow.")]
        public Color Color = new Color(1f, 235f / 255f, 4f / 255f, 50f / 255f);

        /// <summary>
        /// Enables diagonals on the back face for orientation in 3-dimensional space.
        /// </summary>
        [Tooltip("Enables diagonals on the back face for orientation in 3-dimensional space.")]
        public bool Marker = true;

        #endregion // Fields

        #region Methods ================================================================================================

        /// <summary>
        /// Displays the gizmo in the scene view.
        /// </summary>
        /// <param name="center">The center of the arrow in world coordinates.</param>
        /// <param name="sizeX">The size for the x-axis.</param>
        /// <param name="sizeY">The size for the y-axis.</param>
        /// <param name="sizeZ">The size for the z-axis.</param>
        /// <param name="rotation">The local rotation.</param>
        public void Draw(
            Vector3 center,
            float sizeX, 
            float sizeY, 
            float sizeZ,
            Quaternion rotation)
        {
            if (!Enabled)
                return;

            Vector3 tip = new Vector3(0, 0, sizeZ);
            Vector3 topL = new Vector3(-sizeX, sizeY, -sizeZ);
            Vector3 topR = new Vector3(sizeX, sizeY, -sizeZ);
            Vector3 botL = new Vector3(-sizeX, -sizeY, -sizeZ);
            Vector3 botR = new Vector3(sizeX, -sizeY, -sizeZ);

            Gizmos.color = Color;

            // Bottom triangle
            Gizmos.DrawLine(rotation * tip + center, rotation * botL + center);
            Gizmos.DrawLine(rotation * tip + center, rotation * botR + center);
            Gizmos.DrawLine(rotation * botL + center, rotation * botR + center);
            // Top triangle
            Gizmos.DrawLine(rotation * tip + center, rotation * topL + center);
            Gizmos.DrawLine(rotation * tip + center, rotation * topR + center);
            Gizmos.DrawLine(rotation * topL + center, rotation * topR + center);
            // Connection between top and bottom triangle
            Gizmos.DrawLine(rotation * botL + center, rotation * topL + center);
            Gizmos.DrawLine(rotation * botR + center, rotation * topR + center);

            // Orientation markers
            if (Marker)
                Gizmos.DrawLine(rotation * botL + center, rotation * topR + center);
        }

        #endregion // Methods
    } // class ArrowGizmo
} // namespace Polarith.UnityUtils
