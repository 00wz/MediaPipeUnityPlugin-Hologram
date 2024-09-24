using Google.Protobuf.Collections;
using Mediapipe;
using Mediapipe.Unity.CoordinateSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollPoseRepeater : MonoBehaviour
{
  [Header("left")]
  [SerializeField] private Transform leftArm;
  [SerializeField] private Transform leftForeArm;
  [SerializeField] private Transform leftHand;
  [Space]
  [SerializeField] private Transform leftHandIndex1;
  [SerializeField] private Transform leftHandIndex2;
  [SerializeField] private Transform leftHandIndex3;
  [Space]
  [SerializeField] private Transform leftHandMiddle1;
  [SerializeField] private Transform leftHandMiddle2;
  [SerializeField] private Transform leftHandMiddle3;
  [Space]
  [SerializeField] private Transform leftHandThumb1;
  [SerializeField] private Transform leftHandThumb2;
  [SerializeField] private Transform leftHandThumb3;

  [Space]
  [Header("right")]
  [SerializeField] private Transform rightArm;
  [SerializeField] private Transform rightForeArm;

  [SerializeField] private bool IsEnable = true;

  private IList<Landmark> _poseLandmarks;
  private RepeatedField<Landmark> _rightHand;
  private RepeatedField<Landmark> _leftHand;

  public void HandlePose(IList<Landmark> targets)
  {
    _poseLandmarks = targets;
    //RepeatHandsPositions();
  }

  public void HandleHands(List<LandmarkList> targets, IList<ClassificationList> handedness)
  {
    _leftHand = null;
    _rightHand = null;

    if(targets == null || handedness == null)
    {
      return;
    }

    for(int i = 0; i < targets.Count && i < handedness.Count; i++)
    {
      if (handedness[i].Classification[0].Label == "Left")
      {
        _rightHand = targets[i].Landmark;
      }
      else
      {
        _leftHand = targets[i].Landmark;
      }
    }
    /*
    if(_leftHand != null)
    {
      DebugHand(_leftHand, UnityEngine.Color.blue);
    }

    if(_rightHand != null)
    {
      DebugHand(_rightHand, UnityEngine.Color.green);
    }
    */
  }

  private void DebugHand(RepeatedField<Landmark> handLandmarks, UnityEngine.Color debugColor)
  {
    Debug.DrawLine(handLandmarks[0].ToVector(), handLandmarks[1].ToVector(), debugColor);
    Debug.DrawLine(handLandmarks[1].ToVector(), handLandmarks[2].ToVector(), debugColor);
    Debug.DrawLine(handLandmarks[2].ToVector(), handLandmarks[3].ToVector(), debugColor);
    Debug.DrawLine(handLandmarks[3].ToVector(), handLandmarks[4].ToVector(), debugColor);

    Debug.DrawLine(handLandmarks[0].ToVector(), handLandmarks[5].ToVector(), debugColor);
    Debug.DrawLine(handLandmarks[0].ToVector(), handLandmarks[17].ToVector(), debugColor);
    Debug.DrawLine(handLandmarks[5].ToVector(), handLandmarks[9].ToVector(), debugColor);
    Debug.DrawLine(handLandmarks[9].ToVector(), handLandmarks[13].ToVector(), debugColor);
    Debug.DrawLine(handLandmarks[13].ToVector(), handLandmarks[17].ToVector(), debugColor);

    Debug.DrawLine(handLandmarks[5].ToVector(), handLandmarks[6].ToVector(), debugColor);
    Debug.DrawLine(handLandmarks[6].ToVector(), handLandmarks[7].ToVector(), debugColor);
    Debug.DrawLine(handLandmarks[7].ToVector(), handLandmarks[8].ToVector(), debugColor);
  }

  private void RepeatHandsPositions()
  {
    if (!IsEnable)
    {
      return;
    }
    
    //left
    if(_poseLandmarks != null)
    {
      Vector3 leftArmDir = _poseLandmarks[13].ToVector() - _poseLandmarks[11].ToVector();
      Vector3 leftForeArmDir = _poseLandmarks[15].ToVector() - _poseLandmarks[13].ToVector();
      Vector3 leftArmDownDir = Vector3.Cross(leftForeArmDir, leftArmDir);

      leftArm.rotation = Quaternion.LookRotation(leftArmDownDir, leftArmDir);

      Vector3 leftHandThumbDir;// = _poseLandmarks[19].ToVector() - _poseLandmarks[17].ToVector();
      if(_leftHand != null)
      {
        leftHandThumbDir = _leftHand[5].ToVector() - _leftHand[17].ToVector();
      }
      else
      {
        leftHandThumbDir = leftForeArm.right;
      }
      Vector3 leftForeArmDownDir = Vector3.Cross(leftHandThumbDir, leftForeArmDir);

      leftForeArm.rotation = Quaternion.LookRotation(leftForeArmDownDir, leftForeArmDir);
    }

    if(_leftHand != null)
    {
      Vector3 leftHandThumbDir = _leftHand[5].ToVector() - _leftHand[17].ToVector();
      Vector3 leftHandForwardDir = _leftHand[9].ToVector() - _leftHand[0].ToVector();
      leftHand.rotation = Utils.UpRightLookRotation(leftHandForwardDir, leftHandThumbDir);

      RepeatFingerPosition(leftHandIndex1, leftHandIndex2, leftHandIndex3, 8, _leftHand);
      RepeatFingerPosition(leftHandMiddle1, leftHandMiddle2, leftHandMiddle3, 12, _leftHand);
      RepeatThumbPosition(leftHandThumb1, leftHandThumb2, leftHandThumb3, _leftHand);
    }
    


    //right
    if(_poseLandmarks != null)
    {
      Vector3 rightArmDir = _poseLandmarks[14].ToVector() - _poseLandmarks[12].ToVector();
      Vector3 rightForeArmDir = _poseLandmarks[16].ToVector() - _poseLandmarks[14].ToVector();
      Vector3 rightArmDownDir = Vector3.Cross(rightArmDir, rightForeArmDir);

      //rightArm.rotation = Quaternion.LookRotation(rightArmDownDir, rightArmDir);
      rightArm.rotation = Utils.UpForwardLookRotation(rightArmDir, rightArmDownDir);

      Vector3 rightHandThumbDir = _poseLandmarks[20].ToVector() - _poseLandmarks[18].ToVector();
      Vector3 rightForeArmDownDir = Vector3.Cross(rightForeArmDir, rightHandThumbDir);

      //rightForeArm.rotation = Quaternion.LookRotation(rightForeArmDownDir, rightForeArmDir);
      rightForeArm.rotation = Utils.UpForwardLookRotation(rightForeArmDir, rightForeArmDownDir);
    }
  }

  private void RepeatFingerPosition(Transform t1, Transform t2,
    Transform t3, int lastLandmarkNum, RepeatedField<Landmark> hand)
  {
    Vector3 third = hand[lastLandmarkNum--].ToVector() - hand[lastLandmarkNum].ToVector();
    Vector3 second = hand[lastLandmarkNum--].ToVector() - hand[lastLandmarkNum].ToVector();
    Vector3 first = hand[lastLandmarkNum--].ToVector() - hand[lastLandmarkNum].ToVector();
    Vector3 jointAxis = Vector3.Cross(first, third);

    t1.rotation = Utils.UpRightLookRotation(first, jointAxis);
    t2.rotation = Utils.UpRightLookRotation(second, jointAxis);
    t3.rotation = Utils.UpRightLookRotation(third, jointAxis);
  }

  private void RepeatThumbPosition(Transform t1, Transform t2,
    Transform t3, RepeatedField<Landmark> hand, bool isLeft = true)
  {
    int lastLandmarkNum = 4;
    Vector3 third = hand[lastLandmarkNum--].ToVector() - hand[lastLandmarkNum].ToVector();
    Vector3 second = hand[lastLandmarkNum--].ToVector() - hand[lastLandmarkNum].ToVector();
    Vector3 first = hand[lastLandmarkNum--].ToVector() - hand[lastLandmarkNum].ToVector();
    Vector3 jointAxis = Vector3.Cross(third,
      hand[5].ToVector() - hand[17].ToVector()) * 
      (isLeft? -1f : 1f);

    t1.rotation = Utils.UpForwardLookRotation(first, jointAxis);
    t2.rotation = Utils.UpForwardLookRotation(second, jointAxis);
    t3.rotation = Utils.UpForwardLookRotation(third, jointAxis);
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
