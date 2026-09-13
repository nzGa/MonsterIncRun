using UnityEngine;

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

    void Start()
    {
        if (Camera.main == null)
        {
            enabled = false;
            return;
        }

        _mainCamera = Camera.main.transform;
        Vector3 angles = _mainCamera.eulerAngles;
        _mouseX = angles.y;
        _mouseY = angles.x;
        _relativeDistance = cameraIniDistance;
    }

    void LateUpdate()
    {
        if (_mainCamera == null || Cursor.lockState != CursorLockMode.Locked)
            return;

        _mouseX += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
        _mouseY -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        _mouseY = ClampAngle(_mouseY, yMinLimit, yMaxLimit);

        Quaternion rotation = Quaternion.Euler(_mouseY, _mouseX, 0);
        Vector3 position = rotation * new Vector3(0f, alturaOjos, -_relativeDistance) + transform.position;
        _mainCamera.rotation = rotation;
        _mainCamera.position = position;
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
