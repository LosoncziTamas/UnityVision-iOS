///////////////////////////////////////////////////////////////////////////////
// EventArguments.cs
// 
// Author: Adam Hegedus
// Contact: adam.hegedus@possible.com
// Copyright © 2018 POSSIBLE CEE. Released under the MIT license.
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using Possible.Vision.Managed.Bridging;
using UnityEngine;

namespace Possible.Vision.Managed
{
    /// <summary>
    /// Carries the results of a successful image classification request.
    /// </summary>
    public class ClassificationResultArgs : EventArgs
    {
        public readonly VisionClassification[] observations;

        public ClassificationResultArgs(VisionClassification[] observations)
        {
            this.observations = observations;
        }
    }

    public class RectanglesRecognizedArgs : EventArgs
    {
        /// <summary>
        /// Directly returned points for the recognized rectangle.
        /// </summary>
        public readonly IList<Vector2> points;

        public RectanglesRecognizedArgs(IList<Vector2> points)
        {
            this.points = points;
        }
    }

#if false
    
    /// <summary>
    /// Carries the results of a successful rectangle detection request.
    /// </summary>
    public class RectanglesRecognizedArgs : EventArgs
    {
        /// <summary>
        /// Rectangles with their respective normalized frame coordinates.
        /// </summary>
        public readonly VisionRectangle[] rectangles;
        

        public RectanglesRecognizedArgs(IList<Vector2> points)
        {
            var rectCount = points.Count / 4;
            rectangles = new VisionRectangle[rectCount];
            if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                // Align the specified normalized screen coordinates to device orientation.
                for (var i = 0; i < points.Count; i++)
                {
                    points[i] = Vector2.one - points[i];
                }

                for (var i = 0; i < rectCount; i += 4)
                {
                    // The order can change depending on what kind of texture is provided to Vision.
                    // So we sort it by vector components and set the rect corner accordingly.
                    var sorted = points.Take(4).OrderBy(vector2 => vector2.x).ToArray();
                    var bottomLeft = sorted[0].y < sorted[1].y ? sorted[0] : sorted[1];
                    var topLeft = sorted[0].y < sorted[1].y ? sorted[1] : sorted[0];
                    var bottomRight = sorted[2].y < sorted[3].y ? sorted[2] : sorted[3];
                    var topRight = sorted[2].y < sorted[3].y ? sorted[3] : sorted[2];
                    
                    Debug.Log($"orig bottomLeft x : {points[i].x:0.00}, topLeft: {points[i + 1].x:0.00}, bottomRight: {points[i + 2].x:0.00}, topRight: {points[i + 3].x:0.00}");
                    Debug.Log($"orig bottomLeft y: {points[i].y:0.00}, topLeft: {points[i + 1].y:0.00}, bottomRight: {points[i + 2].y:0.00}, topRight: {points[i + 3].y:0.00}");
                    
                    Debug.Log($"sorted bottomLeft x : {bottomLeft.x:0.00}, topLeft: {topLeft.x:0.00}, bottomRight: {bottomRight.x:0.00}, topRight: {topRight.x:0.00}");
                    Debug.Log($"sorted bottomLeft y: {bottomLeft.y:0.00}, topLeft: {topLeft.y:0.00}, bottomRight: {bottomRight.y:0.00}, topRight: {topRight.y:0.00}");

                    rectangles[i] = new VisionRectangle(
                        bottomLeft: bottomLeft, topLeft: topLeft,
                        bottomRight: bottomRight, topRight: topRight);
                }
            }
            else
            {
                for (var i = 0; i < rectCount; i += 4)
                {
                    rectangles[i] = new VisionRectangle(
                        topLeft: points[i], topRight: points[i + 1],
                        bottomRight: points[i + 2], bottomLeft: points[i + 3]);
                }
            }
        }
    }
#endif
}