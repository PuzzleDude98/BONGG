using BONGG;
using UnityEngine;

namespace BONGG
{
    public static class CollisionHelper
    {
        /// <summary>
        /// Method 1: Calculate collision size based on thruster flame renderer bounds
        /// </summary>
        public static void SetupCollisionFromRenderer(GameObject thrusterObject)
        {
            var renderer = thrusterObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                var bounds = renderer.bounds;
                var sphereCollider = thrusterObject.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;

                // Use the largest dimension as radius
                sphereCollider.radius = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 0.5f;

                // Offset the center to match the bounds center
                sphereCollider.center = thrusterObject.transform.InverseTransformPoint(bounds.center);
            }
        }

        /// <summary>
        /// Method 2: Use capsule collider for more realistic thruster flame shape
        /// </summary>
        public static void SetupCapsuleCollision(GameObject thrusterObject, float length = 3f, float radius = 1f)
        {
            var capsuleCollider = thrusterObject.AddComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = true;
            capsuleCollider.direction = 2; // Z-axis (forward direction)
            capsuleCollider.height = length;
            capsuleCollider.radius = radius;

            // Offset forward to represent flame extending from thruster
            capsuleCollider.center = Vector3.forward * (length * 0.5f);
        }

        /// <summary>
        /// Method 3: Adaptive sizing based on thruster scale and type
        /// </summary>
        public static void SetupAdaptiveCollision(GameObject thrusterObject, ThrusterFlameController thrusterController)
        {
            var sphereCollider = thrusterObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;

            // Base size on the transform scale
            var scale = thrusterObject.transform.lossyScale;
            var averageScale = (scale.x + scale.y + scale.z) / 3f;

            // Check if this is a main thruster vs. RCS thruster
            bool isMainThruster = thrusterObject.name.Contains("Main") ||
                                 thrusterObject.name.Contains("Primary");

            float baseRadius = isMainThruster ? 2.5f : 1.5f;
            sphereCollider.radius = baseRadius * averageScale;

            // Adjust based on thruster intensity if available
            if (thrusterController != null)
            {
                // Some thrusters might have different max thrust values
                // This is speculative - you'd need to check the actual Outer Wilds API
                sphereCollider.radius *= 1.2f; // Slight boost for active thrusters
            }
        }

        /// <summary>
        /// Method 4: Multiple colliders for complex thruster shapes
        /// </summary>
        public static void SetupMultipleColliders(GameObject thrusterObject)
        {
            // Main flame body (cylinder)
            var cylinderCollider = thrusterObject.AddComponent<CapsuleCollider>();
            cylinderCollider.isTrigger = true;
            cylinderCollider.direction = 2; // Z-axis
            cylinderCollider.height = 2.5f;
            cylinderCollider.radius = 0.4f;
            cylinderCollider.center = Vector3.forward * 1.00f;

            // Flame tip (smaller sphere)
            var tipObject = new GameObject("ThrusterTip");
            tipObject.transform.SetParent(thrusterObject.transform);
            tipObject.transform.localPosition = Vector3.forward * 2.5f;

            var tipCollider = tipObject.AddComponent<SphereCollider>();
            tipCollider.isTrigger = true;
            tipCollider.radius = 0.5f;

            // Add collision component to both
            var mainCollision = thrusterObject.AddComponent<ThrusterBellCollision>();
            var tipCollision = tipObject.AddComponent<ThrusterBellCollision>();
        }

        /// <summary>
        /// Method 5: Physics-based approach using existing colliders
        /// </summary>
        public static void SetupFromExistingColliders(GameObject thrusterObject)
        {
            var existingColliders = thrusterObject.GetComponents<Collider>();

            if (existingColliders.Length > 0)
            {
                // Use the first existing collider as reference
                var referenceCollider = existingColliders[0];

                var newCollider = thrusterObject.AddComponent<SphereCollider>();
                newCollider.isTrigger = true;

                if (referenceCollider is SphereCollider sphere)
                {
                    newCollider.radius = sphere.radius * 1.5f; // Slightly larger
                    newCollider.center = sphere.center;
                }
                else if (referenceCollider is BoxCollider box)
                {
                    var size = box.size;
                    newCollider.radius = Mathf.Max(size.x, size.y, size.z) * 0.6f;
                    newCollider.center = box.center;
                }
            }
        }

        /// <summary>
        /// Method 6: Visual flame-based sizing (most accurate)
        /// </summary>
        public static void SetupFromFlameVisuals(GameObject thrusterObject, ThrusterFlameController flameController)
        {
            // Try to find the actual flame visual elements
            var flameRenderers = thrusterObject.GetComponentsInChildren<Renderer>();

            if (flameRenderers.Length > 0)
            {
                // Calculate combined bounds of all flame visual elements
                Bounds combinedBounds = flameRenderers[0].bounds;

                for (int i = 1; i < flameRenderers.Length; i++)
                {
                    combinedBounds.Encapsulate(flameRenderers[i].bounds);
                }

                // Create collider based on visual bounds
                var collider = thrusterObject.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = combinedBounds.size.magnitude * 0.5f;
                collider.center = thrusterObject.transform.InverseTransformPoint(combinedBounds.center);

                Debug.Log($"Thruster collision setup: radius={collider.radius}, center={collider.center}");
            }
            else
            {
                // Fallback to default if no renderers found
                SetupAdaptiveCollision(thrusterObject, flameController);
            }
        }
    }
}