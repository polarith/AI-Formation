//
// MIT License
// Copyright (c) 2021 Polarith.
// See the LICENSE file in the project root for full license information.
//

using Polarith.Utils;
using UnityEngine;

namespace Polarith.AI.Move
{
    /// <summary>
    /// This class extends the <see cref="SteeringBehaviour"/> through adding general local information about an agent's
    /// position inside of a formation. The specific shape is defined in derived behaviours.
    ///
    /// <see cref="Formation"/> computes the position of a single agent as part of a formation, i.e., it's 
    /// decentralized. The position of a single agent is primarily based on the <see cref="Size"/> of the 
    /// whole formation, the <see cref="PositionInFormation"/> as its place inside the formation, and the
    /// <see cref="Spacing"/> as horizontal and vertical distance between the agents. <br/>
    /// <see cref="AIMFormation.TargetObject"/>, <see cref="AIMFormation.TargetPosition"/>, or
    /// <see cref="AIMFormation.TargetTag"/> may be considered in <see cref="ComputePosition"/>() in derived
    /// classes to move the formation to a specific position or to follow an object that deals as target agent for the
    /// whole formation.<br/>
    /// The agents' computed target position results in <see cref="SteeringBehaviour.ResultDirection"/> as a vector from
    /// its current position towards its target position. The <see cref="SteeringBehaviour.ResultMagnitude"/> is based
    /// on the <see cref="DistanceMapping"/>. Note that the mapping can't be inverse.<br/>
    /// The agent may slow down near its target position within the <see cref="ArriveRadius"/>, or accelerate
    /// between the <see cref="InnerCatchUpRadius"/> and the <see cref="OuterCatchUpRadius"/> to
    /// move a faster than its neighboring agents. Thus, it's able to keep up to its target position even if the
    /// formation rotates. The acceleration is scalable with <see cref="CatchUpMultiplier"/>.
    ///
    /// Every derived <see cref="Formation"/> needs to implement <see cref="ComputePosition"/>() to set the
    /// exact position based on the specific formation shape.
    ///
    /// Base back-end behaviour of every derived <see cref="AIMFormation"/>
    /// </summary>
    public abstract class Formation : SteeringBehaviour
    {
        #region Fields =================================================================================================

        /// <summary>
        /// Maximum number of agents in the formation.
        /// </summary>
        [Tooltip("Maximum number of agents in the formation. Affects the overall formation shape.")]
        [SerializeField]
        protected int size = 1;

        /// <summary>
        /// Logical position of the agent in the formation, e.g. (1/10), where 10 is the <see cref="Size"/>.
        /// Note that the logical position is zero-based, so the corresponding positions would be in a range from 0 - 9.
        /// </summary>
        [Tooltip("Logical position of the agent in the formation, e.g., (1/10), where 10 is the Size." +
            "Note that the logical position is zero-based, so the corresponding positions would be in a range " +
            "from 0 - 9.")]
        [SerializeField]
        protected int positionInFormation = 0;

        /// <summary>
        /// Distance between each agent in the formation.
        /// </summary>
        [Tooltip("Distance between each adjacent agent in the formation based on its center position.")]
        [SerializeField]
        protected float spacing = 5f;

        /// <summary>
        /// Radius within the agent slows down to arrive the target position. Magnitude is between 0 and 1;
        /// </summary>
        [Tooltip("Radius within the agent slows down to arrive the target position. This radius must be smaller " +
            "or equal to the Inner Catch Up Radius.")]
        [SerializeField]
        protected float arriveRadius = 0f;

        /// <summary>
        /// Inner radius within the agent keeps its to keep next to the target position. This
        /// radius must be greater or equal to the <see cref="ArriveRadius"/>, but smaller than the
        /// <see cref="OuterCatchUpRadius"/>. This is necessary if the formation rotates, since the outer agents need 
        /// to move a longer distance. The magnitude is constantly 1 between the <see cref="ArriveRadius"/> and
        /// the <see cref="InnerCatchUpRadius"/>, or 1 + a value between 0 and 1, based on the distance to the
        /// <see cref="InnerCatchUpRadius"/> and <see cref="OuterCatchUpRadius"/>, times the
        /// <see cref="CatchUpMultiplier"/>.
        /// </summary>
        [Tooltip("Inner radius within the agent keeps its speed constant to keep next to the target position. " +
            "This radius must be greater or equal to the Arrive Radius, but smaller or equal to the " +
            "Outer Catch Up Radius." +
            "This is necessary if the formation rotates, since the outer agents need to move a longer distance.")]
        [SerializeField]
        protected float innerCatchUpRadius = 0f;

        /// <summary>
        /// Outer radius within the agent accelerates to keep next to the target position. This radius must be greater
        /// or equal to the <see cref="InnerCatchUpRadius"/>. This is necessary if the formation rotates, since the
        /// outer agents need to move a longer distance. The magnitude is 1 + a value between 0 and 1, based on the
        /// distance to the <see cref="InnerCatchUpRadius"/> and <see cref="OuterCatchUpRadius"/>, times the
        /// <see cref="CatchUpMultiplier"/>.
        /// </summary>
        [Tooltip("Outer radius within the agent accelerates to keep next to the target position. " +
            "This radius must be greater or equal to the Inner Catch Up Radius. " +
            "This is necessary if the formation rotates, since the outer agents need to move a longer distance.")]
        [SerializeField]
        protected float outerCatchUpRadius = 0;

        /// <summary>
        /// An additional multiplier to obtain values bigger than the standard magnitude. The standard magnitude is
        /// equal to 1 and is used between the <see cref="ArriveRadius"/> and the <see cref="InnerCatchUpRadius"/> to
        /// have a constant magnitude. The <see cref="CatchUpMultiplier"/> is multiplied with a value between 0 and 1,
        /// based on the distance to the <see cref="InnerCatchUpRadius"/> and <see cref="OuterCatchUpRadius"/>,
        /// resulting in magnitudes between 1 + [0,1] * <see cref="CatchUpMultiplier"/>.
        /// </summary>
        [Tooltip("An additional multiplier to obtain values bigger than the standard magnitude. The standard " +
            "magnitude is equal to 1 and is used between the Arrive Radius and the Inner Catch Up Radius to " +
            "have a constant magnitude. The Multiplier is used to gain an even higher acceleration if the agent is " +
            "between the Inner- and the Outer Catch Up Radius, that is added to the standard magnitude.")]
        [SerializeField]
        protected float catchUpMultiplier = 1f;

        /// <summary>
        /// Specifies the mapping type of the distance from the agent to its target position.
        /// </summary>
        [Tooltip("Mapping type of the distance between the agent and the target position in the formation.")]
        [SerializeField]
        protected NonInverseMappingType distanceMapping = NonInverseMappingType.Linear;

        /// <summary>
        /// The number of layers to cover agents dynamically (grows in-depth).
        /// </summary>
        [Tooltip("The number of layers to cover agents dynamically (grows in depth).")]
        protected int layers = 1;

        //--------------------------------------------------------------------------------------------------------------

        private bool init = false;
        private Vector3 resultVector;
        private Vector3 resultPosition;
        private MappingType internalMapping = MappingType.Linear;

        #endregion // Fields

        #region Properties =============================================================================================

        /// <summary>
        /// Maximum number of agents in the formation.
        /// </summary>
        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                if (value < 0)
                {
                    size = 1;
                    Debug.LogWarning("Size needs to be at least 1! Size has been set to 1!");
                }
                else
                {
                    size = value;
                }

                resultPosition = ComputePosition();
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Logical position of the agent in the formation, e.g. (1/10), where 10 is the <see cref="Size"/>.
        /// Note that the logical position is zero-based, so the corresponding positions would be in a range from 0 - 9.
        /// </summary>
        public int PositionInFormation
        {
            get
            {
                return positionInFormation;
            }
            set
            {
                if (value < 0)
                {
                    positionInFormation = 0;
                    Debug.LogWarning("PositionInFormation needs to be at least 0! Value has been set to 0!");
                }
                else
                {
                    positionInFormation = value;
                }

                resultPosition = ComputePosition();
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Distance between each agent in the formation.
        /// </summary>
        public float Spacing
        {
            get
            {
                return spacing;
            }
            set
            {
                if (value < 0)
                {
                    spacing = 0;
                    Debug.LogWarning("Spacing needs to be at least 0! Value has been set to 0!");
                }
                else
                {
                    spacing = value;
                }

                resultPosition = ComputePosition();
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Radius within the agent slows down to arrive the target position. Magnitude is between 0 and 1;
        /// </summary>
        public float ArriveRadius
        {
            get
            {
                return arriveRadius;
            }
            set
            {
                if (value < 0)
                {
                    arriveRadius = 0;
                    Debug.LogWarning("ArriveRadius needs to be at least 0! Value has been set to 0!");
                }
                else if (value > innerCatchUpRadius)
                {
                    arriveRadius = innerCatchUpRadius;
                    Debug.LogWarning("ArriveRadius needs to be smaller or equal to the InnerCatchUpRadius! " +
                        "Value has been set to InnerCatchUpRadius!");
                }
                else
                {
                    arriveRadius = value;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Inner radius within the agent keeps its to keep next to the target position. This
        /// radius must be greater or equal to the <see cref="ArriveRadius"/>, but smaller than the
        /// <see cref="OuterCatchUpRadius"/>. This is necessary if the formation rotates, since the outer agents need 
        /// to move a longer distance. The magnitude is constantly 1 between the <see cref="ArriveRadius"/> and
        /// the <see cref="InnerCatchUpRadius"/>, or 1 + a value between 0 and 1, based on the distance to the
        /// <see cref="InnerCatchUpRadius"/> and <see cref="OuterCatchUpRadius"/>, times the
        /// <see cref="CatchUpMultiplier"/>.
        /// </summary>
        public float InnerCatchUpRadius
        {
            get
            {
                return innerCatchUpRadius;
            }
            set
            {
                if (value < arriveRadius)
                {
                    innerCatchUpRadius = arriveRadius;
                    Debug.LogWarning("InnerCatchUpRadius needs to be greater or equal to the ArriveRadius! " +
                        "Value has been set to ArriveRadius!");
                }
                else if (value > outerCatchUpRadius)
                {
                    innerCatchUpRadius = outerCatchUpRadius;
                    Debug.LogWarning("InnerCatchUpRadius needs to be smaller or equal to the OuterCatchUpRadius! " +
                        "Value has been set to OuterCatchUpRadius!");
                }
                else
                {
                    innerCatchUpRadius = value;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Outer radius within the agent accelerates to keep next to the target position. This radius must be greater
        /// or equal to the <see cref="InnerCatchUpRadius"/>. This is necessary if the formation rotates, since the
        /// outer agents need to move a longer distance. The magnitude is 1 + a value between 0 and 1, based on the
        /// distance to the <see cref="InnerCatchUpRadius"/> and <see cref="OuterCatchUpRadius"/>, times the
        /// <see cref="CatchUpMultiplier"/>.
        /// </summary>
        public float OuterCatchUpRadius
        {
            get
            {
                return outerCatchUpRadius;
            }
            set
            {
                if (value < innerCatchUpRadius)
                {
                    outerCatchUpRadius = innerCatchUpRadius;
                    Debug.LogWarning("OuterCatchUpRadius needs to be greater or equal to the InnerCatchUpRadius! " +
                        "Value has been set to InnerCatchUpRadius");
                }
                else
                {
                    value = outerCatchUpRadius;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// An additional multiplier to obtain values bigger than the standard magnitude. The standard magnitude is
        /// equal to 1 and is used between the <see cref="ArriveRadius"/> and the <see cref="InnerCatchUpRadius"/> to
        /// have a constant magnitude. The <see cref="CatchUpMultiplier"/> is multiplied with a value between 0 and 1,
        /// based on the distance to the <see cref="InnerCatchUpRadius"/> and <see cref="OuterCatchUpRadius"/>,
        /// resulting in magnitudes between 1 + [0,1] * <see cref="CatchUpMultiplier"/>.
        /// </summary>
        public float CatchUpMultiplier
        {
            get { return catchUpMultiplier; }
            set { catchUpMultiplier = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Specifies the mapping type of the distance from the agent to its target position.
        /// </summary>
        public NonInverseMappingType DistanceMapping
        {
            get
            {
                return distanceMapping;
            }
            set
            {
                distanceMapping = value;
                switch (distanceMapping)
                {
                    case NonInverseMappingType.Constant:
                        internalMapping = MappingType.Constant;
                        break;

                    case NonInverseMappingType.Linear:
                        internalMapping = MappingType.Linear;
                        break;

                    case NonInverseMappingType.Quadratic:
                        internalMapping = MappingType.Quadratic;
                        break;

                    case NonInverseMappingType.SquareRoot:
                        internalMapping = MappingType.SquareRoot;
                        break;

                    default:
                        Debug.LogError("No valid Distance Mapping selected!");
                        break;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The number of layers to cover agents dynamically (grows in-depth).
        /// </summary>
        public int Layers
        {
            get { return layers; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true since only percept steering is supported.
        /// </summary>
        protected override bool forEachPercept
        {
            get { return true; }
        }

        //--------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Returns false since only percept steering is supported.
        /// </summary>
        protected override bool forEachReceptor
        {
            get { return false; }
        }

        #endregion // Properties

        #region Methods ================================================================================================

        /// <summary>
        /// Abstract method for building up the formation (compute position for individual agent).
        /// </summary>
        public abstract Vector3 ComputePosition();

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Recomputes and sets the resulting position for the agent in the formation. Call this method if you
        /// update parameters that affect the agents position.
        /// </summary>
        public void UpdateResultPosition()
        {
            resultPosition = ComputePosition();
            init = true;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Processes the steering algorithm for each percept using the same data for each processed receptor. Also 
        /// considers the radii of the catch up system for mapping the result.
        /// </summary>
        protected override void PerceptSteering()
        {
            if (!init)
            {
                resultPosition = ComputePosition();
                init = true;
            }

            resultVector = percept.Rotation * resultPosition + percept.Position;
            resultVector -= Self.Position;

            ResultDirection = resultVector.normalized;
            if (resultVector.magnitude <= ArriveRadius)
                ResultMagnitude = MapSpecial(internalMapping, 0f, ArriveRadius, resultVector.magnitude);
            else if (resultVector.magnitude < InnerCatchUpRadius)
                ResultMagnitude = 1f;
            else if (resultVector.magnitude >= InnerCatchUpRadius)
            {
                ResultMagnitude = 1f + CatchUpMultiplier *
                    MapSpecial(internalMapping, InnerCatchUpRadius, OuterCatchUpRadius, resultVector.magnitude);
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Flips a vector from XY-plane according to an up vector. Useful to transform a vector lying in the
        /// XZ-plane if the attached <see cref="AIMPlanarSensor"/> is not in the XY-plane, e.g., XZ-plane. Note that
        /// <c>vec</c> is a reference, and thus, is changed inside the function.
        /// </summary>
        /// <param name="up">3d vector that is perpendicular to the target plane</param>
        /// <param name="vec">Reference to the target vector that should be flipped.</param>
        protected void ChangeOrientation(Vector3 up, ref Vector3 vec)
        {
            Vector3 tmp = vec;

            if (!Mathf2.Approximately(up.x, 0))
            {
                vec.x = tmp.z * Mathf.Sign(up.x);
                vec.z = tmp.x;
            }
            else if (!Mathf2.Approximately(up.y, 0))
            {
                vec.y = tmp.z * Mathf.Sign(up.x);
                vec.z = tmp.y;
            }
            else if (!Mathf2.Approximately(up.z, 0))
            {
                vec.y = tmp.x * Mathf.Sign(up.z);
                vec.x = tmp.y;
            }
        }

        #endregion // Methods
    } // class Formation
} // namespace Polarith.AI.Move
