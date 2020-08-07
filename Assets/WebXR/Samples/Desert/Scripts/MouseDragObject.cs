using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MouseDragObject : MonoBehaviour
{
  private Camera currentCamera;
  private new Rigidbody rigidbody;
  private Vector3 screenPoint;
  private Vector3 positionOffset;
  private Quaternion rotationOffset;

  private Vector3 velocity;
  private Vector3 oldPosition;
  private Vector3 newPosition;
  private Quaternion newRotation;

  private 

  void Awake()
  {
    rigidbody = GetComponent<Rigidbody>();
  }

  void OnMouseDown()
  {
    currentCamera = FindCamera();
    if (currentCamera != null)
    {
      screenPoint = currentCamera.WorldToScreenPoint(rigidbody.position);
      positionOffset = Quaternion.Inverse(currentCamera.transform.rotation) * (rigidbody.position - currentCamera.ScreenToWorldPoint(GetMousePosWithScreenZ(screenPoint.z)));
      rotationOffset = Quaternion.Inverse(currentCamera.transform.rotation) * rigidbody.rotation;
    }
  }

  void OnMouseUp()
  {
    currentCamera = null;
    rigidbody.velocity = Vector3.ClampMagnitude(velocity * 10f, 5f);
  }

  void Update()
  {
    if (currentCamera != null)
    {
      Vector3 currentScreenPoint = GetMousePosWithScreenZ(screenPoint.z);
      oldPosition = newPosition;
      newPosition = currentCamera.transform.rotation * positionOffset + currentCamera.ScreenToWorldPoint(currentScreenPoint);
      velocity = newPosition - oldPosition;
      newRotation = currentCamera.transform.rotation * rotationOffset;
    }
  }

  void FixedUpdate()
  {
    if (currentCamera != null)
    {
      rigidbody.angularVelocity = Vector3.zero;
      rigidbody.velocity = velocity;
      rigidbody.MovePosition(newPosition);
      rigidbody.MoveRotation(newRotation);
    }
  }

  Vector3 GetMousePosWithScreenZ(float screenZ)
  {
    return new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenZ);
  }

  Camera FindCamera()
  {
    Camera[] cameras = FindObjectsOfType<Camera>();
    Camera result = null;
    int camerasSum = 0;
    foreach (var camera in cameras)
    {
      if (camera.enabled)
      {
        result = camera;
        camerasSum++;
      }
    }
    if (camerasSum > 1)
    {
      result = null;
    }
    return result;
  }
}
