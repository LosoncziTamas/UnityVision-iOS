///////////////////////////////////////////////////////////////////////////////
// ARRectangleExample.cs
// 
// Author: Adam Hegedus
// Contact: adam.hegedus@possible.com
// Copyright © 2018 POSSIBLE CEE. Released under the MIT license.
///////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using Possible.Vision.Managed;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Utils;

namespace Examples
{
	/// <summary>
	/// This example demonstrates how to cast a recognized rectangle from screen to an ARKit surface.
	/// </summary>
	public class ARRectangleExample : MonoBehaviour 
	{
		private readonly Vector2 ScreenDimensions = new Vector2(Screen.width, Screen.height);
		
		[SerializeField] private ARCameraManager _cameraManager;
		[SerializeField] private Vision _vision;
		[SerializeField] private RectangleMarker _rectangleMarkerPrefab;
		[SerializeField] private Camera _arCamera;
		[SerializeField] private ARRaycastManager _raycastManager;
		
		private RectangleMarker _marker;

		private void Awake()
		{
			// We need to tell the Vision plugin what kind of requests do we want it to perform.
			// This call not only prepares the managed wrapper for the specified image requests,
			// but allocates VNRequest objects on the native side. You only need to call this
			// method when you initialize your app, and later if you need to change the type
			// of requests you want to perform. When performing rectangle detection requests,
			// maxObservations refers to the maximum number of rectangles allowed to be recognized at once.
			_vision.SetAndAllocateRequests(VisionRequest.RectangleDetection, maxObservations: 1);
		}

		private void OnEnable()
		{
			// Hook up to the completion event of rectangle detection requests
			_vision.OnRectanglesRecognized += Vision_OnRectanglesRecognized;
		
			// Hook up to ARFoundation's frame update callback to be able to get a handle to the latest frame
			_cameraManager.frameReceived += CameraManager_OnFrameReceived;
		}

		private void OnDisable()
		{
			_vision.OnRectanglesRecognized -= Vision_OnRectanglesRecognized;
			_cameraManager.frameReceived -= CameraManager_OnFrameReceived;
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
				_vision.EvaluateBuffer(frame.nativePtr, ImageDataType.ARFrame);
			}
		}

		class RectangleHit
		{
			public Vector3 topLeft;
			public Vector3 topRight;
			public Vector3 bottomLeft;
			public Vector3 bottomRight;
		}

		private bool HitTestForRect(VisionRectangle visionRect, out RectangleHit result)
		{
			var hits = new List<ARRaycastHit>();
			
			// Vision rect coordinates are normalized.
			var topLeft = Vector3.Scale(visionRect.topLeft, ScreenDimensions);
			var bottomLeft = Vector3.Scale(visionRect.bottomLeft, ScreenDimensions);
			var topRight = Vector3.Scale(visionRect.topRight, ScreenDimensions);
			var bottomRight = Vector3.Scale(visionRect.bottomRight, ScreenDimensions);
			
			result = new RectangleHit();
			
			if (_raycastManager.Raycast(topLeft, hits, TrackableType.Planes))
			{
				result.topLeft = hits.First().pose.position;
			}
			else return false;

			if (_raycastManager.Raycast(topRight, hits, TrackableType.Planes))
			{
				result.topRight = hits.First().pose.position;
			}
			else return false;
			
			if (_raycastManager.Raycast(bottomLeft, hits, TrackableType.Planes))
			{
				result.bottomLeft = hits.First().pose.position;
			} 
			else return false;
			
			if (_raycastManager.Raycast(bottomRight, hits, TrackableType.Planes))
			{
				result.bottomRight = hits.First().pose.position;
			} 
			else return false;

			return true;
		}

		private void Vision_OnRectanglesRecognized(object sender, RectanglesRecognizedArgs e)
		{
			var rectangles = e.rectangles.OrderByDescending(entry => entry.area).ToList();
			var found = false;

			foreach (var rect in rectangles)
			{
				if (HitTestForRect(rect, out var hitResult))
				{
					if (_marker == null)
					{
						_marker = Instantiate(_rectangleMarkerPrefab);
						Debug.Assert(_marker != null, "Could not instantiate rectangle marker prefab.");
					
						// Reset transform
						_marker.transform.position = Vector3.zero;
						_marker.transform.rotation = Quaternion.identity;
						_marker.transform.localScale = Vector3.one;
					}

					// Assign the corners of the marker game object to the surface hit points
					_marker.TopLeft = hitResult.topLeft;
					_marker.TopRight = hitResult.topRight;
					_marker.BottomRight = hitResult.bottomRight;
					_marker.BottomLeft = hitResult.bottomLeft;
				
					found = true;
				}
			}
			
			if (_marker != null)
			{
				// Hide the marker if no rectangles were found
				_marker.gameObject.SetActive(found);
			}
		}
	}
}
