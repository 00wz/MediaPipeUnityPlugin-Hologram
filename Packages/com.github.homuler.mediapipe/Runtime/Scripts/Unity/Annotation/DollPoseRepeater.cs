using Mediapipe;
using Mediapipe.Unity.CoordinateSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollPoseRepeater : MonoBehaviour
{
  [SerializeField] private Transform leftArm;

  public void HandlePose(IList<Landmark> targets)
  {
    if (targets == null)
    {
      return;
    }
    Vector3 leftArmDir = targets[13].ToVector() - targets[11].ToVector();
    Vector3 leftForeArmDir = targets[15].ToVector() - targets[13].ToVector();
    Vector3 leftArmDownDir = Vector3.Cross(leftForeArmDir, leftArmDir);
    /*
    Debug.DrawLine(leftArm.position, leftArm.position + leftArmDir * 999f);
    Debug.DrawLine(leftArm.position, leftArm.position + leftForeArmDir * 999f, UnityEngine.Color.red);
    Debug.DrawLine(leftArm.position, leftArm.position + leftArmDownDir * 999f, UnityEngine.Color.blue);
    */
    leftArm.transform.rotation = Quaternion.LookRotation(leftArmDownDir, leftArmDir);
    /*
    Debug.DrawLine(targets[12].ToVector(), targets[11].ToVector());
    Debug.DrawLine(targets[11].ToVector(), targets[23].ToVector());
    Debug.DrawLine(targets[11].ToVector(), targets[13].ToVector());
    Debug.DrawLine(Vector3.zero, targets[13].ToVector() - targets[11].ToVector(), UnityEngine.Color.red);
    Debug.DrawLine(Vector3.zero, leftArmDir, UnityEngine.Color.blue);
    Debug.DrawLine(targets[12].ToVector(), targets[24].ToVector());
    Debug.DrawLine(targets[24].ToVector(), targets[23].ToVector());
    Debug.DrawLine(targets[13].ToVector(), targets[15].ToVector());
    Debug.DrawLine(targets[12].ToVector(), targets[11].ToVector());
    */
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
