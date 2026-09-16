using UnityEngine;

// Orbits Camera.main around this transform. Mouse yaw never rotates the player.
public class MouseOrbit : MonoBehaviour
{
    public float alturaOjos = 0.6f;
    public float cameraIniDistance = 2;
    public float xSpeed = 250f;
    public float ySpeed = 120f;
    public float yMaxLimit = 50;
    public float yMinLimit = -20;

    Transform _mainCamera;
    float _mouseX;
    float _mouseY;
    float _relativeDistance;
    bool _yawInicializado;

    void Start()
    {
        ResolverCamara();
        if (_mainCamera == null)
            return;

        if (!_yawInicializado)
        {
            Vector3 angles = _mainCamera.eulerAngles;
            _mouseX = angles.y;
            float pitch = angles.x;
            if (pitch > 180f)
                pitch -= 360f;
            _mouseY = ClampAngle(pitch, yMinLimit, yMaxLimit);
            _yawInicializado = true;
        }
        _relativeDistance = cameraIniDistance;
        AplicarTransformCamara();
    }

    void LateUpdate()
    {
        if (_mainCamera == null)
            ResolverCamara();
        if (_mainCamera == null)
            return;

        if (PuedeOrbitar())
        {
            _mouseX += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            _mouseY -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            _mouseY = ClampAngle(_mouseY, yMinLimit, yMaxLimit);
        }

        AplicarTransformCamara();
    }

    static bool PuedeOrbitar()
    {
        return GestionaMultiJugador.ControlJugadorActivo;
    }

    void ResolverCamara()
    {
        if (Camera.main == null)
            return;

        _mainCamera = Camera.main.transform;
        if (_mainCamera.parent != null)
            _mainCamera.SetParent(null, true);
    }

    void AplicarTransformCamara()
    {
        Quaternion rotation = Quaternion.Euler(_mouseY, _mouseX, 0f);
        Vector3 position = rotation * new Vector3(0f, alturaOjos, -_relativeDistance) + transform.position;
        _mainCamera.SetPositionAndRotation(position, rotation);
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
