//
// MIT License
// Copyright (c) 2021 Polarith.
// See the LICENSE file in the project root for full license information.
//

using System;
using UnityEngine;

namespace Polarith.UnityUtils
{
    /// <summary>
    /// Represents a V-gizmo to be drawn within the scene view. This class is serializable so that it is suitable
    /// for the direct use in <see cref="MonoBehaviour"/> instances supporting Unity's inspector.
    /// </summary>
    [Serializable]
    public sealed class VGizmo
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
        [Tooltip("The color of the drawn V.")]
        public Color Color = new Color(1f, 235f / 255f, 4f / 255f, 50f / 255f);

        #endregion // Fields

        #region Methods ================================================================================================

        /// <summary>
        /// Displays the gizmo in the scene view.
        /// </summary>
        /// <param name="center">The center of the arrow in world coordinates.</param>
        /// <param name="sizeX">The size for the x-axis.</param>
        /// <param name="sizeY">The size for the y-axis.</param>
        /// <param name="sizeZ">The size for the z-axis.</param>
        /// <param name="widthV">The width of the v-'wings'.</param>
        /// <param name="lengthV">The length of the v-'wings'.</param>
        /// <param name="rotation">The local rotation.</param>
        public void Draw(
            Vector3 center,
            float sizeX,
            float sizeY,
            float sizeZ,
            float widthV,
            float lengthV,
            Quaternion rotation)
        {
            if (!Enabled)
                return;

            Vector3 topFrontLL = new Vector3(-sizeX, sizeY, sizeZ);
            Vector3 topFrontLR = new Vector3(-sizeX + widthV, sizeY, sizeZ);
            Vector3 topFrontRL = new Vector3(sizeX - widthV, sizeY, sizeZ);
            Vector3 topFrontRR = new Vector3(sizeX, sizeY, sizeZ);
            Vector3 topFrontCenter = new Vector3(0, sizeY - lengthV, sizeZ);
            Vector3 topBack = new Vector3(0, -sizeY, sizeZ);

            Vector3 botFrontLL = new Vector3(-sizeX, sizeY, -sizeZ);
            Vector3 botFrontLR = new Vector3(-sizeX + widthV, sizeY, -sizeZ);
            Vector3 botFrontRL = new Vector3(sizeX - widthV, sizeY, -sizeZ);
            Vector3 botFrontRR = new Vector3(sizeX, sizeY, -sizeZ);
            Vector3 botFrontCenter = new Vector3(0, sizeY - lengthV, -sizeZ);
            Vector3 botBack = new Vector3(0, -sizeY, -sizeZ);

            Gizmos.color = Color;

            // Bottom v
            Gizmos.DrawLine(rotation * botFrontLL + center, rotation * botFrontLR + center);
            Gizmos.DrawLine(rotation * botFrontLR + center, rotation * botFrontCenter + center);
            Gizmos.DrawLine(rotation * botFrontCenter + center, rotation * botFrontRL + center);
            Gizmos.DrawLine(rotation * botFrontRL + center, rotation * botFrontRR + center);
            Gizmos.DrawLine(rotation * botFrontRR + center, rotation * botBack + center);
            Gizmos.DrawLine(rotation * botBack + center, rotation * botFrontLL + center);

            // Top v
            Gizmos.DrawLine(rotation * topFrontLL + center, rotation * topFrontLR + center);
            Gizmos.DrawLine(rotation * topFrontLR + center, rotation * topFrontCenter + center);
            Gizmos.DrawLine(rotation * topFrontCenter + center, rotation * topFrontRL + center);
            Gizmos.DrawLine(rotation * topFrontRL + center, rotation * topFrontRR + center);
            Gizmos.DrawLine(rotation * topFrontRR + center, rotation * topBack + center);
            Gizmos.DrawLine(rotation * topBack + center, rotation * topFrontLL + center);

            // Connection between top and bottom
            Gizmos.DrawLine(rotation * topFrontLL + center, rotation * botFrontLL + center);
            Gizmos.DrawLine(rotation * topFrontLR + center, rotation * botFrontLR + center);
            Gizmos.DrawLine(rotation * topFrontCenter + center, rotation * botFrontCenter + center);
            Gizmos.DrawLine(rotation * topFrontRL + center, rotation * botFrontRL + center);
            Gizmos.DrawLine(rotation * topFrontRR + center, rotation * botFrontRR + center);
            Gizmos.DrawLine(rotation * topBack + center, rotation * botBack + center);
        }

        #endregion // Methods
    } // class VGizmo
} // namespace Polarith.UnityUtils
