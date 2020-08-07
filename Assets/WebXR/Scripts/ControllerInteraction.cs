using UnityEngine;
using System.Collections.Generic;

namespace WebXR
{
  public class ControllerInteraction : MonoBehaviour
  {
    private Rigidbody currentRigidBody = null;
    private Transform controllerTransform;
    private List<Rigidbody> contactRigidBodies = new List<Rigidbody>();

    private Vector3 positionOffset;
    private Quaternion rotationOffset;

    private Vector3 velocity;
    private Vector3 oldPosition;
    private Vector3 newPosition;
    private Quaternion newRotation;

    private Animator anim;
    private WebXRController controller;

    void Awake()
    {
      controllerTransform = transform;
    }

    void Start()
    {
      anim = gameObject.GetComponent<Animator>();
      controller = gameObject.GetComponent<WebXRController>();
    }

    void OnEnable()
    {
      if (controller == null)
      {
        controller = gameObject.GetComponent<WebXRController>();
      }
      controller.OnTransformUpdate += HandleOnTransformUpdate;
    }

    void OnDisable()
    {
      controller.OnTransformUpdate -= HandleOnTransformUpdate;
    }

    void Update()
    {
      float normalizedTime = controller.GetButton("Trigger") ? 1 : controller.GetAxis("Grip");

      if (controller.GetButtonDown("Trigger") || controller.GetButtonDown("Grip"))
        Pickup();

      if (controller.GetButtonUp("Trigger") || controller.GetButtonUp("Grip"))
        Drop();

      // Use the controller button or axis position to manipulate the playback time for hand model.
      anim.Play("Take", -1, normalizedTime);
    }

    void HandleOnTransformUpdate()
    {
      if (currentRigidBody != null)
      {
        oldPosition = newPosition;
        newPosition = controllerTransform.rotation * positionOffset + controllerTransform.position;
        velocity = newPosition - oldPosition;
        newRotation = controllerTransform.rotation * rotationOffset;
      }
    }

    void FixedUpdate()
    {
      if (currentRigidBody != null)
      {
        currentRigidBody.angularVelocity = Vector3.zero;
        currentRigidBody.velocity = Vector3.zero;
        currentRigidBody.MovePosition(newPosition);
        currentRigidBody.MoveRotation(newRotation);
      }
    }

    void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.tag != "Interactable")
        return;

      contactRigidBodies.Add(other.gameObject.GetComponent<Rigidbody>());
      controller.Pulse(0.5f, 250);
    }

    void OnTriggerExit(Collider other)
    {
      if (other.gameObject.tag != "Interactable")
        return;

      contactRigidBodies.Remove(other.gameObject.GetComponent<Rigidbody>());
    }

    public void Pickup()
    {
      currentRigidBody = GetNearestRigidBody();

      if (!currentRigidBody)
        return;

      positionOffset = Quaternion.Inverse(controllerTransform.rotation) * (currentRigidBody.position - controllerTransform.position);
      rotationOffset = Quaternion.Inverse(controllerTransform.rotation) * currentRigidBody.rotation;
      HandleOnTransformUpdate();
      velocity = Vector3.zero;
    }

    public void Drop()
    {
      if (!currentRigidBody)
        return;

      currentRigidBody.velocity = Vector3.ClampMagnitude(velocity * 20f, 5f);
      currentRigidBody = null;
    }

    private Rigidbody GetNearestRigidBody()
    {
      Rigidbody nearestRigidBody = null;
      float minDistance = float.MaxValue;
      float distance = 0.0f;

      foreach (Rigidbody contactBody in contactRigidBodies)
      {
        distance = (contactBody.gameObject.transform.position - transform.position).sqrMagnitude;

        if (distance < minDistance)
        {
          minDistance = distance;
          nearestRigidBody = contactBody;
        }
      }

      return nearestRigidBody;
    }
  }
}
