// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mediapipe.Unity;
using Mediapipe;

using Google.Protobuf;

public class PoseHandTrackingGraph : GraphRunner
{
  public enum ModelComplexity
  {
    Lite = 0,
    Full = 1,
    //Heavy = 2,
  }

  public ModelComplexity modelComplexity = ModelComplexity.Full;
  public int maxNumHands = 2;
  public bool smoothLandmarks = true;

  public bool enableSegmentation = false;
  public bool smoothSegmentation = true;

  private float _minDetectionConfidence = 0.5f;
  public float minDetectionConfidence
  {
    get => _minDetectionConfidence;
    set => _minDetectionConfidence = Mathf.Clamp01(value);
  }

  private float _minTrackingConfidence = 0.5f;
  public float minTrackingConfidence
  {
    get => _minTrackingConfidence;
    set => _minTrackingConfidence = Mathf.Clamp01(value);
  }

  public event EventHandler<OutputEventArgs<LandmarkList>> OnPoseWorldLandmarksOutput
  {
    add => _poseWorldLandmarksStream.AddListener(value);
    remove => _poseWorldLandmarksStream.RemoveListener(value);
  }

  public event EventHandler<OutputEventArgs<List<LandmarkList>>> OnHandWorldLandmarksOutput
  {
    add => _handWorldLandmarksStream.AddListener(value);
    remove => _handWorldLandmarksStream.RemoveListener(value);
  }

  private const string _InputStreamName = "input_video";
  //private const string _PoseDetectionStreamName = "pose_detection";
  //private const string _PoseLandmarksStreamName = "pose_landmarks";
  private const string _PoseWorldLandmarksStreamName = "pose_world_landmarks";
  //private const string _SegmentationMaskStreamName = "segmentation_mask";
  //private const string _RoiFromLandmarksStreamName = "roi_from_landmarks";
  private const string _HandWorldLandmarksStreamName = "hand_world_landmarks";

  //private OutputStream<DetectionPacket, Detection> _poseDetectionStream;
  //private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _poseLandmarksStream;
  private OutputStream<LandmarkListPacket, LandmarkList> _poseWorldLandmarksStream;
  //private OutputStream<ImageFramePacket, ImageFrame> _segmentationMaskStream;
  //private OutputStream<NormalizedRectPacket, NormalizedRect> _roiFromLandmarksStream;
  private OutputStream<LandmarkListVectorPacket, List<LandmarkList>> _handWorldLandmarksStream;

  public override void StartRun(ImageSource imageSource)
  {
    if (runningMode.IsSynchronous())
    {
      //_poseDetectionStream.StartPolling().AssertOk();
      //_poseLandmarksStream.StartPolling().AssertOk();
      _poseWorldLandmarksStream.StartPolling().AssertOk();
      //_segmentationMaskStream.StartPolling().AssertOk();
      //_roiFromLandmarksStream.StartPolling().AssertOk();
      _handWorldLandmarksStream.StartPolling().AssertOk();
    }
    StartRun(BuildSidePacket(imageSource));
  }

  public override void Stop()
  {
    //_poseDetectionStream?.Close();
    //_poseDetectionStream = null;
    //_poseLandmarksStream?.Close();
    //_poseLandmarksStream = null;
    _poseWorldLandmarksStream?.Close();
    _poseWorldLandmarksStream = null;
    //_segmentationMaskStream?.Close();
    //_segmentationMaskStream = null;
    //_roiFromLandmarksStream?.Close();
    //_roiFromLandmarksStream = null;
    _handWorldLandmarksStream?.Close();
    _handWorldLandmarksStream = null;
    base.Stop();
  }

  public void AddTextureFrameToInputStream(TextureFrame textureFrame)
  {
    AddTextureFrameToInputStream(_InputStreamName, textureFrame);
  }

  public bool TryGetNext(out LandmarkList poseWorldLandmarks,
                           out List<LandmarkList> handWorldLandmarks, bool allowBlock = true)
  {
    var currentTimestampMicrosec = GetCurrentTimestampMicrosec();
    //var r1 = TryGetNext(_poseDetectionStream, out poseDetection, allowBlock, currentTimestampMicrosec);
    //var r2 = TryGetNext(_poseLandmarksStream, out poseLandmarks, allowBlock, currentTimestampMicrosec);
    var r3 = TryGetNext(_poseWorldLandmarksStream, out poseWorldLandmarks, allowBlock, currentTimestampMicrosec);
    //var r4 = TryGetNext(_segmentationMaskStream, out segmentationMask, allowBlock, currentTimestampMicrosec);
    //var r5 = TryGetNext(_roiFromLandmarksStream, out roiFromLandmarks, allowBlock, currentTimestampMicrosec);
    var r6 = TryGetNext(_handWorldLandmarksStream, out handWorldLandmarks, allowBlock, currentTimestampMicrosec);

    return r3 || r6;
  }

  protected override IList<WaitForResult> RequestDependentAssets()
  {
    return new List<WaitForResult> {
      WaitForAsset("pose_detection.bytes"),
      WaitForPoseLandmarkModel(),
      WaitForHandLandmarkModel(),
      WaitForAsset("handedness.txt"),
      WaitForPalmDetectionModel(),
    };
  }

  protected override Status ConfigureCalculatorGraph(CalculatorGraphConfig config)
  {
    if (runningMode == RunningMode.NonBlockingSync)
    {
      //_poseDetectionStream = new OutputStream<DetectionPacket, Detection>(
      //    calculatorGraph, _PoseDetectionStreamName, config.AddPacketPresenceCalculator(_PoseDetectionStreamName), timeoutMicrosec);
      //_poseLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(
      //    calculatorGraph, _PoseLandmarksStreamName, config.AddPacketPresenceCalculator(_PoseLandmarksStreamName), timeoutMicrosec);
      _poseWorldLandmarksStream = new OutputStream<LandmarkListPacket, LandmarkList>(
          calculatorGraph, _PoseWorldLandmarksStreamName, config.AddPacketPresenceCalculator(_PoseWorldLandmarksStreamName), timeoutMicrosec);
      //_segmentationMaskStream = new OutputStream<ImageFramePacket, ImageFrame>(
      //    calculatorGraph, _SegmentationMaskStreamName, config.AddPacketPresenceCalculator(_SegmentationMaskStreamName), timeoutMicrosec);
      //_roiFromLandmarksStream = new OutputStream<NormalizedRectPacket, NormalizedRect>(
      //    calculatorGraph, _RoiFromLandmarksStreamName, config.AddPacketPresenceCalculator(_RoiFromLandmarksStreamName), timeoutMicrosec);
      _handWorldLandmarksStream = new OutputStream<LandmarkListVectorPacket, List<LandmarkList>>(
        calculatorGraph, _HandWorldLandmarksStreamName, config.AddPacketPresenceCalculator(_HandWorldLandmarksStreamName), timeoutMicrosec);
    }
    else
    {
      //_poseDetectionStream = new OutputStream<DetectionPacket, Detection>(calculatorGraph, _PoseDetectionStreamName, true, timeoutMicrosec);
      //_poseLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(calculatorGraph, _PoseLandmarksStreamName, true, timeoutMicrosec);
      _poseWorldLandmarksStream = new OutputStream<LandmarkListPacket, LandmarkList>(calculatorGraph, _PoseWorldLandmarksStreamName, true, timeoutMicrosec);
      //_segmentationMaskStream = new OutputStream<ImageFramePacket, ImageFrame>(calculatorGraph, _SegmentationMaskStreamName, true, timeoutMicrosec);
      //_roiFromLandmarksStream = new OutputStream<NormalizedRectPacket, NormalizedRect>(calculatorGraph, _RoiFromLandmarksStreamName, true, timeoutMicrosec);
      _handWorldLandmarksStream = new OutputStream<LandmarkListVectorPacket, List<LandmarkList>>(calculatorGraph, _HandWorldLandmarksStreamName, true, timeoutMicrosec);
    }

    using (var validatedGraphConfig = new ValidatedGraphConfig())
    {
      var status = validatedGraphConfig.Initialize(config);

      if (!status.Ok()) { return status; }

      var extensionRegistry = new ExtensionRegistry() { TensorsToDetectionsCalculatorOptions.Extensions.Ext, ThresholdingCalculatorOptions.Extensions.Ext };
      var cannonicalizedConfig = validatedGraphConfig.Config(extensionRegistry);
      var tensorsToDetectionsCalculators = cannonicalizedConfig.Node.Where((node) => node.Calculator == "TensorsToDetectionsCalculator").ToList();
      var thresholdingCalculators = cannonicalizedConfig.Node.Where((node) => node.Calculator == "ThresholdingCalculator").ToList();

      foreach (var calculator in tensorsToDetectionsCalculators)
      {
        if (calculator.Options.HasExtension(TensorsToDetectionsCalculatorOptions.Extensions.Ext))
        {
          var options = calculator.Options.GetExtension(TensorsToDetectionsCalculatorOptions.Extensions.Ext);
          options.MinScoreThresh = minDetectionConfidence;
        Mediapipe.Unity.Logger.LogInfo(TAG, $"Min Detection Confidence = {minDetectionConfidence}");
        }
      }

      foreach (var calculator in thresholdingCalculators)
      {
        if (calculator.Options.HasExtension(ThresholdingCalculatorOptions.Extensions.Ext))
        {
          var options = calculator.Options.GetExtension(ThresholdingCalculatorOptions.Extensions.Ext);
          options.Threshold = minTrackingConfidence;
        Mediapipe.Unity.Logger.LogInfo(TAG, $"Min Tracking Confidence = {minTrackingConfidence}");
        }
      }
      return calculatorGraph.Initialize(cannonicalizedConfig);
    }
  }

  private WaitForResult WaitForPoseLandmarkModel()
  {
    switch (modelComplexity)
    {
      case ModelComplexity.Lite: return WaitForAsset("pose_landmark_lite.bytes");
      case ModelComplexity.Full: return WaitForAsset("pose_landmark_full.bytes");
      //case ModelComplexity.Heavy: return WaitForAsset("pose_landmark_heavy.bytes");
      default: throw new InternalException($"Invalid model complexity: {modelComplexity}");
    }
  }

  private WaitForResult WaitForHandLandmarkModel()
  {
    switch (modelComplexity)
    {
      case ModelComplexity.Lite: return WaitForAsset("hand_landmark_lite.bytes");
      case ModelComplexity.Full: return WaitForAsset("hand_landmark_full.bytes");
      default: throw new InternalException($"Invalid model complexity: {modelComplexity}");
    }
  }

  private WaitForResult WaitForPalmDetectionModel()
  {
    switch (modelComplexity)
    {
      case ModelComplexity.Lite: return WaitForAsset("palm_detection_lite.bytes");
      case ModelComplexity.Full: return WaitForAsset("palm_detection_full.bytes");
      default: throw new InternalException($"Invalid model complexity: {modelComplexity}");
    }
  }

  private SidePacket BuildSidePacket(ImageSource imageSource)
  {
    var sidePacket = new SidePacket();

    SetImageTransformationOptions(sidePacket, imageSource);
    SetInputHorizontallyFlippedInvers(sidePacket, imageSource, true);

    sidePacket.Emplace("model_complexity", new IntPacket((int)modelComplexity));
    sidePacket.Emplace("smooth_landmarks", new BoolPacket(smoothLandmarks));
    sidePacket.Emplace("enable_segmentation", new BoolPacket(enableSegmentation));
    sidePacket.Emplace("smooth_segmentation", new BoolPacket(smoothSegmentation));
    sidePacket.Emplace("num_hands", new IntPacket(maxNumHands));

    Mediapipe.Unity.Logger.LogInfo(TAG, $"Model Complexity = {modelComplexity}");
    Mediapipe.Unity.Logger.LogInfo(TAG, $"Smooth Landmarks = {smoothLandmarks}");
    Mediapipe.Unity.Logger.LogInfo(TAG, $"Enable Segmentation = {enableSegmentation}");
    Mediapipe.Unity.Logger.LogInfo(TAG, $"Smooth Segmentation = {smoothSegmentation}");
    Mediapipe.Unity.Logger.LogInfo(TAG, $"Max Num Hands = {maxNumHands}");

    return sidePacket;
  }

  private void SetInputHorizontallyFlippedInvers(SidePacket sidePacket, ImageSource imageSource, bool expectedToBeMirrored = false)
  {
    var isInverted = Mediapipe.Unity.CoordinateSystem.ImageCoordinate.IsInverted(rotation);
    var shouldBeMirrored = imageSource.isHorizontallyFlipped ^ expectedToBeMirrored;
    var inputHorizontallyFlipped = isInverted ^ shouldBeMirrored;
    var inputVerticallyFlipped = !isInverted;

    if ((inputHorizontallyFlipped && inputVerticallyFlipped) || rotation == RotationAngle.Rotation180)
    {
      inputHorizontallyFlipped = !inputHorizontallyFlipped;
    }

    sidePacket.Emplace("input_horizontally_flipped_invers", new BoolPacket(inputHorizontallyFlipped));
  }
}
