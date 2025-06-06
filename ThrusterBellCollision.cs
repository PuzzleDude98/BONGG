using UnityEngine;

namespace BONGG
{
    /// <summary>
    /// Collision detection component for thruster bell sound effects
    /// </summary>
    public class ThrusterBellCollision : MonoBehaviour
    {
        private ThrusterBellMod parentMod;
        private float lastCollisionTime = 0f;
        private const float COLLISION_COOLDOWN = 0.5f; // Prevent spam

        /// <summary>
        /// Initialize the collision component with reference to the parent mod
        /// </summary>
        /// <param name="mod">The parent ThrusterBellMod instance</param>
        public void Initialize(ThrusterBellMod mod)
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
            if (other.GetComponent<PlayerCharacterController>() != null)
            {
                // Check cooldown to prevent sound spam
                if (Time.time - lastCollisionTime > COLLISION_COOLDOWN)
                {
                    parentMod?.PlayBellSound();
                    lastCollisionTime = Time.time;
                }
            }
        }

        /// <summary>
        /// Optional: Add continuous collision detection for prolonged contact
        /// </summary>
        /// <param name="other">The collider staying in the trigger</param>
        private void OnTriggerStay(Collider other)
        {
            // Could add logic here for continuous contact effects
            // Currently unused to prevent sound spam
        }
    }
}