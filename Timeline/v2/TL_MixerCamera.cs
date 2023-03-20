using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;

public class TL_MixerCamera : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.ProcessFrame(playable, info, playerData);

        var kPathRoot = playerData as GameObject;
        if (kPathRoot == null) return;

        var inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            if (playable.GetInputWeight(i) <= 0) continue;

            var uPlayableType = playable.GetInput(i).GetPlayableType();
            if (uPlayableType == typeof(TL_BehaviourCamera))
            {
                var kCameraPlayable = playable.GetInput(i);
                var kCameraBehaviour = ((ScriptPlayable<TL_BehaviourCamera>)kCameraPlayable).GetBehaviour();

                if (kCameraBehaviour != null)
                {
                    double fKey = (kCameraBehaviour.m_fEnd - kCameraBehaviour.m_fStart)
                        * kCameraPlayable.GetTime() / kCameraPlayable.GetDuration() + kCameraBehaviour.m_fStart;

                    var kCamera = TL_Utility.FindTimelineCamera();
                    var kPath = TL_Utility.FindChildThroughPath(kPathRoot, kCameraBehaviour.m_kInsidePath)?.GetComponent<CinemachinePathBase>();

                    if (kCamera != null && kPath != null)
                    {
                        // 控制位移
                        kCamera.transform.position = kPath.EvaluatePosition((float)fKey);

                        // 控制朝向
                        if (kCameraBehaviour.m_uCameraRotation == TL_ClipCamera.CameraRotationType.Lookat)
                        {
                            if (kCamera.GetCinemachineComponent<CinemachineHardLookAt>() == null)
                                kCamera.AddCinemachineComponent<CinemachineHardLookAt>();

                            var kLootat = TL_Utility.FindChildThroughPath(kPathRoot, kCameraBehaviour.m_kTargetPath);
                            if (kLootat != null)
                            {
                                kCamera.LookAt = kLootat.transform;
                            }
                        }
                        else
                        {
                            kCamera.LookAt = null;

                            if (kCameraBehaviour.m_uCameraRotation == TL_ClipCamera.CameraRotationType.Cinemachine)
                            {
                                kCamera.transform.rotation = kPath.EvaluateOrientation((float)fKey);
                            }
                            else if (kCameraBehaviour.m_uCameraRotation == TL_ClipCamera.CameraRotationType.Manual)
                            {
                                kCamera.transform.rotation = Quaternion.identity;
                            }
                        }
                    }
                }
            }
        }
    }
}
