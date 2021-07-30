//
// MIT License
// Copyright (c) 2021 Polarith.
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
        /// Constructor that takes two arguments of type integer.
        /// </summary>
        public TupleInt(int _x, int _y)
        {
            X = _x;
            Y = _y;
        }
    } // struct TupleInt
} // namespace Polarith.Utils

