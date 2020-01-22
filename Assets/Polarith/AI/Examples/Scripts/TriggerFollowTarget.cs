using UnityEngine;
using Polarith.AI.Move;

namespace Polarith.AI.Package
{
    /// <summary>
    /// This script changes the target of a <see cref="AIMFollow"/> that enters the trigger.
    /// Note, this is just a script for our example scenes and therefore not part of the actual API. We do not guarantee
    /// that this script is working besides our examples.
    /// </summary>
    public class TriggerFollowTarget : MonoBehaviour
    {
        #region Fields =================================================================================================

        /// <summary>
        /// New target game object that is assigned to <see cref="AIMFollow.Target"/>.
        /// </summary>
        [Tooltip("New target GameObject of the FormationSetup.")]
        public GameObject NewTargetObject;

        /// <summary>
        /// Toggle if the agents of the formation that enters the trigger should be re-assigned.
        /// </summary>
        [Tooltip("Toggle if the agents of the formation that enters the trigger should be re-assigned.")]
        public bool ReAssign = true;

        #endregion // Fields

        #region Methods ================================================================================================

        private void OnTriggerEnter2D(Collider2D other)
        {
            AIMFollow f = other.transform.gameObject.GetComponent<AIMFollow>();
            if (f != null)
                f.Target = NewTargetObject;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            AIMFormationConfiguration c = collision.transform.gameObject.GetComponent<AIMFormationConfiguration>();
            if (c != null)
                c.UpdateConfig();                
        }

        #endregion // Methods
    } // class TriggerFollowTarget
} // namespace Polarith.AI.Package