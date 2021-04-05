//
// Copyright (c) 2016-2021 Polarith. All rights reserved.
// Licensed under the Polarith Software and Source Code License Agreement.
// See the LICENSE file in the project root for full license information.
//

namespace Polarith.Utils
{
    /// <summary>
    /// Struct to store two variables (X,Y) of any type.
    /// </summary>
    public struct Tuple<T, U>
    {
        /// <summary>
        /// First value.
        /// </summary>
        public T X;

        /// <summary>
        /// Second value.
        /// </summary>
        public U Y;

        /// <summary>
        /// Contructor that takes two arguments of any type.
        /// </summary>
        public Tuple(T _x, U _y)
        {
            X = _x;
            Y = _y;
        }
    } // struct Tuple
} // namespace Polarith.Utils

