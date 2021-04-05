//
// Copyright (c) 2016-2021 Polarith. All rights reserved.
// Licensed under the Polarith Software and Source Code License Agreement.
// See the LICENSE file in the project root for full license information.
//

using System;
using UnityEngine;

namespace Polarith.Utils
{
    /// <summary>
    /// Serializable struct to store two variables (X,Y) of type integer. The serialization is necessary for custom 
    /// editors.
    /// </summary>
    [Serializable]
    public struct TupleInt
    {
        /// <summary>
        /// First integer value.
        /// </summary>
        [SerializeField]
        public int X;

        /// <summary>
        /// Second integer value.
        /// </summary>
        [SerializeField]
        public int Y;

        /// <summary>
        /// Contructor that takes two arguments of type integer.
        /// </summary>
        public TupleInt(int _x, int _y)
        {
            X = _x;
            Y = _y;
        }
    } // struct TupleInt
} // namespace Polarith.Utils

