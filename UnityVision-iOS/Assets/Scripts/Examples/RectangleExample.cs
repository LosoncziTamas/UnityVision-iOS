using System.Collections.Generic;
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
        
        // We use Unity's WebCamTexture API to access image data from device camera.
        private WebCamTexture _webCamTexture;
        
        // Reference to the managed CVPixelBuffer wrapper object.
        // The actual object will be allocated using the appropriate factory method.
        private CVPixelBuffer _cvPixelBuffer;

        private Vector2 _topRight;
        private Vector2 _bottomLeft;
        private Vector2 _topLeft;
        private Vector2 _bottomRight;

        private void Awake()
        {
            _webCamTexture = new WebCamTexture(requestedWidth: 1280, requestedHeight: 720);
            
            // Display the target image
            _image.texture = _webCamTexture;
		
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
                _webCamTexture.Play();
            }
        }

        private void Update()
        {
            if (Application.platform != RuntimePlatform.IPhonePlayer) return;
            // We only classify a new image if no other vision requests are in progress
            if (!_vision.InProgress)
            {

                var allocationResult =
                    CVPixelBuffer.TryCreate(fromWebCamTexture: _webCamTexture, result: out _cvPixelBuffer);
                if (allocationResult == CVReturn.Success)
                {
                    _vision.EvaluateBuffer(_cvPixelBuffer.GetNativePtr(), ImageDataType.CoreVideoPixelBuffer);
                }
            }
        }

        private void OnGUI()
        {
            GUI.color = Color.blue;
            
            GUILayout.Label("Orientation: " + Screen.orientation);
            
            GUI.Label(new Rect(_topRight, new Vector2(100, 100)), $"Top Right: {_topRight.x}, {_topRight.y}");
            GUI.Label(new Rect(_topLeft, new Vector2(100, 100)), $"Top Left: {_topLeft.x}, {_topLeft.y}");
            GUI.Label(new Rect(_bottomLeft, new Vector2(100, 100)), $"Bottom Left: {_bottomLeft.x}, {_bottomLeft.y}");
            GUI.Label(new Rect(_bottomRight, new Vector2(100, 100)), $"Bottom Right: {_bottomRight.x}, {_bottomRight.y}");
        }

        private void Vision_OnRectanglesRecognized(object sender, RectanglesRecognizedArgs e)
        {
            var rect = MapToRectangles(e.points).OrderByDescending(r => r.area).First();
            
            _topRight = rect.topRight;
            _topLeft = rect.topLeft;
            _bottomLeft = rect.bottomLeft;
            _bottomRight = rect.bottomRight;
        }

        private List<VisionRectangle> MapToRectangles(IList<Vector2> points)
        {
            var result = new List<VisionRectangle>();
            var rectCount = points.Count / 4;
           
            // Transform the result to GUI coordinates
            for (var i = 0; i < points.Count; i++)
            {
                points[i] = Vector2.one - points[i];
            }
            for (var i = 0; i < rectCount; i += 4)
            {
                result.Add(new VisionRectangle(
                    topRight: Vector2.Scale(points[i + 0], ScreenDimensions),
                    topLeft: Vector2.Scale(points[i + 1], ScreenDimensions),
                    bottomLeft: Vector2.Scale(points[i + 2], ScreenDimensions),
                    bottomRight: Vector2.Scale(points[i + 3], ScreenDimensions)
                ));
            }
            
            return result;
        }
    }
}