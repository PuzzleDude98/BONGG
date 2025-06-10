using BONGG;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace BONGG
{
    public class BONGG : ModBehaviour
    {

        private AudioSource src;
        private AudioClip bellClip;
        private PlayerCharacterController playerController;
        private ShipThrusterController thrusterController;


        private void Start()
        {
            ModHelper.Console.WriteLine($"Thruster Bell Mod is loaded!", MessageType.Success);

            // Load the bell sound effect
            //LoadBellSound();

            // Find player and ship components when the scene loads
            LoadManager.OnCompleteSceneLoad += OnSceneLoaded;
        }

        private void LoadBellSound()
        {
            // Create an AudioSource on this mod's GameObject
            src = gameObject.AddComponent<AudioSource>();
            src.clip = ModHelper.Assets.GetAudio("Assets/boom.wav");
            
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


            GameObject gameObject = FindObjectOfType<PlayerBody>().gameObject;
            src = gameObject.AddComponent<AudioSource>();
            bellClip = this.ModHelper.Assets.GetAudio("Assets/bongg.mp3");
            src.volume = 0.5f;
            src.pitch = 1.2f;
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
                //if (ModHelper.Config.GetSettingsValue<bool>("showDebugColliders"))
                //{
                //    var debugViz = collisionDetector.AddComponent<ColliderDebugVisualizer>();
                //    debugViz.showColliders = true;
                //    debugViz.colliderColor = Color.green;
                //    debugViz.activeColliderColor = Color.red;
                //}
            }
        }

        public void PlayBellSound()
        {

            Debug.Log("BONGG");

            if (src == null)
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
            src.pitch = Random.Range(0.95f, 1.00f);
            src.PlayOneShot(bellClip);
        }

        // Configuration methods
        public override void Configure(IModConfig config)
        {

            var volume = config.GetSettingsValue<float>("volume");
            if (src != null)
            {
                src.volume = volume;
            }
        }
    }
}