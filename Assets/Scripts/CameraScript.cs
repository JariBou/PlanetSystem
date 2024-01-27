using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private CelestialBodyScript _followedGameObject;
    [Header("FreeCam"), SerializeField] private float _lookAroundSpeed = 0.01f;
    [SerializeField] private float _moveSpeed = 0.01f;
    [SerializeField] private float _minSpeed = 0.01f;
    [SerializeField] private float _maxSpeed = 1000000f;
    [SerializeField] private float _maxRaycastDistance = 1000f;
    [Header("PlanetFollow"), SerializeField] private float _rotationSpeed = 0.01f;
    [SerializeField] private float _movSpeedScrollSpeed = 0.001f;
    [SerializeField] private float _minZoom = 0.01f;
    [SerializeField] private float _maxZoom = 100000f;
    [FormerlySerializedAs("_scrollSpeed")] [SerializeField] private float _zoomScrollSpeed = 0.01f;
    
    [Header("Temp"), SerializeField] private List<CelestialBodyScript> _celestialBodyScripts;
    private int _currPlanetIndex;

    private bool _freeCam;
    private float _currZoom = 10;
    private Vector2 _currRotation;
    private Vector2 _moveVec;
    private CelestialBodyScript _lookedAtBody;
    private float _verticalDelta;

    public CelestialBodyScript CurrentPlanet => _followedGameObject;
    public float CurrentZoom => _currZoom;
    public float CurrentSpeed => _moveSpeed;
    public bool FreeCamEnabled => _freeCam;
    public CelestialBodyScript LookedAtBody => _lookedAtBody;

    private void OnEnable()
    {
        CelestialBodyScript.BodyDestroyed += OnCelestialBodyDestroyed;
        CelestialBodyScript.BodyCreated += OnCelestialBodyCreated;
    }
    
    private void OnDisable()
    {
        CelestialBodyScript.BodyDestroyed -= OnCelestialBodyDestroyed;
        CelestialBodyScript.BodyCreated -= OnCelestialBodyCreated;
    }

    private void OnCelestialBodyCreated(CelestialBodyScript obj)
    {
        _celestialBodyScripts.Add(obj);
    }

    private void OnCelestialBodyDestroyed(int instanceID)
    {
        try
        {
            _celestialBodyScripts.Remove(_celestialBodyScripts.First(script => script.GetInstanceID() == instanceID));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        if (_followedGameObject == null) return;
        if (_followedGameObject.GetInstanceID() == instanceID)
        {
            ActivateFreeCam();
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        foreach (CelestialBodyScript child in GalaxyConfigurator.Instance.PlanetsParent.GetComponentsInChildren<CelestialBodyScript>())
        {
            if (!_celestialBodyScripts.Contains(child))
            {
                _celestialBodyScripts.Add(child);
            }
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        _currPlanetIndex = _followedGameObject != null ? _celestialBodyScripts.IndexOf(_followedGameObject) : 0;
        UpdateFollowedGameObject(_celestialBodyScripts[_currPlanetIndex]);
    }

    private void UpdateFollowedGameObject(CelestialBodyScript followedGameObject)
    {
        DeactivateFreeCam();
        _followedGameObject = followedGameObject;
        _minZoom = _followedGameObject.PlanetRadius * 2f;
        _currZoom = Math.Clamp(_currZoom, _minZoom*_followedGameObject.PlanetRadius*2f, _maxZoom);
    }

    private void ActivateFreeCam()
    {
        _followedGameObject = null;
        _freeCam = true;
        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.z = 0;
        transform.rotation = Quaternion.Euler(rotation);
    }
    
    private void DeactivateFreeCam()
    {
        _freeCam = false;
        _moveVec = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            Time.timeScale += Mathf.Clamp(Mathf.Pow(2, (int)Time.timeScale) / 10f, 0.1f, 2f);
        }else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            Time.timeScale = Math.Clamp(Time.timeScale - Mathf.Clamp(Mathf.Pow(2, (int)Time.timeScale-1) / 10f, 0.1f, 2f), 0, 1000f);
        }
        
        if (!_freeCam) LookAtPoint(_followedGameObject.transform.position);
        else
        {
            TryGetLookedAtBody(out _lookedAtBody);
            transform.position += (_moveVec.x * transform.right + _moveVec.y * transform.forward) * _moveSpeed;
            transform.position += Vector3.up * _verticalDelta * _moveSpeed;
        }
    }

    private void LookAtPoint(Vector3 point)
    {
        transform.position = point;
        
        transform.rotation = Quaternion.Euler(new Vector3(0, 1, 0) * _currRotation.x + new Vector3(1, 0, 0) * _currRotation.y);
            
        transform.Translate(new Vector3(0, 0, -_currZoom));
    }
    
    private float Mod2(float x, float m)
    {
        int q = (int)(x / m);
        float r = x - q * m;
        return r;
    }
    
    private bool TryGetLookedAtBody(out CelestialBodyScript bodyScript)
    {
        Vector3 direction = transform.forward;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, _maxRaycastDistance))
        {
            bodyScript = hit.rigidbody.GetComponent<CelestialBodyScript>();
            if (bodyScript == null) return false;

            return true;
        }

        bodyScript = null;
        return false;
    }
    
    public float DistanceFrom(CelestialBodyScript cameraLookedAtBody)
    {
        return (cameraLookedAtBody.transform.position - transform.position).magnitude;
    }

    #region InputActions

    public void OnMouseMoved(InputAction.CallbackContext callbackContext)
    {
        Vector2 delta = callbackContext.ReadValue<Vector2>() ;
        if (_freeCam)
        {
            delta *= _lookAroundSpeed;
            _currRotation.x += delta.x;
            _currRotation.y -= delta.y;
            
            var xQuat = Quaternion.AngleAxis(_currRotation.x, Vector3.up);
            var yQuat = Quaternion.AngleAxis(_currRotation.y, Vector3.right);

            transform.rotation = xQuat * yQuat;
        } else
        {
            delta *= _rotationSpeed;
            _currRotation.x += delta.x;
            _currRotation.y -= delta.y;
            _currRotation.x = Mod2((int)_currRotation.x, 360);
            _currRotation.y = Mathf.Clamp(_currRotation.y, -89, 89);
        }
    }
    
    public void OnMouseScroll(InputAction.CallbackContext callbackContext)
    {
        Vector2 delta = callbackContext.ReadValue<Vector2>();
        if (_freeCam)
        {
            delta *= _movSpeedScrollSpeed;
            _moveSpeed = Math.Clamp(_moveSpeed + delta.y, _minSpeed, _maxSpeed);
        }
        else
        {
            delta *= _zoomScrollSpeed;
            _currZoom = Math.Clamp(_currZoom - delta.y, _minZoom, _maxZoom);
        }
    }
    
    public void OnFreeCamKeyPressed(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed || _freeCam) return;
        ActivateFreeCam();
    }

    public void OnKeyboardMove(InputAction.CallbackContext callbackContext)
    {
        if (!_freeCam) return;
        _moveVec = callbackContext.ReadValue<Vector2>().normalized;
    }
    
    public void PrevPlanet(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;

        _currPlanetIndex = _currPlanetIndex == 0 ? _celestialBodyScripts.Count-1 : _currPlanetIndex - 1;
        UpdateFollowedGameObject(_celestialBodyScripts[_currPlanetIndex]);
    }
    
    public void NextPlanet(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;

        _currPlanetIndex = (_currPlanetIndex + 1) % _celestialBodyScripts.Count;
        UpdateFollowedGameObject(_celestialBodyScripts[_currPlanetIndex]);
    }
    
    public void OnClick(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed || !_freeCam) return;

        if (TryGetLookedAtBody(out CelestialBodyScript bodyScript))
        {
            UpdateFollowedGameObject(bodyScript);
        }
    }
    
    public void OnVerticalFreeMovement(InputAction.CallbackContext callbackContext)
    {
        if (!_freeCam) return;

        _verticalDelta = callbackContext.ReadValue<float>();
    }
    
    #endregion

}
