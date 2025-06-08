using BONGG;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace BONGG
{
    public class BONGG : ModBehaviour
    {
        private AudioSource bellAudioSource;
        private AudioClip bellClip;
        private PlayerCharacterController playerController;
        private ShipThrusterController thrusterController;

        private void Start()
        {
            ModHelper.Console.WriteLine($"Thruster Bell Mod is loaded!", MessageType.Success);

            // Load the bell sound effect
            LoadBellSound();

            // Find player and ship components when the scene loads
            LoadManager.OnCompleteSceneLoad += OnSceneLoaded;
        }

        private void LoadBellSound()
        {
            // Create an AudioSource on this mod's GameObject
            bellAudioSource = gameObject.AddComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);

            // Try to load custom bell sound first
            string customBellPath = ModHelper.Manifest.ModFolderPath + "assets/bongg.mp3";

            if (System.IO.File.Exists(customBellPath))
            {

                // Load the custom audio file
                StartCoroutine(LoadAudioClip(customBellPath));
            }
            else
            {
                Debug.LogWarning($"[BONGG] Custom bell sound not found at: {customBellPath}");
            }
        }

        private System.Collections.IEnumerator LoadAudioClip(string path)
        {
            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip("file://" + path, UnityEngine.AudioType.UNKNOWN))
            {
                yield return www.SendWebRequest();

                if (!www.isNetworkError && !www.isHttpError)
                {
                    bellClip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);

                    if (bellClip != null)
                    {
                        ConfigureAudioSource();
                    }
                    else
                    {
                        Debug.LogError("[BONGG] Failed to extract AudioClip from loaded file");
                    }
                }
                else
                {
                    Debug.LogError($"[BONGG] Failed to load custom audio: {www.error}");
                }
            }
        }

        private void ConfigureAudioSource()
        {
            bellAudioSource.clip = bellClip;
            bellAudioSource.volume = 0.5f;
            bellAudioSource.pitch = 1.2f;
            bellAudioSource.spatialBlend = 0f; // 2D sound
        }

        private void OnSceneLoaded(OWScene scene, OWScene loadScene)
        {
            if (loadScene != OWScene.SolarSystem) return;

            // Find the player controller
            playerController = FindObjectOfType<PlayerCharacterController>();

            // Find the ship thruster controller
            var ship = GameObject.Find("Ship_Body");
            if (ship != null)
            {
                thrusterController = ship.GetComponentInChildren<ShipThrusterController>();
            }

            if (playerController != null && thrusterController != null)
            {
                SetupCollisionDetection();
            }
            else
            {
                ModHelper.Console.WriteLine("Could not find required components!", MessageType.Error);
            }
        }

        private void SetupCollisionDetection()
        {
            // Get all thruster objects
            var thrusters = thrusterController.GetComponentsInChildren<ThrusterFlameController>();

            foreach (var thruster in thrusters)
            {
                // Use the kinematic-safe collision helper
                GameObject collisionDetector = KinematicCollisionHelper.CreateAdaptiveCollisionDetector(
                    thruster.gameObject,
                    thruster);

                // Add our custom collision component to the detector
                var bellCollision = collisionDetector.AddComponent<ThrusterBellCollision>();
                bellCollision.Initialize(this);

                // Add debug visualizer if debug mode is enabled
                if (ModHelper.Config.GetSettingsValue<bool>("showDebugColliders"))
                {
                    var debugViz = collisionDetector.AddComponent<ColliderDebugVisualizer>();
                    debugViz.showColliders = true;
                    debugViz.colliderColor = Color.green;
                    debugViz.activeColliderColor = Color.red;
                }
            }
        }

        public void PlayBellSound()
        {

            if (bellAudioSource == null)
            {
                Debug.LogError("[BONGG] bellAudioSource is null!");
                return;
            }

            if (bellClip == null)
            {
                Debug.LogError("[BONGG] bellClip is null!");
                return;
            }

            // Add some randomization to prevent repetitive sounds
            bellAudioSource.pitch = Random.Range(0.95f, 1.00f);
            bellAudioSource.PlayOneShot(bellClip);
        }

        // Configuration methods
        public override void Configure(IModConfig config)
        {

            var volume = config.GetSettingsValue<float>("volume");
            if (bellAudioSource != null)
            {
                bellAudioSource.volume = volume;
            }
        }
    }
}