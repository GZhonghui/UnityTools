using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TL_BehaviourCamera : PlayableBehaviour
{
    public double m_fStart;
    public double m_fEnd;
    public TL_ClipCamera.CameraRotationType m_uCameraRotation = TL_ClipCamera.CameraRotationType.Cinemachine;
    public List<string> m_kInsidePath;
    public List<string> m_kTargetPath;
}
