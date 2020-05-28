﻿///////////////////////////////////////////////////////////////////////////////
// ImageDataType.cs
// 
// Author: Adam Hegedus
// Contact: adam.hegedus@possible.com
// Copyright © 2018 POSSIBLE CEE. Released under the MIT license.
///////////////////////////////////////////////////////////////////////////////

namespace Possible.Vision.Managed
{
	public enum ImageDataType 
	{
		/// <summary>
		/// Use this value whenever you need to analyze image data stored in a texture (using Metal graphics api).
		/// https://docs.unity3d.com/ScriptReference/Texture.GetNativeTexturePtr.html
		/// </summary>
		MetalTexture,
		
		/// <summary>
		/// Use this value if you need to analyze image data stored in the ARFrame
		/// https://developer.apple.com/documentation/arkit/arframe
		/// </summary>
		ARFrame
	}
}