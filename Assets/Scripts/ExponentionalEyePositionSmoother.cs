using UnityEngine;
using UnityEngine.UI;

public class ExponentionalEyePositionSmoother : MonoBehaviour
{
  [SerializeField] private float coefficient = 1f;
  [SerializeField] private Text coefficientText;
  [SerializeField] private bool isSlerpSmooth = true;

  public bool IsSlerpSmooth
  {
    get => isSlerpSmooth;
    set
    {
      isSlerpSmooth = value;
    }
  }
  public float Coefficient
  {
    get => coefficient;
    set
    {
      coefficient = value;
      coefficientText.text = value.ToString();
    }
  }

  public Vector3 SmoothedEyesPositionRelationProjectionPlane { get; private set; }
  public Quaternion GyroscopeReadingCorrespondingEyesPosition { get; private set; }

  private Vector3 _lastEyesPositionRelativeProjectionPlane;
  private Quaternion _gyroscopeReadingCorrespondingLastEyesPosition;

  public void Init(Vector3 eyesPositionRelativeProjectionPlane,
    Quaternion correspondingGyroscopeReading)
  {
    SetLastEyesPositionRelativeProjectionPlane(
      eyesPositionRelativeProjectionPlane,
      correspondingGyroscopeReading);
    SmoothedEyesPositionRelationProjectionPlane = eyesPositionRelativeProjectionPlane;
    GyroscopeReadingCorrespondingEyesPosition = _gyroscopeReadingCorrespondingLastEyesPosition;
  }

  public void SetLastEyesPositionRelativeProjectionPlane(Vector3 eyesPositionRelativeProjectionPlane,
    Quaternion correspondingGyroscopeReading)
  {
    _lastEyesPositionRelativeProjectionPlane = eyesPositionRelativeProjectionPlane;
    _gyroscopeReadingCorrespondingLastEyesPosition = correspondingGyroscopeReading;
  }

  private void Update()
  {
    if(isSlerpSmooth)
    {
      SmoothedEyesPositionRelationProjectionPlane = Vector3.SlerpUnclamped(
      SmoothedEyesPositionRelationProjectionPlane,
      _lastEyesPositionRelativeProjectionPlane,
      coefficient);
      GyroscopeReadingCorrespondingEyesPosition = Quaternion.SlerpUnclamped(
      GyroscopeReadingCorrespondingEyesPosition,
      _gyroscopeReadingCorrespondingLastEyesPosition,
      coefficient);
    }
    else
    {
      SmoothedEyesPositionRelationProjectionPlane = Vector3.LerpUnclamped(
      SmoothedEyesPositionRelationProjectionPlane,
      _lastEyesPositionRelativeProjectionPlane,
      coefficient);
      GyroscopeReadingCorrespondingEyesPosition = Quaternion.LerpUnclamped(
      GyroscopeReadingCorrespondingEyesPosition,
      _gyroscopeReadingCorrespondingLastEyesPosition,
      coefficient);
    }
  }
}
