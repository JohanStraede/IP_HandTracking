using UnityEngine;
using UnityEngine.VFX;

public class PortalManager : MonoBehaviour
{
    [SerializeField] private VisualEffect portalEffect;
    private int currentSpawnRate = 0;
    private int spawnRateIncrement = 50; // Amount to increase each press
    private int maxSpawnRate = 20000; // Maximum spawn rate limit

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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            currentSpawnRate = Mathf.Min(currentSpawnRate + spawnRateIncrement, maxSpawnRate);
            portalEffect.SetInt("Spawn rate", currentSpawnRate);
            Debug.Log($"Spawn rate increased to: {currentSpawnRate}");
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
