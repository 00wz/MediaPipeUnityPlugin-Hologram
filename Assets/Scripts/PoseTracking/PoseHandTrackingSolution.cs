using System.Collections;
using UnityEngine;
using Mediapipe.Unity;
using Mediapipe.Unity.PoseTracking;
using Mediapipe;
using System.Collections.Generic;

public class PoseHandTrackingSolution : ImageSourceSolution<PoseHandTrackingGraph>
{
  //[SerializeField] private RectTransform _worldAnnotationArea;
  //[SerializeField] private DetectionAnnotationController _poseDetectionAnnotationController;
  //[SerializeField] private PoseLandmarkListAnnotationController _poseLandmarksAnnotationController;
  [SerializeField] private PoseWorldLandmarkListAnnotationController _poseWorldLandmarksAnnotationController;
  //[SerializeField] private MaskAnnotationController _segmentationMaskAnnotationController;
  //[SerializeField] private NormalizedRectAnnotationController _roiFromLandmarksAnnotationController;
  [SerializeField] private PoseLandmarkListAnnotationController _poseLandmarksAnnotationController;
  [SerializeField] private MultiHandLandmarkListAnnotationController _handLandmarksAnnotationController;

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
/*
  public bool enableSegmentation
  {
    get => graphRunner.enableSegmentation;
    set => graphRunner.enableSegmentation = value;
  }

  public bool smoothSegmentation
  {
    get => graphRunner.smoothSegmentation;
    set => graphRunner.smoothSegmentation = value;
  }
*/
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
  /*
  protected override void SetupScreen(ImageSource imageSource)
  {
    base.SetupScreen(imageSource);
    _worldAnnotationArea.localEulerAngles = imageSource.rotation.Reverse().GetEulerAngles();
  }
  */
  protected override void OnStartRun()
  {
    if (!runningMode.IsSynchronous())
    {
      //graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
      graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
      graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
      //graphRunner.OnSegmentationMaskOutput += OnSegmentationMaskOutput;
      //graphRunner.OnRoiFromLandmarksOutput += OnRoiFromLandmarksOutput;
      graphRunner.OnHandWorldLandmarksOutput += OnHandWorldLandmarksOutput;
      graphRunner.OnHandednessOutput += OnHandednessOutput;
      graphRunner.OnHandLandmarksOutput += OnHandLandmarksOutput;
    }

    var imageSource = ImageSourceProvider.ImageSource;
    //SetupAnnotationController(_poseDetectionAnnotationController, imageSource);
    SetupAnnotationController(_poseLandmarksAnnotationController, imageSource);
    SetupAnnotationController(_poseWorldLandmarksAnnotationController, imageSource);
    //SetupAnnotationController(_segmentationMaskAnnotationController, imageSource);
    //_segmentationMaskAnnotationController.InitScreen(imageSource.textureWidth, imageSource.textureHeight);
    //SetupAnnotationController(_roiFromLandmarksAnnotationController, imageSource);
    SetupAnnotationController(_handLandmarksAnnotationController, imageSource, true);
  }

  protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
  {
    graphRunner.AddTextureFrameToInputStream(textureFrame);
  }

  protected override IEnumerator WaitForNextValue()
  {
    //Detection poseDetection = null;
    NormalizedLandmarkList poseLandmarks = null;
    LandmarkList poseWorldLandmarks = null;
    //ImageFrame segmentationMask = null;
    //NormalizedRect roiFromLandmarks = null;
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

    //_poseDetectionAnnotationController.DrawNow(poseDetection);
    _poseLandmarksAnnotationController.DrawNow(poseLandmarks);
    _poseWorldLandmarksAnnotationController.DrawNow(poseWorldLandmarks);
    //_segmentationMaskAnnotationController.DrawNow(segmentationMask);
    //_roiFromLandmarksAnnotationController.DrawNow(roiFromLandmarks);
    _handLandmarksAnnotationController.DrawNow(handLandmarks, handedness);
  }

  private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs)
  {
    _poseWorldLandmarksAnnotationController.DrawLater(eventArgs.value);
  }

  private void OnPoseLandmarksOutput(object sender, OutputEventArgs<NormalizedLandmarkList> eventArgs)
  {
    _poseLandmarksAnnotationController.DrawLater(eventArgs.value);
  }

  private void OnHandWorldLandmarksOutput(object stream, OutputEventArgs<List<LandmarkList>> eventArgs)
  {

  }

  private void OnHandLandmarksOutput(object sender, OutputEventArgs<List<NormalizedLandmarkList>> eventArgs)
  {
    _handLandmarksAnnotationController.DrawLater(eventArgs.value);
  }

  private void OnHandednessOutput(object sender, OutputEventArgs<List<ClassificationList>> eventArgs)
  {
    _handLandmarksAnnotationController.DrawLater(eventArgs.value);
  }
}
