using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe;
using Mediapipe.Unity;

public class HandWorldLandmarksAdapterController : AnnotationController<HandWorldLandmarksAdapter>
{
  private List<LandmarkList> _currentTarget;

  public void DrawNow(List<LandmarkList> target)
  {
    _currentTarget = target;
    SyncNow();
  }

  public void DrawLater(List<LandmarkList> target)
  {
    UpdateCurrentTarget(target, ref _currentTarget);
  }

  protected override void SyncNow()
  {
    isStale = false;
    annotation.HandleHands(_currentTarget);
  }
}
