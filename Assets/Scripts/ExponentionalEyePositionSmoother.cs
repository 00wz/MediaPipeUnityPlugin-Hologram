using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExponentionalEyePositionSmoother : MonoBehaviour
{
  public Vector3 SmoothedEyesPositionRelationProjectionPlane { get; private set; }
  public Quaternion GyroscopeReadingCorrespondingEyesPosition { get; private set; }

  private Vector3 _lastEyesPositionRelativeProjectionPlane;
  private Quaternion _gyroscopeReadingCorrespondingLastEyesPosition;

  public void SetLastEyesPositionRelativeProjectionPlane(Vector3 eyesPositionRelativeProjectionPlane,
    Quaternion correspondingGyroscopeReading)
  {
    _lastEyesPositionRelativeProjectionPlane = eyesPositionRelativeProjectionPlane;
    _gyroscopeReadingCorrespondingLastEyesPosition = correspondingGyroscopeReading;
  }

  void Update()
  {
        
  }
}
