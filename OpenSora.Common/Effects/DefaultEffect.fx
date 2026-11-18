#include "Macros.fxh"

DECLARE_TEXTURE(_texture, 0);

BEGIN_CONSTANTS

#ifdef LIGHTNING
float3 _lightDir;
float3 _lightColor;
#endif

float4 _diffuseColor;

MATRIX_CONSTANTS

#ifdef LIGHTNING
float3x3 _worldInverseTranspose;
#endif

float4x4 _worldViewProj;

END_CONSTANTS

struct VSInput
{
    float4 Position : SV_POSITION;
    float3 Normal   : NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position: SV_POSITION;
    float2 TexCoord: TEXCOORD0;

#ifdef LIGHTNING
	float3 WorldNormal: TEXCOORD1;
#endif
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output = (VSOutput)0;
    
    output.Position = mul(input.Position, _worldViewProj);
    output.TexCoord = input.TexCoord;

#ifdef LIGHTNING
	output.WorldNormal = mul(input.Normal, _worldInverseTranspose);
#endif

    return output;
}

float3 ComputeLighting(float3 normalVector, float3 lightDirection, float3 lightColor, float attenuation)
{
    float diffuse = max(dot(normalVector, lightDirection), 0.0);
    float3 diffuseColor = lightColor  * diffuse * attenuation;

    return diffuseColor;
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(_texture, input.TexCoord);

#ifdef LIGHTNING
	float3 result = float3(0, 0, 0);
	float3 normal = normalize(input.WorldNormal);
    result += ComputeLighting(normal, -_lightDir, _lightColor, 1.0);

	float4 c = color * float4(result, 1) * _diffuseColor;
#else
	float4 c = color * _diffuseColor;
#endif

	// Alpha clipping
    clip(c.a < 0.1?-1:1);

    return c;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);