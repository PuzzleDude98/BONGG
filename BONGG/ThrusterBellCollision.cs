using UnityEngine;
using OWML.Common;

namespace BONGG
{
    /// <summary>
    /// Collision detection component for thruster bell sound effects
    /// </summary>
    public class ThrusterBellCollision : MonoBehaviour
    {
        private BONGG parentMod;
        private float lastCollisionTime = 0f;
        private const float COLLISION_COOLDOWN = 0.5f; // Prevent spam

        /// <summary>
        /// Initialize the collision component with reference to the parent mod
        /// </summary>
        /// <param name="mod">The parent ThrusterBellMod instance</param>
        public void Initialize(BONGG mod)
        {
            parentMod = mod;
        }

        /// <summary>
        /// Called when another collider enters the trigger
        /// </summary>
        /// <param name="other">The collider that entered the trigger</param>
        private void OnTriggerEnter(Collider other)
        {

            // Check if the colliding object is the player
            var playerController = other.GetComponent<PlayerCharacterController>();
            if (playerController != null)
            {

                // Check cooldown to prevent sound spam
                if (Time.time - lastCollisionTime > COLLISION_COOLDOWN)
                {

                    if (parentMod != null)
                    {
                        parentMod.PlayBellSound(this.GetComponentInParent<AudioSource>());
                        lastCollisionTime = Time.time;
                    }
                    else
                    {
                        Debug.LogError("[BONGG] parentMod is null! Cannot play sound.");
                    }
                }
            }
            else
            {
                // Debug: What components does this object have?
                var components = other.GetComponents<Component>();
                string componentList = "Components: ";
                foreach (var comp in components)
                {
                    componentList += comp.GetType().Name + ", ";
                }
            }
        }
    }
}