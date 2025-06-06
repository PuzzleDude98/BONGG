using UnityEngine;
using OWML.Common;
using BONGG;

namespace BONGG
{
    /// <summary>
    /// Debug component that visualizes colliders in-game
    /// </summary>
    public class ColliderDebugVisualizer : MonoBehaviour
    {
        [Header("Debug Settings")]
        public bool showColliders = true;
        public Color colliderColor = Color.green;
        public Color activeColliderColor = Color.red;
        public float wireframeAlpha = 0.3f;
        public bool showLabels = true;

        private Collider targetCollider;
        private ThrusterBellCollision bellCollision;
        private bool isPlayerInside = false;
        private GameObject wireframeObject;
        private LineRenderer wireframeRenderer;

        private void Start()
        {
            targetCollider = GetComponent<Collider>();
            bellCollision = GetComponent<ThrusterBellCollision>();

            if (targetCollider != null && showColliders)
            {
                CreateWireframeVisualization();
            }
        }

        private void CreateWireframeVisualization()
        {
            // Create a child object for the wireframe
            wireframeObject = new GameObject("ColliderWireframe");
            wireframeObject.transform.SetParent(transform);
            wireframeObject.transform.localPosition = Vector3.zero;
            wireframeObject.transform.localRotation = Quaternion.identity;
            wireframeObject.transform.localScale = Vector3.one;

            // Add LineRenderer for drawing the wireframe
            wireframeRenderer = wireframeObject.AddComponent<LineRenderer>();
            wireframeRenderer.material = CreateWireframeMaterial();
            wireframeRenderer.startWidth = 0.05f;
            wireframeRenderer.endWidth = 0.05f;
            wireframeRenderer.useWorldSpace = false;
            wireframeRenderer.loop = true;

            UpdateWireframeGeometry();
        }

        private Material CreateWireframeMaterial()
        {
            // Create a simple unlit material for the wireframe
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(colliderColor.r, colliderColor.g, colliderColor.b, wireframeAlpha);
            return mat;
        }

        private void UpdateWireframeGeometry()
        {
            if (wireframeRenderer == null || targetCollider == null) return;

            Vector3[] points = null;

            if (targetCollider is SphereCollider sphere)
            {
                points = GenerateSphereWireframe(sphere);
            }
            else if (targetCollider is CapsuleCollider capsule)
            {
                points = GenerateCapsuleWireframe(capsule);
            }
            else if (targetCollider is BoxCollider box)
            {
                points = GenerateBoxWireframe(box);
            }

            if (points != null)
            {
                wireframeRenderer.positionCount = points.Length;
                wireframeRenderer.SetPositions(points);
            }
        }

        private Vector3[] GenerateSphereWireframe(SphereCollider sphere)
        {
            int segments = 32;
            Vector3[] points = new Vector3[segments * 3]; // 3 circles: XY, XZ, YZ
            Vector3 center = sphere.center;
            float radius = sphere.radius;

            // XY circle
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2;
                points[i] = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0
                );
            }

            // XZ circle  
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2;
                points[segments + i] = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0,
                    Mathf.Sin(angle) * radius
                );
            }

            // YZ circle
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2;
                points[segments * 2 + i] = center + new Vector3(
                    0,
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius
                );
            }

            return points;
        }

        private Vector3[] GenerateCapsuleWireframe(CapsuleCollider capsule)
        {
            int segments = 16;
            Vector3[] points = new Vector3[segments * 4]; // Top circle, bottom circle, and connecting lines
            Vector3 center = capsule.center;
            float radius = capsule.radius;
            float height = capsule.height;

            Vector3 direction = Vector3.up;
            if (capsule.direction == 0) direction = Vector3.right;
            else if (capsule.direction == 2) direction = Vector3.forward;

            Vector3 topCenter = center + direction * (height * 0.5f - radius);
            Vector3 bottomCenter = center - direction * (height * 0.5f - radius);

            // Top circle
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2;
                Vector3 offset = Vector3.Cross(direction, Vector3.up).normalized * Mathf.Cos(angle) * radius +
                               Vector3.Cross(Vector3.Cross(direction, Vector3.up), direction).normalized * Mathf.Sin(angle) * radius;
                points[i] = topCenter + offset;
            }

            // Bottom circle
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2;
                Vector3 offset = Vector3.Cross(direction, Vector3.up).normalized * Mathf.Cos(angle) * radius +
                               Vector3.Cross(Vector3.Cross(direction, Vector3.up), direction).normalized * Mathf.Sin(angle) * radius;
                points[segments + i] = bottomCenter + offset;
            }

            return points;
        }

        private Vector3[] GenerateBoxWireframe(BoxCollider box)
        {
            Vector3[] points = new Vector3[8];
            Vector3 center = box.center;
            Vector3 size = box.size * 0.5f;

            // Generate box corners
            points[0] = center + new Vector3(-size.x, -size.y, -size.z);
            points[1] = center + new Vector3(size.x, -size.y, -size.z);
            points[2] = center + new Vector3(size.x, size.y, -size.z);
            points[3] = center + new Vector3(-size.x, size.y, -size.z);
            points[4] = center + new Vector3(-size.x, -size.y, size.z);
            points[5] = center + new Vector3(size.x, -size.y, size.z);
            points[6] = center + new Vector3(size.x, size.y, size.z);
            points[7] = center + new Vector3(-size.x, size.y, size.z);

            return points;
        }

        private void Update()
        {
            if (!showColliders && wireframeObject != null)
            {
                wireframeObject.SetActive(false);
                return;
            }

            if (wireframeObject != null)
            {
                wireframeObject.SetActive(true);

                // Change color when player is inside
                Color currentColor = isPlayerInside ? activeColliderColor : colliderColor;
                currentColor.a = wireframeAlpha;
                wireframeRenderer.material.color = currentColor;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerCharacterController>() != null)
            {
                isPlayerInside = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PlayerCharacterController>() != null)
            {
                isPlayerInside = false;
            }
        }

        private void OnGUI()
        {
            if (!showLabels || !showColliders) return;

            // Show collider info on screen
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

            if (screenPos.z > 0 && Vector3.Distance(transform.position, Camera.main.transform.position) < 50f)
            {
                screenPos.y = Screen.height - screenPos.y; // Flip Y coordinate

                string colliderInfo = $"{gameObject.name}\n";
                if (targetCollider is SphereCollider sphere)
                {
                    colliderInfo += $"Sphere R:{sphere.radius:F1}";
                }
                else if (targetCollider is CapsuleCollider capsule)
                {
                    colliderInfo += $"Capsule H:{capsule.height:F1} R:{capsule.radius:F1}";
                }
                else if (targetCollider is BoxCollider box)
                {
                    colliderInfo += $"Box {box.size.x:F1}x{box.size.y:F1}x{box.size.z:F1}";
                }

                if (isPlayerInside)
                {
                    colliderInfo += "\n[PLAYER INSIDE]";
                }

                GUI.color = isPlayerInside ? Color.red : Color.white;
                GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 30, 100, 60), colliderInfo);
            }
        }

        // Public methods for controlling debug display
        public void SetDebugEnabled(bool enabled)
        {
            showColliders = enabled;
        }

        public void SetColliderColor(Color color)
        {
            colliderColor = color;
        }

        public void SetActiveColor(Color color)
        {
            activeColliderColor = color;
        }

        private void OnDestroy()
        {
            if (wireframeObject != null)
            {
                Destroy(wireframeObject);
            }
        }
    }
}