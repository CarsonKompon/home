
//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	Description = "Pixelation shader that pixelates the given texture";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "common/features.hlsl"
}

//=========================================================================================================================
COMMON
{
	#include "common/shared.hlsl"
}

//=========================================================================================================================

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"
	//
	// Main
	//
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VertexInput i ) )
	{
		PixelInput o = ProcessVertex( i );
		// Add your vertex manipulation functions here
		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{
    #include "common/pixel.hlsl"
	
	// Define the color variable
	float3 g_vColorTint < 
		UiType( Color ); 
		Default3( 1.0, 1.0, 1.0 ); 
		UiGroup( "Color,10/20" ); 
	>;

	// Expose the variable to the material editor
	Float3Attribute( g_vColorTint, g_vColorTint );

	// Define the texture variable
	float3 g_vTexture < 
		UiType( Texture ); 
		Default3( 1.0, 1.0, 1.0 ); 
		UiGroup( "Texture,10/20" ); 
	>;

	// Expose the variable to the material editor
	TextureAttribute( g_vTexture, g_vTexture );

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::From( i );

		// Apply the color tint and texture to the material
		m.Color = g_vColorTint;
		m.Texture = g_vTexture;

		return ShadingModelStandard::Shade( i, m );
	}
}

// //=========================================================================================================================

// HEADER
// {
// 	Description = "Pixelation shader that pixelates the given texture";
// }

// COMMON
// {
// 	#include "common/shared.hlsl"
// }

// struct VertexInput
// {
// 	#include "common/vertexinput.hlsl"
// };

// struct PixelInput
// {
// 	#include "common/pixelinput.hlsl"
// };

// VS
// {
// 	#include "common/vertex.hlsl"

// 	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VertexInput i ) )
// 	{
// 		PixelInput o = ProcessVertex( i );
// 		return FinalizeVertex( o );
// 	}
// }

// PS
// {
//     #include "common/pixel.hlsl"
	
// 	// Define the color variable
// 	float3 g_vColorTint < 
// 		UiType( Color ); 
// 		Default3( 1.0, 1.0, 1.0 ); 
// 		UiGroup( "Color,10/20" ); 
// 	>;

// 	// Expose the variable to the material editor
// 	Float3Attribute( g_vColorTint, g_vColorTint );

// 	// Define the texture variable
// 	float3 g_vTexture < 
// 		UiType( Texture ); 
// 		Default3( 1.0, 1.0, 1.0 ); 
// 		UiGroup( "Texture,10/20" ); 
// 	>;

// 	// Expose the variable to the material editor
// 	TextureAttribute( g_vTexture, g_vTexture );

// 	float4 MainPs( PixelInput i ) : SV_Target0
// 	{
// 		Material m = GatherMaterial( i );

// 		// Apply the color tint and texture to the material
// 		m.Color = g_vColorTint;
// 		m.Texture = g_vTexture;

// 		return FinalizePixelMaterial( i, m );
// 	}
// }
