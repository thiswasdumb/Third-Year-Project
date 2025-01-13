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
    public float groundCheckOffset = 0.5f; // Forward offset for ground check
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
        // Perform advanced ground check
        isGrounded = AdvancedGroundCheck();

        if (isGrounded)
        {
            // Get movement input
            float moveHorizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right
            float moveVertical = Input.GetAxis("Vertical"); // W/S or Up/Down
            Vector3 movementInput = (transform.forward * moveVertical + transform.right * moveHorizontal).normalized;

            if (movementInput.magnitude > 0)
            {
                rb.MovePosition(rb.position + movementInput * speed * Time.fixedDeltaTime);
                HandleAudioPlayback(true); // Start music when moving
            }
            else
            {
                HandleAudioPlayback(false); // Stop music when not moving
            }
        }
        else
        {
            // Apply sticky gravity to keep the player grounded
            rb.AddForce(Vector3.down * 50f, ForceMode.Force);
            Debug.LogWarning("Player is airborne.");
            HandleAudioPlayback(false); // Stop music when airborne
        }
    }

    private bool AdvancedGroundCheck()
    {
        // Perform multiple raycasts to improve ground detection
        Vector3 origin = transform.position;

        // Raycast directly downward
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitCenter, groundCheckDistance, groundLayer))
        {
            Debug.Log($"Grounded (center): {hitCenter.point}");
            return true;
        }

        // Raycast slightly forward for steep slopes
        Vector3 forwardOrigin = origin + transform.forward * groundCheckOffset;
        if (Physics.Raycast(forwardOrigin, Vector3.down, out RaycastHit hitForward, groundCheckDistance, groundLayer))
        {
            Debug.Log($"Grounded (forward): {hitForward.point}");
            return true;
        }

        // Raycast slightly backward for steep descents
        Vector3 backwardOrigin = origin - transform.forward * groundCheckOffset;
        if (Physics.Raycast(backwardOrigin, Vector3.down, out RaycastHit hitBackward, groundCheckDistance, groundLayer))
        {
            Debug.Log($"Grounded (backward): {hitBackward.point}");
            return true;
        }

        // If all raycasts fail, return false
        Debug.LogWarning("Not grounded!");
        return false;
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
            audioSource.Play();
            Debug.Log("Test playback started.");
        }
        else
        {
            Debug.LogError("No audio files found in Resources/cs310_music.");
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

        Debug.Log($"AudioSource status: IsPlaying = {audioSource.isPlaying}, Clip = {audioSource.clip.name}");
    }
}
