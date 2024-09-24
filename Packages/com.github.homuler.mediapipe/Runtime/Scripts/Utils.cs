using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
  static readonly Quaternion YZ_TO_ZY_ROTATE =
    Quaternion.Inverse(Quaternion.LookRotation(Vector3.up, Vector3.forward));
  public static Quaternion UpForwardLookRotation(Vector3 upward, Vector3 forwards)
  {
    return Quaternion.LookRotation(upward, forwards) * YZ_TO_ZY_ROTATE;
  }
  
  static readonly Quaternion XZ_TO_ZY_ROTATE =
    Quaternion.Inverse(Quaternion.LookRotation(Vector3.right, Vector3.forward));
  public static Quaternion RightForwardLookRotation(Vector3 right, Vector3 forwards)
  {
    return Quaternion.LookRotation(right, forwards) * XZ_TO_ZY_ROTATE;
  }
  
  static readonly Quaternion YX_TO_ZY_ROTATE =
    Quaternion.Inverse(Quaternion.LookRotation(Vector3.up, Vector3.right));
  public static Quaternion UpRightLookRotation(Vector3 upward, Vector3 right)
  {
    return Quaternion.LookRotation(upward, right) * YX_TO_ZY_ROTATE;
  }

  public static Quaternion OmniLookRotation(
               Vector3 exactAxis, Vector3 exactTarget,
               Vector3 approximateAxis, Vector3 approximateTarget
  )
  {
    // Compute a rotation that takes the z+ and y+ axes to our custom axes.
    var zyToCustom = Quaternion.LookRotation(exactAxis, approximateAxis);
    // Invert this, to map our custom axes to z+ and y+.
    var customToZY = Quaternion.Inverse(zyToCustom);

    // Compute a rotation that takes the z+ and y+ axes to our target directions.
    var zyToTarget = Quaternion.LookRotation(exactTarget, approximateTarget);

    // Chain these two rotations so that exactAxis maps to exactTarget,
    // and approximateAxis maps as closely as it can to approximateTarget.
    var customToTarget = zyToTarget * customToZY;

    return customToTarget;
  }
}
