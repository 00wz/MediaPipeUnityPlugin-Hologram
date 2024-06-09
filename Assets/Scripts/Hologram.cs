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

    private Vector3 _lastValidEyePositionRelationProjectionPlane = Vector3.back * 100;
    private Quaternion _lastValidGyro = Quaternion.identity;

    private bool _gyroCorrectionActive = false;
    public void SetAccelerometerCorrection(bool active) => _gyroCorrectionActive = active;

    private float _centerOfScreenToFrontCameraDistanceMM;
    private float _unitToMillimeterRatioProjectionPlaneLocal;
    private Vector3 _localFrontalCameraPosition;
    public float CenterOfScreenToFrontCameraDistanceMM
    {
      get => _centerOfScreenToFrontCameraDistanceMM;
      set
      {
        _centerOfScreenToFrontCameraDistanceMM = value;
        _unitToMillimeterRatioProjectionPlaneLocal =
          _localFrontalCameraPosition.magnitude / value;
      }
    }

    private void Start()
    {
      Input.gyro.enabled = true;
      _localFrontalCameraPosition = FrontCameraPosition.localPosition;
      CenterOfScreenToFrontCameraDistanceMM = 76f;
    }

    private void Update()
    {
      if (IrisTracker.IrisPositions != null)
      {
        Vector3 eyePositionRelationFrontCameraMM = (IrisTracker.IrisPositions.Value.LeftIrisPositionMM +
          IrisTracker.IrisPositions.Value.RightIrisPositionMM) * 0.5f;

        _lastValidEyePositionRelationProjectionPlane =
          Rotate180ByY(eyePositionRelationFrontCameraMM
          * _unitToMillimeterRatioProjectionPlaneLocal) + _localFrontalCameraPosition;

        _lastValidGyro = IrisTracker.IrisPositions.Value.Attitude;
      }

      if (_gyroCorrectionActive)
      {
        var currentGyro = Input.gyro.attitude;
        Quaternion deltaRotate = QuaternionDiff(_lastValidGyro, currentGyro);
        //Vector3 virtualEyePositionRelationFrontCameraUnit =
        Camera.localPosition =
          GyroToUnity(deltaRotate) * _lastValidEyePositionRelationProjectionPlane;

        //Camera.position = FrontCameraPosition.TransformPoint(virtualEyePositionRelationFrontCameraUnit);
      }
      else
      {
        Camera.localPosition = _lastValidEyePositionRelationProjectionPlane;
        //Camera.position = FrontCameraPosition.TransformPoint(_lastValidEyePositionRelationProjectionPlane);
      }
    }

    private Vector3 Rotate180ByY(Vector3 srs)
    {
      return new Vector3(-srs.x, srs.y, -srs.z);
    }

    private Quaternion GyroToUnity(Quaternion q)
    {
      return new Quaternion(q.x, q.y, -q.z, -q.w);
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
