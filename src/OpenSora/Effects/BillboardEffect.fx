#include "Macros.fxh"

DECLARE_TEXTURE(_texture, 0);

BEGIN_CONSTANTS

float4 _diffuseColor;

MATRIX_CONSTANTS

float4x4 _worldView;
float4x4 _projection;

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
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output = (VSOutput)0;
    
    float4 pos = mul(input.Position, _worldView);
	float4x4 worldView2 = _worldView;
	worldView2._m00_m01_m02_m03 = float4(1, 0, 0, 0);
	worldView2._m10_m11_m12_m13 = float4(0, 1, 0, 0);
	worldView2._m20_m21_m22_m23 = float4(0, 0, 1, 0);
	
	float4 pos2 = mul(input.Position, worldView2);
	
    output.Position = mul(float4(pos2.x, pos2.y, pos.z, pos2.w), _projection);
    output.TexCoord = input.TexCoord;

    return output;
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(_texture, input.TexCoord);
	float4 c = color * _diffuseColor;
	
	// Alpha clipping
    clip(c.a < 0.1?-1:1);

    return c;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);