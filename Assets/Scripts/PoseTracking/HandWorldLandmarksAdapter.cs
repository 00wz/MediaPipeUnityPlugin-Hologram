using Mediapipe;
using Mediapipe.Unity;
using System.Collections.Generic;
using UnityEngine;

public class HandWorldLandmarksAdapter : HierarchicalAnnotation
{
  [SerializeField] private DollPoseRepeater dollPoseRepeater;

  public void HandleHands(List<LandmarkList> targets)
  {
    dollPoseRepeater.HandleHands(targets);
  }
}
