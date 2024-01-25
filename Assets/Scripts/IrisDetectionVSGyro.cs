using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HologramPipe
{
  public class IrisDetectionVSGyro : MonoBehaviour
  {
    [SerializeField]
    private IrisTracker IrisTracker;

    public Text FocalLengthPxText;
    [SerializeField]
    GameObject EyeDirection;
    [SerializeField]
    GameObject GyroscopeDirection;

    Vector3 lastValidFacePositionInRelationToCamera = Vector3.forward;
    Vector3 lastSelectedDirection = Vector3.forward;
    Quaternion lastValidGyro = Quaternion.identity;

    public void OnFocalLengthCange(float value)
    {
      FocalLengthPxText.text = value.ToString();
      IrisTracker.FocalLengthPixel = value;
    }

    private void Start()
    {
      Input.gyro.enabled = true;
      FocalLengthPxText.text = IrisTracker.FocalLengthPixel.ToString();
    }

    private void Update()
    {
      if (IrisTracker.IrisPositions != null)
      {
        lastValidFacePositionInRelationToCamera = (IrisTracker.IrisPositions.Value.LeftIrisPositionMM +
          IrisTracker.IrisPositions.Value.RightIrisPositionMM) * 0.5f;

        EyeDirection.transform.LookAt(EyeDirection.transform.position +
          transform.TransformVector(lastValidFacePositionInRelationToCamera));

      }

      var current = Input.gyro.attitude;
      Quaternion deltaRotate = QuaternionDiff(lastValidGyro, current);
      Vector3 virtualFacePosition = FrontalCameraGyroToUnity(deltaRotate) * lastSelectedDirection;

      GyroscopeDirection.transform.LookAt(GyroscopeDirection.transform.position +
        transform.TransformVector(virtualFacePosition));
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

    public void SelectDirection()
    {
      lastValidGyro = Input.gyro.attitude;
      lastSelectedDirection = lastValidFacePositionInRelationToCamera;
    }
  }
}

