/**************************************************************************************/
/* https://alastaira.wordpress.com/2014/12/27/using-the-stencil-buffer-in-unity-free/ */
/* http://docs.unity3d.com/Manual/SL-Stencil.html									  */
/**************************************************************************************/
Shader "Custom/DepthStencil" 
{
	Properties
	{
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_SpecColor("Spec Color", Color) = (1, 1, 1, 0)
		_Emission("Emissive Color", Color) = (0, 0, 0, 0)
		_Shininess("Shininess", Range(0.1, 1)) = 0.7
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader
	{
		Tags{ "Queue" = "Geometry-1" }  // Write to the stencil buffer before drawing any geometry to the screen
		Pass
		{
			// Write the value 1 to the stencil buffer
			Stencil
			{
				Ref 1           //Reference Value 1
				Comp Always     //Make the stencil test always pass.
				Pass Replace    //Write the reference value into the buffer.
			}
		}
	}
}