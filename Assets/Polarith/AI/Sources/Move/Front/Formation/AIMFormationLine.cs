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
    /// <see cref="AIMFormationLine"/> computes an agent's position based on its order inside a line-shaped 
    /// formation and the reference position or -object of the formation.
    /// Front-end component of the underlying <see cref="Move.FormationLine"/> class.
    /// </summary>
    [AddComponentMenu("Polarith AI » Move/Behaviours/Steering/AIM Formation Line")]
    [HelpURL("http://docs.polarith.com/ai/component-aim-formationline.html")]
    public sealed class AIMFormationLine : AIMFormation
    {
        #region Fields =================================================================================================

        [Tooltip("Reference to the underlying back-end class (read only).")]
        [SerializeField]
        private FormationLine formationLine = new FormationLine();

        [Tooltip("Visualizes the shape of the formation by its boundary.")]
        [SerializeField]
        private BoxGizmo formationGizmo = new BoxGizmo();

        #endregion // Fields

        #region Properties =============================================================================================

        /// <summary>
        /// Polymorphic reference to the underlying back-end class <see cref="Move.SteeringBehaviour"/> (read only).
        /// </summary>
        public override SteeringBehaviour SteeringBehaviour
        {
            get { return formationLine; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Polymorphic reference to the underlying back-end class <see cref="Move.Formation"/> (read only).
        /// </summary>
        public override Formation Formation
        {
            get { return formationLine; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reference to the underlying back-end class <see cref="Move.FormationLine"/> (read only).
        /// </summary>
        public FormationLine FormationLine
        {
            get { return formationLine; }
            set { formationLine = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Determines whether the underlying back-end class is thread-safe (read only). Returns always <c>true</c>.
        /// </summary>
        public override bool ThreadSafe
        {
            get { return true; }
        }

        #endregion // Properties

        #region Methods ================================================================================================

        /// <summary>
        /// Visualizes the boundary of the formation based on its size.
        /// </summary>
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (!formationGizmo.Enabled || !isActiveAndEnabled)
                return;

            if (TargetObject != null)
            {
                formationGizmo.Draw(TargetObject.transform.position,
                    formationLine.Size / 2f * formationLine.Spacing, formationLine.Spacing / 2f, 0,
                    TargetObject.transform.rotation);
            }
            else
            {
                formationGizmo.Draw(TargetPosition,
                    formationLine.Size / 2f * formationLine.Spacing, formationLine.Spacing / 2f, 0,
                    Quaternion.identity);
            }
        }

        #endregion // Methods
    } // class AIMFormationLine
} // namespace Polarith.AI.Move
