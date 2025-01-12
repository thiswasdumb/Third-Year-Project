using UnityEngine;
using System.IO;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f; // Player movement speed
    public float mouseSensitivity = 100f; // Sensitivity for mouse rotation
    public Transform cameraTransform; // Camera reference (drag in the Inspector)

    private Rigidbody rb;
    private float rotationY = 0f;

    // Ground check parameters
    public float groundCheckDistance = 1.1f; // Distance for ground detection
    public LayerMask groundLayer; // Assign your terrain layer here

    private bool isGrounded;

    // Audio setup
    private AudioSource audioSource;
    private string musicFolderPath = "cs310_music"; // Folder containing .mp3 files
    private string selectedSongPath;
    private bool isPlayerMoving;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the game window
        rb.freezeRotation = true; // Prevent Rigidbody from rotating due to physics

        // Audio setup
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 1.0f; // Set to maximum volume
        audioSource.spatialBlend = 0f; // Ensure it's 2D sound
        Debug.Log("Audio source initialized.");

        SelectRandomSong();

        // Ensure player spawns on the terrain
        PositionOnTerrain();
    }

    void Update()
    {
        // Mouse rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        rotationY -= mouseY;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(rotationY, 0f, 0f);
    }

    void FixedUpdate()
    {
        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            // Get movement input
            float moveHorizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right
            float moveVertical = Input.GetAxis("Vertical"); // W/S or Up/Down

            // Calculate movement direction
            Vector3 movementInput = (transform.forward * moveVertical + transform.right * moveHorizontal).normalized;

            // Check if player is moving
            isPlayerMoving = movementInput.magnitude > 0;

            // Apply movement using Rigidbody velocity
            rb.velocity = new Vector3(movementInput.x * speed, rb.velocity.y, movementInput.z * speed);

            // Handle audio playback
            HandleAudioPlayback(isPlayerMoving);
        }
        else
        {
            // Simulate gravity if not grounded
            rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration);

            // Pause audio if not grounded
            HandleAudioPlayback(false);
        }
    }

    private void PositionOnTerrain()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            float terrainHeight = terrain.SampleHeight(transform.position);
            transform.position = new Vector3(transform.position.x, terrainHeight + 1f, transform.position.z);
            Debug.Log($"Player positioned at terrain height: {terrainHeight}");
        }
        else
        {
            Debug.LogError("No active terrain found.");
        }
    }

private void SelectRandomSong()
{
    Debug.Log("Loading songs from Resources folder...");
    AudioClip[] audioClips = Resources.LoadAll<AudioClip>("cs310_music");

    if (audioClips.Length > 0)
    {
        audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
        Debug.Log($"Selected song: {audioSource.clip.name}");

        // Test playback immediately
        audioSource.Play();
        Debug.Log("Test playback started.");
    }
    else
    {
        Debug.LogError("No audio files found in Resources/cs310_music.");
    }
}




    private IEnumerator LoadAudioClip(string path)
{
    string url = "file://" + path;
    Debug.Log($"Requesting audio file from: {url}");

    using (var www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, UnityEngine.AudioType.MPEG))
    {
        yield return www.SendWebRequest();

        if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            audioSource.clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
            Debug.Log($"Audio clip loaded successfully: {audioSource.clip.name}");
        }
        else
        {
            Debug.LogError($"Failed to load audio file. Error: {www.error}");
        }
    }
}


    private void HandleAudioPlayback(bool isMoving)
    {
        if (audioSource.clip == null)
        {
            Debug.LogWarning("Audio clip is null; cannot play.");
            return;
        }

        if (isMoving)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.UnPause();
                Debug.Log("Audio playback started.");
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                Debug.Log("Audio playback paused.");
            }
        }
    }
}
