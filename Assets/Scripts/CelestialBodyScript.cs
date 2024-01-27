using System;
using System.Collections.Generic;
using System.Linq;
using GraphicsLabor.Scripts.Attributes.LaborerAttributes.InspectedAttributes;
using UnityEngine;

public class CelestialBodyScript : MonoBehaviour
{
    [SerializeField] private string _planetName = "";
    [SerializeField] private CelestialType _celestialType;
    [SerializeField] private float _mass = 20;
    [SerializeField] private Color _color;
    [SerializeField] private Vector3 _initialVelocity;
    [SerializeField] private SphereCollider _sphereCollider;
    public CelestialType CelestialType => _celestialType;
    
    [ReadOnly, SerializeField] private List<CelestialBodyScript> _inRangeBodies = new();
    private Rigidbody _rb;

    public string PlanetName => _planetName;

    private float _colliderRadius;
    public float PlanetRadius => _sphereCollider.radius * transform.localScale.x;

    public Vector3 Velocity => _rb.velocity;
    public bool Colliding { get; private set; }
    public float Mass => _mass;

    public static event Action<int> BodyDestroyed; // Takes InstanceID 
    public static event Action<CelestialBodyScript> BodyCreated; // Takes InstanceID 

    private void OnEnable()
    {
        BodyDestroyed += OnBodyDestroyed;
    }
    
    private void OnDisable()
    {
        BodyDestroyed -= OnBodyDestroyed;
    }

    private void OnBodyDestroyed(int instanceId)
    {
        foreach (CelestialBodyScript bodyScript in _inRangeBodies.Where(bodyScript => bodyScript.GetInstanceID() == instanceId))
        {
            _inRangeBodies.Remove(bodyScript);
            return;
        }
    }

    private void NotifyDestruction(int instanceId)
    {
        BodyDestroyed?.Invoke(instanceId);
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = _initialVelocity;
        GetComponent<TrailRenderer>().startColor = _color;
        GetComponent<TrailRenderer>().endColor = _color;
        GetComponent<MeshRenderer>().material.color = _color;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PlanetName == "")
        {
            _planetName = gameObject.name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 totalForce = CalculateTotalForce();

        _rb.velocity += totalForce * Time.fixedDeltaTime;
    }

    private Vector3 CalculateTotalForce()
    {
        Vector3 force = Vector3.zero;
        foreach (CelestialBodyScript body in _inRangeBodies)
        {
             Vector3 direction = body.transform.position - transform.position;
             float totalMass = body.Mass;

             force += totalMass * direction.normalized / (direction.magnitude * direction.magnitude) * 6.674f;
        }

        return force;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        CelestialBodyScript script = other.GetComponent<CelestialBodyScript>();
        if (script != null)
        {
            _inRangeBodies.Add(script);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        CelestialBodyScript script = other.GetComponent<CelestialBodyScript>();
        if (script != null)
        {
            _inRangeBodies.Remove(script);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (Colliding) return;
        CelestialBodyScript bodyScript = other.gameObject.GetComponent<CelestialBodyScript>();
        
        if (bodyScript == null) return;
        if(bodyScript.Colliding) return;
            
        Colliding = true;
        bodyScript.Colliding = true;
        CollisionHandler.CalculateCollision(this, bodyScript)?.DoCollision();
    }

    public void Config(CelestialType resultingType, float mass, Vector3 velocity)
    {
        _celestialType = resultingType;
        _mass = mass;
        _initialVelocity = velocity;
        _rb.velocity = velocity;
    }

    /// <summary>
    /// Always use DestroySelf to destroy the gameobject
    /// </summary>
    public void DestroySelf()
    {
        NotifyDestruction(GetInstanceID());
        Destroy(gameObject);
    }

    public void Create()
    {
        BodyCreated?.Invoke(this);
    }
}
