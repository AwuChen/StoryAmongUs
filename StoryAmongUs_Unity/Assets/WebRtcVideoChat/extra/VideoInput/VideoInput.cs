using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Byn.Unity.Examples
{
    /// <summary>
    /// Experimental only!
    /// Attempt to create a cross-platform interface for video input.
    /// 
    /// Note that no matter how many instances you create of VideoInput they all share a single static backend. A device added
    /// via one will appear in all others as well.
    /// </summary>
    public class VideoInput
    {
        /// <summary>
        /// There are platform specific formats:
        /// Unity's ARGB32 corresponds to native WebRTC's BGRA which is the only format supported in Unity and WebRTC.
        /// WebRTC might support other formats though. The fastest will be I420p but these need to be handled manually in Unity.
        /// (use NativeVideoInput directly in this case)
        /// 
        /// Unity's RGBA32 is the only format that is supported in the browser's HTMLCanvasElement no others will work
        /// </summary>
        public TextureFormat Format
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return TextureFormat.RGBA32;
#else
                return TextureFormat.ARGB32;
#endif
            }
        }
#if UNITY_WEBGL && !UNITY_EDITOR
#else
        private Awrtc.Native.NativeVideoInput mInternal;
#endif
        public VideoInput()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
#else
            mInternal = Awrtc.Unity.UnityCallFactory.Instance.VideoInput;
#endif
        }

        /// <summary>
        /// Adds a new device. It will be treated by other parts of the asset as a camera device that
        /// has one one supported width, height and FPS you set via this method. 
        /// 
        /// Note that actual frame updates do not need to be done in the exact FPS but it is recommended to do so.
        /// </summary>
        /// <param name="name">name of the device. Can be used via MediaConfig.VideoDeviceName and will be returned by UnityCallFactory.GetVideoDevices()</param>
        /// <param name="width">Width of the images you want to deliver</param>
        /// <param name="height">Height of the images you want to deliver</param>
        /// <param name="fps">expected framerate you want to deliver</param>
        public void AddDevice(string name, int width, int height, int fps)
        {

#if UNITY_WEBGL && !UNITY_EDITOR
            Byn.Awrtc.Browser.CAPI.Unity_VideoInput_AddDevice(name, width, height, fps);
#else
            mInternal.AddDevice(name, width, height, fps);
#endif
        }
        /// <summary>
        /// Removes the device from the internal device list.
        /// </summary>
        /// <param name="name"></param>
        public void RemoveDevice(string name)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Byn.Awrtc.Browser.CAPI.Unity_VideoInput_RemoveDevice(name);
#else
            mInternal.RemoveDevice(name);
#endif
        }

        /// <summary>
        /// Updates the frame of the video device using the given Texture2D. Note that using Textures might not be
        /// the fastest method.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="texture"></param>
        /// <param name="rotation"></param>
        /// <param name="firstRowIsBottom"></param>
        /// <returns></returns>
        public bool UpdateFrame(string name, Texture2D texture, int rotation, bool firstRowIsBottom)
        {
            if (texture.format != Format)
                throw new FormatException("Only " + Format + " supported so far. Use NativeVideoInput.UpdateFrame directly for anything else. ");
            var dataPtr = texture.GetRawTextureData();

#if UNITY_WEBGL && !UNITY_EDITOR
            return Byn.Awrtc.Browser.CAPI.Unity_VideoInput_UpdateFrame(name, dataPtr, 0, dataPtr.Length, texture.width, texture.height, rotation, firstRowIsBottom);
#else

            return mInternal.UpdateFrame(name, dataPtr, texture.width, texture.height, WebRtcCSharp.VideoType.kBGRA, rotation, firstRowIsBottom);
#endif
        }
        /// <summary>
        /// Updates frames via a byte[].  
        /// Width & Height should be the size you used in AddDevice. It is possible the lower layers will scale your image
        /// to fit the values from AddDevice though.
        /// </summary>
        /// <param name="name">Device name. Must be added before this call via AddDevice</param>
        /// <param name="dataPtr">raw data of the image. Must be in the expected Format</param>
        /// <param name="width">width of the image you supply</param>
        /// <param name="height">height of the image you supply</param>
        /// <param name="rotation">Attaches a rotation value to the image. (might not work on all platforms)</param>
        /// <param name="firstRowIsBottom">Unity often has the frames up-side-down in memory. Set this to true to flip it</param>
        /// <returns>
        /// Returns true if the image was updated. 
        /// False if the device wasn't updated (e.g. invalid username, other problems).Consider logging a warning if this happens
        /// </returns>
        public bool UpdateFrame(string name, byte[] dataPtr, int width, int height, int rotation, bool firstRowIsBottom)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return Byn.Awrtc.Browser.CAPI.Unity_VideoInput_UpdateFrame(name, dataPtr, 0, dataPtr.Length, width, height, rotation, firstRowIsBottom);
#else

            return mInternal.UpdateFrame(name, dataPtr, width, height, WebRtcCSharp.VideoType.kBGRA, rotation, firstRowIsBottom);
#endif
        }
        /// <summary>
        /// Updates frames via a IntPtr. Less safe but allows interaction with other native plugins. 
        /// 
        /// See UpdateFrame for other docs
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataPtr"></param>
        /// <param name="length"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rotation"></param>
        /// <param name="firstRowIsBottom"></param>
        /// <returns></returns>
        public bool UpdateFrame(string name, IntPtr dataPtr, int length, int width, int height, int rotation, bool firstRowIsBottom)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return Byn.Awrtc.Browser.CAPI.Unity_VideoInput_UpdateFrame(name, dataPtr, 0, length, width, height, rotation, firstRowIsBottom);
#else

            return mInternal.UpdateFrame(name, dataPtr, length, width, height, WebRtcCSharp.VideoType.kBGRA, rotation, firstRowIsBottom);
#endif
        }

    }
}
