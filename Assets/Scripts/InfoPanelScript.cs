using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class InfoPanelScript : MonoBehaviour
{
    [SerializeField] private CameraScript _camera;
    [SerializeField] private TMP_Text _planetName;
    [FormerlySerializedAs("_zoomLevel")] [SerializeField] private TMP_Text _zoomOrSpeedLevel;
    [SerializeField] private TMP_Text _timeScaleText;
    [SerializeField] private TMP_Text _lookingAtScript;
    [SerializeField] private GameObject _aimGameObject;
    

    private void FixedUpdate()
    {
        _planetName.text = _camera.FreeCamEnabled ? "Using FreeCam" : $"Looking at: {_camera.CurrentPlanet.PlanetName}";
        _zoomOrSpeedLevel.text = _camera.FreeCamEnabled ? $"Current Speed: {_camera.CurrentSpeed:0.0}" : $"Current Zoom: {_camera.CurrentZoom:0.0}";
        _timeScaleText.text = $"Current Time Scale: {Time.timeScale:0.0}";

        _aimGameObject.SetActive(_camera.FreeCamEnabled);
        
        _lookingAtScript.text = "";
        if (_camera.FreeCamEnabled)
        {
            if (_camera.LookedAtBody != null)
            {
                _lookingAtScript.text =
                    $"{_camera.LookedAtBody.PlanetName} \n {_camera.DistanceFrom(_camera.LookedAtBody):0.0}";
            }
        }
    }
}
