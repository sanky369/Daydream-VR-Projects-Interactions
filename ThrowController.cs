/*Grabbing and Throwing objects using Daydream Controller*/
//This code is for grabbing and throwing objects using Daydream Controller. It uses the GvrLaserPointerImpl to pickup and throw objects tagged as grabable. Event triggers on objects are not required for this script.
//1. On controller clickdown, detect the gameobject, check whether its grabable. Add a configurable joint to the gameobject this script.
//2. While the controller touchpad is pressed/clicked, set the object as child of controller and match its rotation to controller's orientation and position it at the wrist joint. 
//3. On controller click up, destroy the configurable joint, remove connected body and throw the object with a throw velocity(equal to difference in controller velocity) and controller's angular velocity.
//Attach this script to the GvrControllerPointer gameobject, add a rigid body to it and set it IsKinematic to true.


using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ThrowController: MonoBehaviour  {
	bool objectThrown = false;
	bool objectPicked = false;
	Vector3 throwVelocity;
	private Vector3 objLastPosition;
	public GameObject controller;
	public GameObject laser;
	GameObject objectDetected;
	GameObject objectGrabbed;
	GvrLaserPointerImpl laserpointerimpl;

	void Start () {
		throwVelocity = new Vector3 (1f, 1f, 1f);
	}
	void Update () {
		laserpointerimpl = (GvrLaserPointerImpl)GvrPointerManager.Pointer;
		Quaternion ori = GvrController.Orientation;
		Ray ray = laserpointerimpl.PointerIntersectionRay;
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100)) {
			if(GvrController.ClickButtonDown) {
				objectDetected = hit.collider.gameObject;
				if (objectDetected.tag == "grabable") {
					objectGrabbed = objectDetected;
					var joint = AddConfigurableJoint();
					joint.connectedBody = objectGrabbed.GetComponent<Rigidbody> ();
					objectPicked = true;
					objectThrown = false;
				}
			}
		}

		if (objectPicked) {
			if (GvrController.ClickButton) {
				objectGrabbed.transform.parent = gameObject.transform;
				objectGrabbed.transform.localPosition = GvrController.ArmModel.pointerPosition;
				objectGrabbed.transform.localRotation = ori;
				controller.SetActive (false);
				laser.SetActive (false);
			}
		}
			
		throwVelocity = (GvrController.ArmModel.wristPosition - objLastPosition)/Time.deltaTime;
		objLastPosition = GvrController.ArmModel.wristPosition;

		if (objectPicked) {
			if(GvrController.ClickButtonUp){
				ThrowObject ();
			}
		}

		if (objectThrown) {
			controller.SetActive (true);
			laser.SetActive (true);
		}
			
	}

	private ConfigurableJoint AddConfigurableJoint()
	{
		ConfigurableJoint fx = gameObject.AddComponent<ConfigurableJoint>();
		fx.axis = GvrController.ArmModel.wristPosition;
		fx.targetAngularVelocity = GvrController.Gyro;
		fx.xMotion = ConfigurableJointMotion.Free;
		fx.yMotion = ConfigurableJointMotion.Free;
		fx.zMotion = ConfigurableJointMotion.Free;
		fx.angularXMotion = ConfigurableJointMotion.Free;
		fx.angularYMotion = ConfigurableJointMotion.Free;
		fx.angularZMotion = ConfigurableJointMotion.Free;
		fx.breakForce = 20;
		fx.breakTorque = 20;
		return fx;
	}


	void ThrowObject () {
		objectGrabbed.transform.parent = null;
		objectGrabbed.transform.localPosition = objectGrabbed.transform.position;
		GetComponent<ConfigurableJoint>().connectedBody = null;
		Destroy(GetComponent<ConfigurableJoint>());
		objectGrabbed.GetComponent<Rigidbody>().velocity = throwVelocity;
		objectGrabbed.GetComponent<Rigidbody>().angularVelocity = GvrController.Gyro;
		objectThrown = true;
		objectPicked = false;
	}

}