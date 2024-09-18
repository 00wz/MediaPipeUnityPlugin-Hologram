using Mediapipe;
using Mediapipe.Unity;
using System.Collections.Generic;
using UnityEngine;

public class PoseWorldLandmarksAdapter : HierarchicalAnnotation
{
  [SerializeField] private DollPoseRepeater dollPoseRepeater;

  public void HandlePose(IList<Landmark> targets)
  {
    dollPoseRepeater.HandlePose(targets);
  }
}
