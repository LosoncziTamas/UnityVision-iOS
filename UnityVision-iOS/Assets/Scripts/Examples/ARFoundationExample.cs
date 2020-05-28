using System;
using System.Linq;
using Possible.Vision.Managed;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Examples
{
    public class ARFoundationExample : MonoBehaviour
    {
        [SerializeField] private ARCameraManager _cameraManager;
        [SerializeField] private Camera _arCamera;
        [SerializeField] private Vision _vision;
        [SerializeField] private Text _text;

        private void Awake()
        {
            // We need to tell the Vision plugin what kind of requests do we want it to perform.
            // This call not only prepares the managed wrapper for the specified image requests,
            // but allocates VNRequest objects on the native side. You only need to call this
            // method when you initialize your app, and later if you need to change the type
            // of requests you want to perform. When performing image classification requests,
            // maxObservations refers to the number of returned guesses, ordered by confidence.
            _vision.SetAndAllocateRequests(VisionRequest.Classification, maxObservations: 1);            
        }

        private void OnEnable()
        {
            // Hook up to ARKit's frame update callback to be able to get a handle to the latest pixel buffer
            _cameraManager.frameReceived += CameraManager_OnFrameReceived;
        
            // Hook up to the completion event of object classification requests
            _vision.OnObjectClassified += Vision_OnObjectClassified;   
        }
        
        private void OnDisable()
        {
            _cameraManager.frameReceived -= CameraManager_OnFrameReceived;
            _vision.OnObjectClassified -= Vision_OnObjectClassified;
        }

        private void Vision_OnObjectClassified(object sender, ClassificationResultArgs e)
        {
            // Display the top guess for the dominant object on the image
            var result = e.observations.First();
            _text.text = $"{result.identifier} \nconfidence: {result.confidence:P2}";
        }
        
        private void CameraManager_OnFrameReceived(ARCameraFrameEventArgs obj)
        {
            if (_vision.InProgress)
            {
                return;
            }

            var cameraParams = new XRCameraParams
            {
                zNear = _arCamera.nearClipPlane,
                zFar = _arCamera.farClipPlane,
                screenWidth = Screen.width,
                screenHeight = Screen.height,
                screenOrientation = Screen.orientation
            };

            if (_cameraManager.subsystem.TryGetLatestFrame(cameraParams, out var frame))
            {
                // This is the call where we pass in the handle to the image data to be analysed
                _vision.EvaluateBuffer(frame.nativePtr, ImageDataType.CoreVideoPixelBuffer);
            }
#if false
            if (_cameraManager.TryGetLatestImage(out var image))
            {
                var convParams = new XRCameraImageConversionParams()
                {
                    // The image is rotated counter-clockwise.
                    inputRect = new RectInt(0, 0, image.width, image.height),
                    outputDimensions = new Vector2Int(image.width, image.height),
                    outputFormat = TextureFormat.RGB24,
                    transformation = CameraImageTransformation.None
                };
                        
                // Perform conversion on a newly created buffer
                var buffer = new NativeArray<byte>(image.GetConvertedDataSize(convParams), Allocator.Temp);
                image.Convert(convParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);
                image.Dispose();
                
                // Convert it to a texture and store it
                if (!_cameraImageBuffer)
                {
                    _cameraImageBuffer = new Texture2D(convParams.outputDimensions.x,convParams.outputDimensions.y, convParams.outputFormat,false);
                }
                _cameraImageBuffer.LoadRawTextureData(buffer);
                _cameraImageBuffer.Apply();
            
                buffer.Dispose();   
            }
#endif
        }

    }
}