using UnityEngine;

public class SimpleParticleEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    public int particleCount = 20;
    public float particleSpeed = 5f;
    public float particleLifetime = 2f;
    public Color startColor = Color.red;
    public Color endColor = Color.yellow;
    public float startSize = 0.1f;
    public float endSize = 0.05f;
    public bool useGravity = false;
    
    [Header("Prefab")]
    public GameObject particlePrefab; // Simple cube or sphere for particles
    
    void Start()
    {
        CreateParticles();
    }
    
    void CreateParticles()
    {
        for (int i = 0; i < particleCount; i++)
        {
            // Create particle
            GameObject particle;
            if (particlePrefab != null)
            {
                particle = Instantiate(particlePrefab, transform.position, Quaternion.identity);
            }
            else
            {
                // Create a simple cube particle if no prefab provided
                particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                particle.transform.position = transform.position;
                particle.transform.localScale = Vector3.one * startSize;
            }
            
            // Add rigidbody for physics
            Rigidbody rb = particle.GetComponent<Rigidbody>();
            if (rb == null)
                rb = particle.AddComponent<Rigidbody>();
            
            rb.useGravity = useGravity;
            
            // Random direction
            Vector3 randomDirection = Random.onUnitSphere;
            rb.AddForce(randomDirection * particleSpeed, ForceMode.Impulse);
            
            // Add rotation
            rb.AddTorque(Random.onUnitSphere * particleSpeed, ForceMode.Impulse);
            
            // Set initial color and material
            Renderer renderer = particle.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = startColor;
                renderer.material = mat;
            }
            
            // Add particle behavior component
            ParticleBehavior behavior = particle.AddComponent<ParticleBehavior>();
            behavior.Initialize(particleLifetime, startColor, endColor, startSize, endSize);
        }
        
        // Destroy this effect object after all particles are done
        Destroy(gameObject, particleLifetime + 1f);
    }
}

public class ParticleBehavior : MonoBehaviour
{
    private float lifetime;
    private float timer;
    private Color startColor;
    private Color endColor;
    private float startSize;
    private float endSize;
    private Vector3 initialScale;
    private Renderer particleRenderer;
    
    public void Initialize(float life, Color startCol, Color endCol, float startSz, float endSz)
    {
        lifetime = life;
        startColor = startCol;
        endColor = endCol;
        startSize = startSz;
        endSize = endSz;
        timer = 0f;
        
        initialScale = transform.localScale;
        particleRenderer = GetComponent<Renderer>();
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / lifetime;
        
        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }
        
        // Fade color
        if (particleRenderer != null)
        {
            Color currentColor = Color.Lerp(startColor, endColor, progress);
            currentColor.a = 1f - progress; // Fade out
            particleRenderer.material.color = currentColor;
        }
        
        // Scale down
        float currentSize = Mathf.Lerp(startSize, endSize, progress);
        transform.localScale = initialScale * currentSize;
    }
}
