//
// MIT License
// Copyright (c) 2021 Polarith.
// See the LICENSE file in the project root for full license information.
//

namespace Polarith.AI.Move
{
    /// <summary>
    /// Used as constraint version of <see cref="MappingType"/> to avoid inverse mapping. Cast this type to 
    /// <see cref="MappingType"/> to use within the 
    /// <see cref="MoveBehaviour.MapSpecialSqr(MappingType, float, float, float)"/> and <see
    /// cref="MoveBehaviour.MapSpecial(MappingType, float, float, float)"/> methods in order to specify the desired 
    /// type of the applied mapping function.
    /// </summary>
    public enum NonInverseMappingType
    {
        /// <summary>
        /// Results in 1 constantly.
        /// </summary>
        Constant,

        /// <summary>
        /// Maps linearly from the min/max interval to 0 and 1.
        /// </summary>
        Linear,

        /// <summary>
        /// Applies a quadratic mapping from the min/max interval to 0 and 1.
        /// </summary>
        Quadratic,

        /// <summary>
        /// Applies a square root mapping from the min/max interval to 0 and 1.
        /// </summary>
        SquareRoot
    } // enum NonInverseMappingType
} // namespace Polarith.AI.Move
