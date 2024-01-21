using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HologramPipe
{
  public class Debugger : MonoBehaviour
  {
    [SerializeField]
    private IrisTracker IrisTracker;

    /*
    private void Update()
    {
      if(IrisTracker.IrisPositions!=null)
      {
        Debug.DrawLine(transform.position, transform.position +
          transform.TransformVector(IrisTracker.IrisPositions.Value.LeftIrisPositionMM),
          new UnityEngine.Color(0f, 0f, 1.0f), 1.0f);
        Debug.DrawLine(transform.position, transform.position +
          transform.TransformVector(IrisTracker.IrisPositions.Value.RightIrisPositionMM),
          new UnityEngine.Color(0f, 1.0f, 0f), 1.0f);
      }
    }
    */

    private void OnDrawGizmos()
    {
      if (IrisTracker.IrisPositions != null)
      {
        Gizmos.color = new UnityEngine.Color(0f, 0f, 1.0f);
        Gizmos.DrawLine(transform.position, transform.position +
          transform.TransformVector(IrisTracker.IrisPositions.Value.LeftIrisPositionMM));

        Gizmos.color = new UnityEngine.Color(0f, 1.0f, 0f);
        Gizmos.DrawLine(transform.position, transform.position +
          transform.TransformVector(IrisTracker.IrisPositions.Value.RightIrisPositionMM));
      }
    }
  }
}

