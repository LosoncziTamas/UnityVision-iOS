using System.Linq;
using Possible.Vision.Managed;
using Possible.Vision.Managed.CoreVideo;
using UnityEngine;
using UnityEngine.UI;

namespace Examples
{
    public class RectangleExample : MonoBehaviour
    {
        private readonly Vector2 ScreenDimensions = new Vector2(Screen.width, Screen.height);
        
        [SerializeField] private Vision _vision;
        [SerializeField] private RawImage _image;
        [SerializeField] private Texture2D _imageToRecognize;
        
        // Reference to the managed CVPixelBuffer wrapper object.
        // The actual object will be allocated using the appropriate factory method.
        private CVPixelBuffer _cvPixelBuffer;

        private Vector2 _topLeft;
        private Vector2 _bottomLeft;
        private Vector2 _topRight;
        private Vector2 _bottomRight;

        private void Awake()
        {
            // Display the target image
            _image.texture = _imageToRecognize;
		
            // We need to tell the Vision plugin what kind of requests do we want it to perform.
            // This call not only prepares the Vision instance for the specified image requests,
            // but allocates VNRequest objects on the native side. You only need to call this
            // method when you initialize your app, and later if you need to change the type
            // of requests you want to perform. When performing image classification requests,
            // maxObservations refers to the number of returned guesses, ordered by confidence.
            _vision.SetAndAllocateRequests(VisionRequest.RectangleDetection, maxObservations: 1);
        }
	
        private void OnEnable()
        {
            // Hook up to the completion event of object classification requests
            _vision.OnRectanglesRecognized += Vision_OnRectanglesRecognized;
        }

        private void OnDisable()
        {
            _vision.OnRectanglesRecognized -= Vision_OnRectanglesRecognized;
        }

        private void Start()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                var allocationResult = CVPixelBuffer.TryCreate(fromTexture: _imageToRecognize, result: out _cvPixelBuffer);
                if (allocationResult == CVReturn.Success)
                {
                    _vision.EvaluateBuffer(_cvPixelBuffer.GetNativePtr(), ImageDataType.CoreVideoPixelBuffer);
                }
                else
                {
                    Debug.LogError("Could not allocate CVPixelBuffer (" + allocationResult + ")");
                }
            }
        }

        private Vector2 FlipVertically(Vector2 vec)
        {
            // ScreenDimensions.y - 
            return new Vector2(vec.x, vec.y);
        }

        private void OnGUI()
        {
            GUI.color = Color.blue;
            
            // In GUI space the Y starts from the top, that's why there is no need for flipping the coordinates in this case.
            GUI.Label(new Rect(FlipVertically(_topLeft), new Vector2(100, 100)), "top left");
            GUI.Label(new Rect(FlipVertically(_topRight), new Vector2(100, 100)), "top right");
            GUI.Label(new Rect(FlipVertically(_bottomLeft), new Vector2(100, 100)), "bottom left");
            GUI.Label(new Rect(FlipVertically(_bottomRight), new Vector2(100, 100)), "bottom right");
        }

        private void Vision_OnRectanglesRecognized(object sender, RectanglesRecognizedArgs e)
        {
            var result = e.rectangles.First();
            
            // TODO: figure out why coordinates are so inconsistent
            _topLeft = Vector2.Scale(result.bottomLeft, ScreenDimensions);
            _topRight = Vector2.Scale(result.bottomRight, ScreenDimensions);
            _bottomLeft = Vector2.Scale(result.topLeft, ScreenDimensions);
            _bottomRight = Vector2.Scale(result.topRight, ScreenDimensions);
        }
    }
}