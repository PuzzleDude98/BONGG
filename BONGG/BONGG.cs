using BONGG;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace BONGG
{
    public class BONGG : ModBehaviour
    {
        private AudioClip bellClip;
        private PlayerCharacterController playerController;
        private ShipThrusterController thrusterController;
        private float volume;


        private void Start()
        {
            ModHelper.Console.WriteLine($"Thruster Bell Mod is loaded!", MessageType.Success);

            // Find player and ship components when the scene loads
            LoadManager.OnCompleteSceneLoad += OnSceneLoaded;
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

            bellClip = this.ModHelper.Assets.GetAudio("Assets/bongg.mp3");
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
                AudioSource thisSrc = collisionDetector.gameObject.AddComponent<AudioSource>();
                thisSrc.volume = 0.5f;
                thisSrc.spatialBlend = 1.0f;
            }
        }

        public void PlayBellSound(AudioSource audio)
        {

            Debug.Log("BONGG");

            if (audio == null)
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
            audio.pitch = Random.Range(0.95f, 1.00f);
            audio.volume = volume;
            audio.PlayOneShot(bellClip);
        }

        // Configuration methods
        public override void Configure(IModConfig config)
        {

            volume = config.GetSettingsValue<float>("volume");
        }
    }
}