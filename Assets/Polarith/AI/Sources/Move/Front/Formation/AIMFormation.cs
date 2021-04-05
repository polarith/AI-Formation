//
// Copyright (c) 2016-2021 Polarith. All rights reserved.
// Licensed under the Polarith Software and Source Code License Agreement.
// See the LICENSE file in the project root for full license information.
//

using Polarith.UnityUtils;
using UnityEngine;

namespace Polarith.AI.Move
{
    /// <summary>
    /// <see cref="AIMFormation"/> provides general information of a formation to compute an agent's position in
    /// derived formations based on its position in the formation, the formation size, and the reference object or
    /// position.
    /// Reference object or position as <see cref="TargetObject"/>,
    /// <see cref="TargetPosition"/>, or <see cref="TargetTag"/> is considered in the 
    /// back-end behaviour <see cref="Formation"/> to obtain a transformation of the formation to the reference, i.e., 
    /// the formation's center.
    ///
    /// Front-end component of all underlying <see cref="Formation"/> classes.
    /// </summary>
    public abstract class AIMFormation : AIMSteeringBehaviour
    {
        #region Fields =================================================================================================

        /// <summary>
        /// Sets up the visualization of the inner radius (editor only).
        /// </summary>
        [Tooltip("Sets up the visualization of the inner radius.")]
        [SerializeField]
        protected WireSphereGizmo innerRadiusGizmo = new WireSphereGizmo();

        /// <summary>
        /// Sets up the visualization of the outer radius (editor only).
        /// </summary>
        [Tooltip("Sets up the visualization of the outer radius.")]
        [SerializeField]
        protected WireSphereGizmo outerRadiusGizmo = new WireSphereGizmo();

        /// <summary>
        /// Sets up the visualization of the arrive radius (editor only).
        /// </summary>
        [Tooltip("Sets up the visualization of the outer radius for planar sensor shapes.")]
        [SerializeField]
        protected WireSphereGizmo arriveRadiusGizmo = new WireSphereGizmo();

        /// <summary>
        /// Sets up the visualization of the inner radius for planar sensor shapes (editor only).
        /// </summary>
        [Tooltip("Sets up the visualization of the inner radius for planar sensor shapes.")]
        [SerializeField]
        protected CircleGizmo innerCircleGizmo = new CircleGizmo();

        /// <summary>
        /// Sets up the visualization of the outer radius for planar sensor shapes (editor only).
        /// </summary>
        [Tooltip("Sets up the visualization of the outer radius for planar sensor shapes.")]
        [SerializeField]
        protected CircleGizmo outerCircleGizmo = new CircleGizmo();

        /// <summary>
        /// Sets up the visualization of the arrive radius for planar sensor shapes (editor only).
        /// </summary>
        [Tooltip("Sets up the visualization of the outer radius for planar sensor shapes.")]
        [SerializeField]
        protected CircleGizmo arriveCircleGizmo = new CircleGizmo();

        //--------------------------------------------------------------------------------------------------------------

        [Tooltip("The target game object used by the agent as reference for the formation.")] // Based on transform
        [SerializeField]
        private GameObject targetObject;

        [Tooltip("The target position used by the agent as reference, therefore, the 'Target Object' must be 'null'.")]
        [SerializeField]
        private Vector3 targetPosition;

        [Tooltip("The target tag to get the Target Object by its tag, therefore, the 'Target Object' must be 'null'.")]
        [Tag, SerializeField]
        private string targetTag = "Untagged";

#pragma warning disable 414

        [SerializeField]
        [HideInInspector]
        private bool formationFoldout = true;

        [SerializeField]
        [HideInInspector]
        private int referenceIndex = 0;

#pragma warning restore 414

        #endregion // Fields

        #region Properties =============================================================================================

        /// <summary>
        /// The target game object used by the agent as reference for the formation.
        /// </summary>
        public GameObject TargetObject
        {
            get { return targetObject; }
            set { targetObject = value; }
        }

        //--------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// The target position used by the agent as reference, therefore, the <see cref="TargetObject"/> must be
        /// <c>null</c>.
        /// </summary>
        public Vector3 TargetPosition
        {
            get { return targetPosition; }
            set { targetPosition = value; }
        }

        //--------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// The target tag to get the <see cref="TargetObject"/> by its tag, therefore, the <see
        /// cref="targetObject"/> must be <c>null</c>.
        /// </summary>
        public string TargetTag
        {
            get { return targetTag; }
            set { targetTag = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Polymorphic reference to the underlying back-end class <see cref="Move.Formation"/> (read only).
        /// <para/>
        /// Needs to be implemented by derived components.
        /// </summary>
        public abstract Formation Formation { get; }

        #endregion // Properties

        #region Methods ================================================================================================

        /// <summary>
        /// When <see cref="AIMSteeringBehaviour.PrepareEvaluation"/> is called, this method is used in order to
        /// transfer the data from <see cref="TargetObject"/> or <see cref="TargetPosition"/> to <see
        /// cref="AIMPerceptBehaviour{T}.GameObjects"/>.
        /// </summary>
        public override void PrepareEvaluation()
        {
            if (FilteredEnvironments.Count != 0)
                FilteredEnvironments.Clear();

            if (GameObjects.Count == 1)
            {
                GameObjects[0] = targetObject;
            }
            else
            {
                GameObjects.Clear();
                GameObjects.Add(targetObject);
            }

            base.PrepareEvaluation();

            if (targetObject == null)
            {
                PerceptBehaviour.Percepts[0].Position = targetPosition;
                PerceptBehaviour.Percepts[0].Rotation = Quaternion.identity;
                PerceptBehaviour.Percepts[0].Active = true;
                PerceptBehaviour.Percepts[0].Significance = 1f;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Visualizes the core functions of a <see cref="Formation"/> such as the radii of the catch-up system.
        /// </summary>
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            outerCircleGizmo.Color = outerRadiusGizmo.Color;
            innerCircleGizmo.Color = innerRadiusGizmo.Color;
            arriveCircleGizmo.Color = arriveRadiusGizmo.Color;

            // Draw Gizmos
            if (outerRadiusGizmo.Enabled)
            {
#if POLARITH_AI_PRO
                    if (aimContext.Sensor is AIMSpatialSensor)
                    {
                        outerRadiusGizmo.Draw(
                            gameObject.transform.position,
                            Formation.OuterCatchUpRadius);
                    }
                    else
#endif
                {
                    outerCircleGizmo.Draw(
                        gameObject.transform.position,
                        transform.rotation * aimContext.Sensor.Sensor.Rotation,
                        Formation.OuterCatchUpRadius);
                }
            }
            if (innerRadiusGizmo.Enabled)
            {
#if POLARITH_AI_PRO
                    if (aimContext.Sensor is AIMSpatialSensor)
                    {
                        innerRadiusGizmo.Draw(
                            gameObject.transform.position,
                            Formation.InnerCatchUpRadius);
                    }
                    else
#endif
                {
                    innerCircleGizmo.Draw(
                        gameObject.transform.position,
                        transform.rotation * aimContext.Sensor.Sensor.Rotation,
                        Formation.InnerCatchUpRadius);
                }
            }
            if (arriveRadiusGizmo.Enabled)
            {
#if POLARITH_AI_PRO
                    if (aimContext.Sensor is AIMSpatialSensor)
                    {
                        arriveRadiusGizmo.Draw(
                            gameObject.transform.position,
                            Formation.ArriveRadius);
                    }
                    else
#endif
                {
                    arriveCircleGizmo.Draw(
                        gameObject.transform.position,
                        transform.rotation * aimContext.Sensor.Sensor.Rotation,
                        Formation.ArriveRadius);
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void Start()
        {
            if (targetObject == null && targetTag != "Untagged")
            {
                GameObject tmp = GameObject.FindGameObjectWithTag(targetTag);
                if (tmp != null)
                    targetObject = tmp;
            }
        }

        #endregion // Methods
    } // class AIMFormation
} // namespace Polarith.AI.Move
