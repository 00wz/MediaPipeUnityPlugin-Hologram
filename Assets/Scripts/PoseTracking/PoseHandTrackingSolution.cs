using System.Collections;
using UnityEngine;
using Mediapipe.Unity;
using Mediapipe.Unity.PoseTracking;
using Mediapipe;
using System.Collections.Generic;

public class PoseHandTrackingSolution : ImageSourceSolution<PoseHandTrackingGraph>
{
  [SerializeField] private PoseWorldLandmarksAdapterController _poseWorldLandmarksAdapterController;
  [SerializeField] private PoseLandmarkListAnnotationController _poseLandmarksAnnotationController;
  [SerializeField] private MultiHandLandmarkListAnnotationController _handLandmarksAnnotationController;
  [SerializeField] private HandWorldLandmarksAdapterController _handWorldLandmarksAdapterController;

  public PoseHandTrackingGraph.ModelComplexity modelComplexity
  {
    get => graphRunner.modelComplexity;
    set => graphRunner.modelComplexity = value;
  }

  public int maxNumHands
  {
    get => graphRunner.maxNumHands;
    set => graphRunner.maxNumHands = value;
  }

  public bool smoothLandmarks
  {
    get => graphRunner.smoothLandmarks;
    set => graphRunner.smoothLandmarks = value;
  }

  public float minDetectionConfidence
  {
    get => graphRunner.minDetectionConfidence;
    set => graphRunner.minDetectionConfidence = value;
  }

  public float minTrackingConfidence
  {
    get => graphRunner.minTrackingConfidence;
    set => graphRunner.minTrackingConfidence = value;
  }

  protected override void OnStartRun()
  {
    if (!runningMode.IsSynchronous())
    {
      graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
      graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
      graphRunner.OnHandWorldLandmarksOutput += OnHandWorldLandmarksOutput;
      graphRunner.OnHandednessOutput += OnHandednessOutput;
      graphRunner.OnHandLandmarksOutput += OnHandLandmarksOutput;
    }

    var imageSource = ImageSourceProvider.ImageSource;
    SetupAnnotationController(_poseLandmarksAnnotationController, imageSource);
    SetupAnnotationController(_poseWorldLandmarksAdapterController, imageSource);
    SetupAnnotationController(_handLandmarksAnnotationController, imageSource);
    SetupAnnotationController(_handWorldLandmarksAdapterController, imageSource);
  }

  protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
  {
    graphRunner.AddTextureFrameToInputStream(textureFrame);
  }

  protected override IEnumerator WaitForNextValue()
  {
    NormalizedLandmarkList poseLandmarks = null;
    LandmarkList poseWorldLandmarks = null;
    List<LandmarkList> handWorldLandmarks = null;
    List<NormalizedLandmarkList> handLandmarks = null;
    List<ClassificationList> handedness = null;

    if (runningMode == RunningMode.Sync)
    {
      var _ = graphRunner.TryGetNext(out poseWorldLandmarks, out poseLandmarks, out handWorldLandmarks, 
        out handLandmarks, out handedness, true);
    }
    else if (runningMode == RunningMode.NonBlockingSync)
    {
      yield return new WaitUntil(() => 
      graphRunner.TryGetNext(out poseWorldLandmarks, out poseLandmarks, out handWorldLandmarks,
        out handLandmarks, out handedness, false));
    }

    _poseLandmarksAnnotationController.DrawNow(poseLandmarks);
    _poseWorldLandmarksAdapterController.DrawNow(poseWorldLandmarks);
    _handLandmarksAnnotationController.DrawNow(handLandmarks, handedness);
    _handWorldLandmarksAdapterController.DrawNow(handWorldLandmarks, handedness);
  }

  private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs)
  {
    _poseWorldLandmarksAdapterController.DrawLater(eventArgs.value);
  }

  private void OnPoseLandmarksOutput(object sender, OutputEventArgs<NormalizedLandmarkList> eventArgs)
  {
    _poseLandmarksAnnotationController.DrawLater(eventArgs.value);
  }

  private void OnHandWorldLandmarksOutput(object stream, OutputEventArgs<List<LandmarkList>> eventArgs)
  {
    _handWorldLandmarksAdapterController.DrawLater(eventArgs.value);
  }

  private void OnHandLandmarksOutput(object sender, OutputEventArgs<List<NormalizedLandmarkList>> eventArgs)
  {
    _handLandmarksAnnotationController.DrawLater(eventArgs.value);
  }

  private void OnHandednessOutput(object sender, OutputEventArgs<List<ClassificationList>> eventArgs)
  {
    _handLandmarksAnnotationController.DrawLater(eventArgs.value);
    _handWorldLandmarksAdapterController.DrawLater(eventArgs.value);
  }
}
