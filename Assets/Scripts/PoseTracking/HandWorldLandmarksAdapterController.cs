using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe;
using Mediapipe.Unity;
using System;

public class HandWorldLandmarksAdapterController : AnnotationController<HandWorldLandmarksAdapter>
{
  private List<LandmarkList> _currentHandWorldLandmarks;
  private IList<ClassificationList> _currentHandedness;

  public void DrawNow(List<LandmarkList> target, List<ClassificationList> handedness)
  {
    _currentHandWorldLandmarks = target;
    _currentHandedness = handedness;
    SyncNow();
  }

  public void DrawLater(List<LandmarkList> target)
  {
    UpdateCurrentTarget(target, ref _currentHandWorldLandmarks);
  }

  internal void DrawLater(List<ClassificationList> handedness)
  {
    UpdateCurrentTarget(handedness, ref _currentHandedness);
  }

  protected override void SyncNow()
  {
    isStale = false;
    annotation.HandleHands(_currentHandWorldLandmarks, _currentHandedness);
  }
}
