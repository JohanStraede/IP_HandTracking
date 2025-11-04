using UnityEngine;
using UnityEngine.VFX;

public class PortalManager : MonoBehaviour
{
    [SerializeField] private VisualEffect portalEffect;
    // Internal spawn rate tracked as float for smooth time-based ramping.
    private float currentSpawnRateF = 0f;
    private int currentSpawnRate => Mathf.RoundToInt(currentSpawnRateF);

    [Header("Spawn rate ramping (units per second)")]
    [SerializeField] private float spawnRateIncreasePerSecond = 200f; // how fast the rate ramps up while condition holds
    [SerializeField] private float spawnRateDecreasePerSecond = 300f; // decrease speed (per second)
    [SerializeField] private int maxSpawnRate = 5000; // Maximum spawn rate limit

    [Header("Portal open settings")]
    [SerializeField] private GameObject portalOpenObject; // object to activate when threshold reached
    [SerializeField] private int openThreshold = 3500; // threshold to open portal
    private bool isPortalOpen = false;

    [Header("Portal radius mapping")]
    [SerializeField] private float portalRadiusMax = 2f; // maximum portal radius value in VFX
    [SerializeField] private float portalRadiusSigmoidSteepness = 10f; // steepness of the sigmoid mapping
    [SerializeField] private float portalRadiusMin = 0.5f; // minimum portal radius value in VFX
    [SerializeField] private float tangensSpeedMin = 2f; // minimum tangens speed
    [SerializeField] private float tangensSpeedMax = 8f; // maximum tangens speed

    [Header("OSC")]
     oscListnerClient osc;
    
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
        }

        if (portalOpenObject != null)
            portalOpenObject.SetActive(false); // ensure starts closed

    //OSC
    // Use the modern API to find any instance (preferred to the deprecated FindObjectOfType)
    osc = FindAnyObjectByType<oscListnerClient>();
        if (osc == null) Debug.LogError("No oscListnerClient found in scene");
    }

    // Update is called once per frame
    void Update()
    {
        if (portalEffect == null) return;

        // Increase on key down (single tap increases once)
        /*if (Input.GetKeyDown(KeyCode.K))
        {
            currentSpawnRate = Mathf.Min(currentSpawnRate + spawnRateIncrement, maxSpawnRate);
        }

        // Only decrease while K is NOT held. Decrease amount is time-based for consistent behaviour.
        if (!Input.GetKey(KeyCode.K))
        {
            int dec = Mathf.RoundToInt(spawnRateDecreasePerSecond * Time.deltaTime);
            currentSpawnRate = Mathf.Max(currentSpawnRate - dec, 0);
        }
*/
        portalEffect.SetInt("Spawn rate", currentSpawnRate);
        Debug.Log($"Current spawn rate: {currentSpawnRate}");

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

        //OSC
        if (osc == null) return;

        bool spell = osc.SpellOn;
        bool rightMoving = osc.RightMoving;
        float rightSpeed = osc.RightSpeed;

        // Use the values (example) - ramp spawn rate smoothly over time (per second)
        if (spell && rightMoving)
        {
            currentSpawnRateF = Mathf.Min(currentSpawnRateF + spawnRateIncreasePerSecond * Time.deltaTime * (rightSpeed + 1f), (float)maxSpawnRate);
        }
        else
        {
            currentSpawnRateF = Mathf.Max(currentSpawnRateF - spawnRateDecreasePerSecond * Time.deltaTime, 0f);
        }
        // Map current spawn rate (0..openThreshold) through a sigmoid-like logistic function
        // and scale to 0..portalRadiusMax. This caps the value between 0 and portalRadiusMax
        // while providing a smooth transition around the midpoint.
        float x = 0f;
        if (openThreshold > 0)
            x = Mathf.Clamp01(currentSpawnRateF / (float)openThreshold);

        // Logistic function centered at 0.5 in normalized space
        float s = portalRadiusSigmoidSteepness;
        float logistic = 1f / (1f + Mathf.Exp(-s * (x - 0.5f)));
        // Map logistic output into [portalRadiusMin, portalRadiusMax]
        float portalRadius = Mathf.Lerp(portalRadiusMin, portalRadiusMax, logistic);
        portalEffect.SetFloat("Portal radius", portalRadius);
        // Map the same logistic to Tangens speed in [tangensSpeedMin, tangensSpeedMax]
        float tangensSpeed = Mathf.Lerp(tangensSpeedMin, tangensSpeedMax, logistic);
        portalEffect.SetFloat("Tangent speed", tangensSpeed);
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
