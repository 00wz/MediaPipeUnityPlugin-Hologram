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

  private IList<Landmark> targets;

  public void HandlePose(IList<Landmark> targets)
  {
    this.targets = targets;
    //RepeatHandsPositions();
  }

  private void RepeatHandsPositions()
  {
    if (targets == null)
    {
      return;
    }

    if (!IsEnable)
    {
      return;
    }

    //left
    Vector3 leftArmDir = targets[13].ToVector() - targets[11].ToVector();
    Vector3 leftForeArmDir = targets[15].ToVector() - targets[13].ToVector();
    Vector3 leftArmDownDir = Vector3.Cross(leftForeArmDir, leftArmDir);

    leftArm.rotation = Quaternion.LookRotation(leftArmDownDir, leftArmDir);

    Vector3 leftHandThumbDir = targets[19].ToVector() - targets[17].ToVector();
    Vector3 leftForeArmDownDir = Vector3.Cross(leftHandThumbDir, leftForeArmDir);

    leftForeArm.rotation = Quaternion.LookRotation(leftForeArmDownDir, leftForeArmDir);

    //right
    Vector3 rightArmDir = targets[14].ToVector() - targets[12].ToVector();
    Vector3 rightForeArmDir = targets[16].ToVector() - targets[14].ToVector();
    Vector3 rightArmDownDir = Vector3.Cross(rightArmDir, rightForeArmDir);

    rightArm.rotation = Quaternion.LookRotation(rightArmDownDir, rightArmDir);

    Vector3 rightHandThumbDir = targets[20].ToVector() - targets[18].ToVector();
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
