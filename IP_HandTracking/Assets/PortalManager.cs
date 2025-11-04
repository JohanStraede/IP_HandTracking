using UnityEngine;
using UnityEngine.VFX;

public class PortalManager : MonoBehaviour
{
    [SerializeField] private VisualEffect portalEffect;
    private int currentSpawnRate = 0;
    [SerializeField] private int spawnRateIncrement = 100; // Amount to increase each press (increased a bit)
    [SerializeField] private float spawnRateDecreasePerSecond = 500f; // decrease speed (per second)
    private int maxSpawnRate = 20000; // Maximum spawn rate limit

    [Header("Portal open settings")]
    [SerializeField] private GameObject portalOpenObject; // object to activate when threshold reached
    [SerializeField] private int openThreshold = 3500; // threshold to open portal
    private bool isPortalOpen = false;

    [Header("Radius settings")]
    [SerializeField] private string radiusParameter = "Radius"; // VFX exposed float parameter name
    [SerializeField] private float radiusMin = 0f;
    [SerializeField] private float radiusMax = 2f;
    
    void Start()
    {
        Debug.Log("PortalManager started on: " + gameObject.name);
        
        // Try to find the effect in multiple ways
        portalEffect = GetComponent<VisualEffect>();
        if (portalEffect == null)
        {
            Debug.Log("Trying to find VFX in children...");
            portalEffect = GetComponentInChildren<VisualEffect>();
            
            if (portalEffect == null)
            {
                Debug.Log("Trying to find VFX in parent...");
                portalEffect = GetComponentInParent<VisualEffect>();
                
                if (portalEffect == null)
                {
                    Debug.LogError("No VisualEffect component found anywhere!");
                }
            }
        }
        
        if (portalEffect != null)
        {
            Debug.Log("Found VFX on: " + portalEffect.gameObject.name);
            portalEffect.SetInt("Spawn rate", 0); // Changed from SetFloat to SetInt
            // ensure radius starts at minimum
            portalEffect.SetFloat(radiusParameter, radiusMin);
        }

        if (portalOpenObject != null)
            portalOpenObject.SetActive(false); // ensure starts closed
    }

    // Update is called once per frame
    void Update()
    {
        if (portalEffect == null) return;

        // Increase on key down (single tap increases once)
        if (Input.GetKeyDown(KeyCode.K))
        {
            currentSpawnRate = Mathf.Min(currentSpawnRate + spawnRateIncrement, maxSpawnRate);
        }

        // Only decrease while K is NOT held. Decrease amount is time-based for consistent behaviour.
        if (!Input.GetKey(KeyCode.K))
        {
            int dec = Mathf.RoundToInt(spawnRateDecreasePerSecond * Time.deltaTime);
            currentSpawnRate = Mathf.Max(currentSpawnRate - dec, 0);
        }

        portalEffect.SetInt("Spawn rate", currentSpawnRate);
        Debug.Log($"Current spawn rate: {currentSpawnRate}");

        // map currentSpawnRate -> radius between radiusMin and radiusMax while below/at openThreshold
        float t = Mathf.Clamp01(currentSpawnRate / (float)openThreshold);
        float radius = Mathf.Lerp(radiusMin, radiusMax, t);
        portalEffect.SetFloat(radiusParameter, radius);

        // optionally scale the portalOpenObject to visualize radius (if assigned)
        if (portalOpenObject != null)
        {
            portalOpenObject.transform.localScale = Vector3.one * radius;
        }

        // Open/close portal object based on threshold
        if (!isPortalOpen && currentSpawnRate >= openThreshold)
        {
            if (portalOpenObject != null) portalOpenObject.SetActive(true);
            isPortalOpen = true;
        }
        else if (isPortalOpen && currentSpawnRate < openThreshold)
        {
            if (portalOpenObject != null) portalOpenObject.SetActive(false);
            isPortalOpen = false;
        }
    }
    
    // Example methods to control VFX parameters
    public void SetSpawnRate(int rate) // Changed parameter type to int
    {
        portalEffect.SetInt("Spawn rate", rate);
    }
    
    public void SetParticleSize(float size)
    {
        portalEffect.SetFloat("ParticleSize", size);
    }
    
    public void SetColor(Color color)
    {
        portalEffect.SetVector4("ParticleColor", color);
    }

    // You can call these from Update() or other methods
    void Example()
    {
        // Basic controls
        portalEffect.Play();                         // Start the effect
        portalEffect.Stop();                         // Stop the effect
        portalEffect.SetFloat("Speed", 1.0f);        // Set speed
        portalEffect.SetInt("Count", 100);           // Set integer values
        portalEffect.SetVector3("Position", Vector3.zero); // Set position
        portalEffect.SendEvent("OnPlay");            // Trigger events
    }
}
