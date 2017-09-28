/****************************************************************************************************/
/* Minus the Stencil{} part, thanks to :  http://wiki.unity3d.com/index.php?title=AlphaVertexLitZ   */
/****************************************************************************************************/

Shader "Custom/TransparencyStencil" 
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
		// Write the value 1 to the stencil buffer
		Stencil
		{
			Ref 1		  //Reference Value 
			Comp NotEqual //Only render pixels whose reference value differs from the value in the buffer.
		}

		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+1" }
		//Render into depth buffer only
		Pass
		{
			ColorMask 0
		}
		// Render normally
		Pass
		{
			ZWrite Off
			Blend SrcAlpha SrcAlpha
			ColorMask RGB
			Material
			{
				Diffuse[_Color]
				Ambient[_Color]
				Shininess[_Shininess]
				Specular[_SpecColor]
				Emission[_Emission]
			}
			Lighting On
			SetTexture[_MainTex]
			{
				Combine texture * primary DOUBLE, texture * primary //multyply rgb * double, multiply alpha 
			}
		}
	}
}
