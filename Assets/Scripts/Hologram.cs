using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HologramPipe
{
  public class Hologram : MonoBehaviour
  {
    [SerializeField]
    private IrisTracker IrisTracker;
    [SerializeField]
    private Transform FrontCameraPosition;
    [SerializeField]
    private Transform CenterOfScreenPosition;
    [SerializeField]
    private Transform Camera;

    private Vector3 _lastValidEyePositionRelationFrontCameraUnit = Vector3.forward;
    private Vector3 _lastSelectedDirection = Vector3.forward;
    private Quaternion _lastValidGyro = Quaternion.identity;

    private bool _gyroCorrectionActive = false;
    public void SetAccelerometerCorrection(bool active) => _gyroCorrectionActive = active;

    private float _centerOfScreenToFrontCameraDistanceMM;
    private float _unitToMillimeterRatio;
    public float CenterOfScreenToFrontCameraDistanceMM
    {
      get => _centerOfScreenToFrontCameraDistanceMM;
      set
      {
        _centerOfScreenToFrontCameraDistanceMM = value;
        _unitToMillimeterRatio =
          (FrontCameraPosition.position - CenterOfScreenPosition.position).magnitude / value;
      }
    }

    private void Start()
    {
      Input.gyro.enabled = true;
      CenterOfScreenToFrontCameraDistanceMM = 76f;
    }

    private void Update()
    {
      if (IrisTracker.IrisPositions != null)
      {
        Vector3 eyePositionRelationFrontCameraMM = (IrisTracker.IrisPositions.Value.LeftIrisPositionMM +
          IrisTracker.IrisPositions.Value.RightIrisPositionMM) * 0.5f;

        _lastValidEyePositionRelationFrontCameraUnit = eyePositionRelationFrontCameraMM
          * _unitToMillimeterRatio;
        _lastValidGyro = IrisTracker.IrisPositions.Value.Attitude;
      }

      if (_gyroCorrectionActive)
      {
        var currentGyro = Input.gyro.attitude;
        Quaternion deltaRotate = QuaternionDiff(_lastValidGyro, currentGyro);
        Vector3 virtualEyePositionRelationFrontCameraUnit =
          FrontalCameraGyroToUnity(deltaRotate) * _lastValidEyePositionRelationFrontCameraUnit;

        Camera.position = FrontCameraPosition.TransformPoint(virtualEyePositionRelationFrontCameraUnit);
      }
      else
      {
        Camera.position = FrontCameraPosition.TransformPoint(_lastValidEyePositionRelationFrontCameraUnit);
      }
    }

    private Quaternion FrontalCameraGyroToUnity(Quaternion q)
    {
      return new Quaternion(-q.x, q.y, q.z, -q.w);
    }

    private Quaternion QuaternionDiff(Quaternion srs, Quaternion delta)
    {
      return Quaternion.Inverse(delta) * srs;
    }
  }
}
