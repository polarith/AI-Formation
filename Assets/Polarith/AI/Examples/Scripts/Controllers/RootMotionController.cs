using Polarith.AI.Move;
using UnityEngine;

namespace Polarith.AI.Package
{
    /// <summary>
    /// This class illustrates how to combine Polarith.AI with Unitys state - and root motion mechanism. It acts as an
    /// example and may be altered, improved or maybe used as base for more elaborate implemenations for specific
    /// animation controllers. By combining Polarith AI and root motion mechanis different animation states like idle,
    /// walk and run are easily manageble with just a few lines.
    /// <para/>
    /// This implementation does two things. First, the character is aligned along the direction received by <see
    /// cref="AIMContext.DecidedDirection"/>. Second, a simple float parameter is passed to the <see cref="Animator"/>
    /// to provide a hint how fast the character should move. This way, the different animation states, like idle or
    /// walking are managed by the animation controller like it should be. Further information can be found here,
    /// https://docs.unity3d.com/Manual/AnimationStateMachines.html. The actual movement of the character is done by the
    /// root motion mechanis (https://docs.unity3d.com/Manual/RootMotion.html).
    /// <para/>
    /// A prerequisite is of course a fitting animation controller setup, like the SimpleWalkCycle.controller in the
    /// Polarith AI package.
    /// <para/>
    /// Note, this is just a script for our example scenes and therefore not part of the actual API. We do not guarantee
    /// that this script is working besides our examples.
    /// </summary>
    public class RootMotionController : MonoBehaviour
    {
        #region Fields =================================================================================================

        /// <summary>
        /// The animation controller that holds and manages the different animation states. This is a mandatory
        /// reference since the decision of Polarith AI ( <see cref="Context"/>) is applied to this animation
        /// controller. If the reference is <c>null</c>, the component searches for an attached Animator OnEnable.
        /// </summary>
        [Tooltip("The animation controller that holds and manages the different animation states. This is a " +
            "mandatory reference since the decision of Polarith AI (see 'Context') is applied to this animation " +
            "controller. If the reference is null, the component searches for an attached Animator OnEnable.")]
        public Animator Animator;

        /// <summary>
        /// This component provides the results of the AI system. These results are then applied to the attached <see
        /// cref="Animator"/>. Thus, the reference to an AIMContext component is mandatory. The controller is disabled
        /// if no Context instance can be found at OnEnable.
        /// </summary>
        [Tooltip("This component provides the results of the AI system. These results are then applied to the " +
            "attached Animator. Thus, the reference to an AIMContext component is mandatory.The controller is + " +
            "disabled if no Context instance can be found at OnEnable.")]
        public AIMContext Context;

        /// <summary>
        /// The maximum value of the parameter passed to the <see cref="AnimatorParameter"/> that is assumend to somehow
        /// control the movement animation. Thus, it could be seen as a limit for movement speed.
        /// </summary>
        public float MovementSpeed = 0.5f;

        /// <summary>
        /// Controls how fast the character can rotate to a direction given by the <see cref="Context"/>. In radians per
        /// second. For example, a value of 3.141 means that the character can turn around in one second.
        /// </summary>
        [Tooltip("Controls how fast the character can rotate to a direction given by the Context. In radians per " +
            "second. For example, a value of 3.141 means that the character can turn around in one second.")]
        public float RotationSpeed = 1.0f;

        /// <summary>
        /// If set equal to or greater than 0, the evaluated AI decision value is multiplied to the <see
        /// cref="MovementSpeed"/>.
        /// </summary>
        [Tooltip("If set equal to or greater than 0, the evaluated AI decision value is multiplied to the 'Speed'.")]
        [TargetObjective(true)]
        public int ObjectiveAsSpeed = -1;

        #endregion // Fields

        #region Methods ================================================================================================

        private void OnEnable()
        {
            if (Animator == null)
                Animator = GetComponent<Animator>();

            if (Context == null)
                Context = GetComponent<AIMContext>();

            if (Context == null || Animator == null)
            {
                Debug.LogWarning('(' + typeof(RootMotionController).Name + ") " + name +": deactivated because a " +
                    "reference to either an AIMContext or an Animator is missing.");
                enabled = false;
                return;
            }

            Animator.applyRootMotion = true;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void Update()
        {
            // Rotate the character
            Vector3 targetDirection = Context.DecidedDirection;
            float step = RotationSpeed * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, step, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);

            // Calculate rotation distance
            float speedMultiplier = 1.0f;
            if (Vector3.Angle(targetDirection, transform.forward) > 50.0f)
                speedMultiplier = 0.0f;

            // Move the character
            if (ObjectiveAsSpeed >= 0 && ObjectiveAsSpeed < Context.DecidedValues.Count)
            {
                float magnitude = Context.DecidedValues[ObjectiveAsSpeed] * MovementSpeed;
                magnitude = magnitude > MovementSpeed ? MovementSpeed : magnitude;
                Animator.SetFloat("Speed", magnitude * speedMultiplier);
            }
            else
            {
                Animator.SetFloat("Speed", MovementSpeed * speedMultiplier);
            }        
        }
        #endregion // Methods
    } // class RootMotionController
} // namespace Polarith.AI.Package
