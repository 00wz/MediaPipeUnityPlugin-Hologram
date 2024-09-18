using Mediapipe;
using Mediapipe.Unity.CoordinateSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollPoseRepeater : MonoBehaviour
{
  [SerializeField] private Transform leftArm;
  [SerializeField] private Transform leftForeArm;

  [SerializeField] private Transform rightArm;
  [SerializeField] private Transform rightForeArm;

  [SerializeField] private bool IsEnable = true;

  private IList<Landmark> _poseLandmarks;
  private List<LandmarkList> _handsLandmarks;

  public void HandlePose(IList<Landmark> targets)
  {
    _poseLandmarks = targets;
    //RepeatHandsPositions();
  }

  public void HandleHands(List<LandmarkList> targets)
  {
    _handsLandmarks = targets;
    if(targets == null)
    {
      return;
    }
    for (int i = 0; i < targets.Count; i++)
    {
      var handLandmarks = targets[i].Landmark;
      Debug.DrawLine(handLandmarks[0].ToVector(), handLandmarks[1].ToVector());
      Debug.DrawLine(handLandmarks[1].ToVector(), handLandmarks[2].ToVector());
      Debug.DrawLine(handLandmarks[2].ToVector(), handLandmarks[3].ToVector());
      Debug.DrawLine(handLandmarks[3].ToVector(), handLandmarks[4].ToVector());

      Debug.DrawLine(handLandmarks[0].ToVector(), handLandmarks[5].ToVector());
      Debug.DrawLine(handLandmarks[0].ToVector(), handLandmarks[17].ToVector());
      Debug.DrawLine(handLandmarks[5].ToVector(), handLandmarks[9].ToVector());
      Debug.DrawLine(handLandmarks[9].ToVector(), handLandmarks[13].ToVector());
      Debug.DrawLine(handLandmarks[13].ToVector(), handLandmarks[17].ToVector());

      Debug.DrawLine(handLandmarks[5].ToVector(), handLandmarks[6].ToVector());
      Debug.DrawLine(handLandmarks[6].ToVector(), handLandmarks[7].ToVector());
      Debug.DrawLine(handLandmarks[7].ToVector(), handLandmarks[8].ToVector());
    }
  }

  private void RepeatHandsPositions()
  {
    if (_poseLandmarks == null)
    {
      return;
    }

    if (!IsEnable)
    {
      return;
    }

    //left
    Vector3 leftArmDir = _poseLandmarks[13].ToVector() - _poseLandmarks[11].ToVector();
    Vector3 leftForeArmDir = _poseLandmarks[15].ToVector() - _poseLandmarks[13].ToVector();
    Vector3 leftArmDownDir = Vector3.Cross(leftForeArmDir, leftArmDir);

    leftArm.rotation = Quaternion.LookRotation(leftArmDownDir, leftArmDir);

    Vector3 leftHandThumbDir = _poseLandmarks[19].ToVector() - _poseLandmarks[17].ToVector();
    Vector3 leftForeArmDownDir = Vector3.Cross(leftHandThumbDir, leftForeArmDir);

    leftForeArm.rotation = Quaternion.LookRotation(leftForeArmDownDir, leftForeArmDir);

    //right
    Vector3 rightArmDir = _poseLandmarks[14].ToVector() - _poseLandmarks[12].ToVector();
    Vector3 rightForeArmDir = _poseLandmarks[16].ToVector() - _poseLandmarks[14].ToVector();
    Vector3 rightArmDownDir = Vector3.Cross(rightArmDir, rightForeArmDir);

    rightArm.rotation = Quaternion.LookRotation(rightArmDownDir, rightArmDir);

    Vector3 rightHandThumbDir = _poseLandmarks[20].ToVector() - _poseLandmarks[18].ToVector();
    Vector3 rightForeArmDownDir = Vector3.Cross(rightForeArmDir, rightHandThumbDir);

    rightForeArm.rotation = Quaternion.LookRotation(rightForeArmDownDir, rightForeArmDir);
  }

  private void LateUpdate()
  {
    RepeatHandsPositions();
  }
}

public static class PoseTrackingExtentions
{
  public static Vector3 ToVector(this Landmark self)
  {
    return RealWorldCoordinate.RealWorldToLocalPoint(self.X, self.Y, self.Z, 1.0f);
    //return new Vector3(self.X, self.Y, self.Z);
  }
}
