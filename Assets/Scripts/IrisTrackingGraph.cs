using Mediapipe;
using Mediapipe.Unity;
using System;
using System.Collections.Generic;

namespace Hologrampipe
{
  public class IrisTrackingGraph : GraphRunner
  {
    public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnFaceLandmarksWithIrisOutput
    {
      add => _faceLandmarksWithIrisStream.AddListener(value);
      remove => _faceLandmarksWithIrisStream.RemoveListener(value);
    }

    private const string _InputStreamName = "input_video";

    private const string _FaceLandmarksWithIrisStreamName = "face_landmarks_with_iris";

    private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _faceLandmarksWithIrisStream;

    public override void StartRun(ImageSource imageSource)
    {
      if (runningMode.IsSynchronous())
      {
        _faceLandmarksWithIrisStream.StartPolling().AssertOk();
      }
      StartRun(BuildSidePacket(imageSource));
    }

    public override void Stop()
    {
      _faceLandmarksWithIrisStream?.Close();
      _faceLandmarksWithIrisStream = null;
      base.Stop();
    }

    public void AddTextureFrameToInputStream(TextureFrame textureFrame)
    {
      AddTextureFrameToInputStream(_InputStreamName, textureFrame);
    }

    public bool TryGetNext(out NormalizedLandmarkList faceLandmarksWithIris, bool allowBlock = true)
    {
      var currentTimestampMicrosec = GetCurrentTimestampMicrosec();
      return TryGetNext(_faceLandmarksWithIrisStream, out faceLandmarksWithIris, allowBlock, currentTimestampMicrosec);
    }

    protected override IList<WaitForResult> RequestDependentAssets()
    {
      return new List<WaitForResult> {
        WaitForAsset("face_detection_short_range.bytes"),
        WaitForAsset("face_landmark.bytes"),
        WaitForAsset("iris_landmark.bytes"),
      };
    }

    protected override Status ConfigureCalculatorGraph(CalculatorGraphConfig config)
    {
      if (runningMode == RunningMode.NonBlockingSync)
      {
        _faceLandmarksWithIrisStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(
            calculatorGraph, _FaceLandmarksWithIrisStreamName, config.AddPacketPresenceCalculator(_FaceLandmarksWithIrisStreamName), timeoutMicrosec);
      }
      else
      {
        _faceLandmarksWithIrisStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(calculatorGraph, _FaceLandmarksWithIrisStreamName, true, timeoutMicrosec);
      }
      return calculatorGraph.Initialize(config);
    }

    private SidePacket BuildSidePacket(ImageSource imageSource)
    {
      var sidePacket = new SidePacket();
      SetImageTransformationOptions(sidePacket, imageSource);
      return sidePacket;
    }
  }
}
