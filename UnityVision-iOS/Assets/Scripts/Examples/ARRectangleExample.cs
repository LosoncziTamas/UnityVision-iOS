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

		private bool HitTestForRect(VisionRectangle rectangle, out RectangleHit result)
		{
			var hits = new List<ARRaycastHit>();
			result = null;
			
			Vector3 topLeft;
			if (_raycastManager.Raycast(rectangle.topLeft, hits, TrackableType.Planes))
			{
				topLeft = hits.First().pose.position;
			}
			else return false;

			Vector3 topRight;
			if (_raycastManager.Raycast(rectangle.topRight, hits, TrackableType.Planes))
			{
				topRight = hits.First().pose.position;
			}
			else return false;
			
			Vector3 bottomLeft;
			if (_raycastManager.Raycast(rectangle.bottomLeft, hits, TrackableType.Planes))
			{
				bottomLeft = hits.First().pose.position;
			} 
			else return false;
			
			Vector3 bottomRight;
			if (_raycastManager.Raycast(rectangle.bottomRight, hits, TrackableType.Planes))
			{
				bottomRight = hits.First().pose.position;
			} 
			else return false;

			result = new RectangleHit
			{
				topLeft = topLeft, 
				bottomLeft = bottomLeft, 
				topRight = topRight, 
				bottomRight = bottomRight
			};

			return true;
		}
	
		private void Vision_OnRectanglesRecognized(object sender, RectanglesRecognizedArgs e)
		{
			var rectangles = e.rectangles.OrderByDescending(entry => entry.area).ToList();
			var found = false;

			foreach (var rect in rectangles)
			{
				if (HitTestForRect(rect, out var result))
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
					_marker.TopLeft = result.topLeft;
					_marker.TopRight = result.topRight;
					_marker.BottomRight = result.bottomRight;
					_marker.BottomLeft = result.bottomLeft;

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
