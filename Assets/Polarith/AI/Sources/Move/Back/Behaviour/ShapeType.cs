//
// MIT License
// Copyright (c) 2021 Polarith.
// See the LICENSE file in the project root for full license information.
//

namespace Polarith.AI.Move
{
    /// <summary>
    /// Defines the visual shape of the formation. You select whether the formation should be build as 2D or 3D
    /// version, i.e., as a flat rectangle or as a cuboid. The concrete shape depends on the corresponding formation
    /// type.
    /// </summary>
    public enum ShapeType
    {
        /// <summary>
        /// 2-dimensional representation.
        /// </summary>
        Planar,

        /// <summary>
        /// 3-dimensional representation.
        /// </summary>
        Spatial
    } // enum ShapeType
} // namespace Polarith.AI.Move
