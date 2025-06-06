using UnityEngine;
using OWML.Common;
using System.Linq;

namespace BONGG
{
    /// <summary>
    /// Helper class for setting up collision detection that works properly with kinematic rigidbodies
    /// </summary>
    public static class KinematicCollisionHelper
    {
        /// <summary>
        /// Creates a separate collision detector GameObject to avoid kinematic rigidbody conflicts
        /// </summary>
        /// <param name="parentObject">The thruster object to attach the detector to</param>
        /// <param name="colliderRadius">Radius of the collision sphere</param>
        /// <param name="offset">Local position offset for the detector</param>
        /// <returns>The created collision detector GameObject</returns>
        public static GameObject CreateCollisionDetector(GameObject parentObject, float colliderRadius = 2f, Vector3 offset = default)
        {
            // Create a separate collision detector GameObject
            GameObject detector = new GameObject($"{parentObject.name}_CollisionDetector");
            detector.transform.SetParent(parentObject.transform);
            detector.transform.localPosition = offset;
            detector.transform.localRotation = Quaternion.identity;
            detector.transform.localScale = Vector3.one;

            // Add and configure the trigger collider
            var sphereCollider = detector.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = colliderRadius;

            // Add rigidbody with proper kinematic settings
            var rigidbody = detector.AddComponent<Rigidbody>();
            ConfigureKinematicRigidbody(rigidbody);

            return detector;
        }

        /// <summary>
        /// Creates a capsule collision detector for more realistic thruster flame shapes
        /// </summary>
        /// <param name="parentObject">The thruster object to attach the detector to</param>
        /// <param name="height">Height of the capsule</param>
        /// <param name="radius">Radius of the capsule</param>
        /// <param name="offset">Local position offset for the detector</param>
        /// <returns>The created collision detector GameObject</returns>
        public static GameObject CreateCapsuleCollisionDetector(GameObject parentObject, float height = 3f, float radius = 1f, Vector3 offset = default)
        {
            GameObject detector = new GameObject($"{parentObject.name}_CapsuleDetector");
            detector.transform.SetParent(parentObject.transform);
            detector.transform.localPosition = offset;
            detector.transform.localRotation = Quaternion.identity;
            detector.transform.localScale = Vector3.one;

            // Add and configure the capsule trigger collider
            var capsuleCollider = detector.AddComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = true;
            capsuleCollider.height = height;
            capsuleCollider.radius = radius;
            capsuleCollider.direction = 2; // Z-axis (forward)
            capsuleCollider.center = Vector3.forward * (height * 0.25f); // Offset forward slightly

            // Add rigidbody with proper kinematic settings
            var rigidbody = detector.AddComponent<Rigidbody>();
            ConfigureKinematicRigidbody(rigidbody);

            return detector;
        }

        /// <summary>
        /// Configures a rigidbody for optimal kinematic collision detection
        /// </summary>
        /// <param name="rigidbody">The rigidbody to configure</param>
        public static void ConfigureKinematicRigidbody(Rigidbody rigidbody)
        {
            // Set kinematic properties
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            // Use ContinuousSpeculative for kinematic bodies (avoids the warning)
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            // Freeze all constraints since we don't want physics movement
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            // Set mass to minimal value
            rigidbody.mass = 0.001f;
        }

        /// <summary>
        /// Safely adds collision detection to existing objects, handling existing rigidbodies
        /// </summary>
        /// <param name="targetObject">Object to add collision to</param>
        /// <param name="colliderRadius">Radius of the collision sphere</param>
        /// <returns>True if successful, false if conflicts detected</returns>
        public static bool SafeAddCollisionToExistingObject(GameObject targetObject, float colliderRadius = 2f)
        {
            // Check if object already has a rigidbody that might conflict
            var existingRb = targetObject.GetComponent<Rigidbody>();
            if (existingRb != null && !existingRb.isKinematic)
            {
                Debug.LogWarning($"Object {targetObject.name} has non-kinematic rigidbody. Creating separate detector instead.");
                return false;
            }

            // Add collider if it doesn't exist
            var collider = targetObject.GetComponent<Collider>();
            if (collider == null)
            {
                var sphereCollider = targetObject.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
                sphereCollider.radius = colliderRadius;
            }
            else
            {
                // Make existing collider a trigger
                collider.isTrigger = true;
            }

            // Add or configure rigidbody
            if (existingRb == null)
            {
                existingRb = targetObject.AddComponent<Rigidbody>();
            }

            ConfigureKinematicRigidbody(existingRb);

            return true;
        }

        /// <summary>
        /// Creates multiple collision detectors for complex thruster shapes
        /// </summary>
        /// <param name="parentObject">The thruster object</param>
        /// <returns>Array of created detector GameObjects</returns>
        public static GameObject[] CreateMultipleCollisionDetectors(GameObject parentObject)
        {
            var detectors = new GameObject[2];

            // Main flame body
            detectors[0] = CreateCapsuleCollisionDetector(
                parentObject,
                height: 2.5f,
                radius: 0.8f,
                offset: Vector3.forward * 0.5f
            );
            detectors[0].name = $"{parentObject.name}_MainFlameDetector";

            // Flame tip
            detectors[1] = CreateCollisionDetector(
                parentObject,
                colliderRadius: 0.6f,
                offset: Vector3.forward * 2.0f
            );
            detectors[1].name = $"{parentObject.name}_FlameTipDetector";

            return detectors;
        }

        /// <summary>
        /// Adaptive collision setup that analyzes the thruster and creates appropriate detectors
        /// </summary>
        /// <param name="thrusterObject">The thruster GameObject</param>
        /// <param name="thrusterController">The thruster controller component</param>
        /// <returns>The created collision detector</returns>
        public static GameObject CreateAdaptiveCollisionDetector(GameObject thrusterObject, ThrusterFlameController thrusterController)
        {
            // Analyze thruster size and type
            var bounds = GetThrusterBounds(thrusterObject);
            var scale = thrusterObject.transform.lossyScale;
            var averageScale = (scale.x + scale.y + scale.z) / 3f;

            // Determine if this is a main thruster or RCS thruster
            bool isMainThruster = thrusterObject.name.ToLower().Contains("main") ||
                                 thrusterObject.name.ToLower().Contains("primary") ||
                                 bounds.size.magnitude > 2f;

            GameObject detector;

            if (isMainThruster)
            {
                // Use capsule for main thrusters (more realistic flame shape)
                detector = CreateCapsuleCollisionDetector(
                    thrusterObject,
                    height: 1.3f * averageScale,
                    radius: 0.7f * averageScale,
                    offset: Vector3.forward * -0.8f * averageScale
                );
            }
            else
            {
                // Use sphere for RCS thrusters (simpler, smaller)
                detector = CreateCollisionDetector(
                    thrusterObject,
                    colliderRadius: 1.5f * averageScale,
                    offset: Vector3.forward * 0.3f * averageScale
                );
            }

            return detector;
        }

        /// <summary>
        /// Gets the bounds of a thruster by analyzing its renderers
        /// </summary>
        /// <param name="thrusterObject">The thruster GameObject</param>
        /// <returns>Combined bounds of all renderers</returns>
        private static Bounds GetThrusterBounds(GameObject thrusterObject)
        {
            var renderers = thrusterObject.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                // Default bounds if no renderers found
                return new Bounds(thrusterObject.transform.position, Vector3.one);
            }

            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            return combinedBounds;
        }

        /// <summary>
        /// Cleans up collision detectors when needed
        /// </summary>
        /// <param name="parentObject">The parent thruster object</param>
        public static void CleanupCollisionDetectors(GameObject parentObject)
        {
            // Find and destroy all collision detector children
            var detectors = parentObject.GetComponentsInChildren<Transform>()
                .Where(t => t.name.Contains("CollisionDetector") || t.name.Contains("Detector"))
                .ToArray();

            foreach (var detector in detectors)
            {
                if (detector != parentObject.transform) // Don't destroy the parent
                {
                    Object.Destroy(detector.gameObject);
                }
            }
        }
    }
}