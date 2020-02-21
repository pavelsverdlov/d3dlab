@vertex@

#include "Common"


struct VSOut
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float2 tex : TEXCOORD0;
};


VSOut main(float4 position : POSITION, float3 normal : NORMAL, float2 tex : TEXCOORD0)
{
    VSOut output = (VSOut) 0;

    output.position = toWVP(position);

    output.normal = mul(normal, World);
    output.normal = normalize(output.normal);
    output.tex = tex;

    return output;
}

@fragment@

#include "Common"

SamplerState SampleType;
Texture2D texturemap : register(t0);

struct PSIn
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float2 tex : TEXCOORD0;
};
float4 main(PSIn input, bool isFront : SV_IsFrontFace) : SV_TARGET
{
    float4 texcolor = texturemap.Sample(SampleType, input.tex);
    
    Material mat = CurrentMaterial;

    mat.ColorAmbient = texcolor;
    mat.ColorDiffuse = texcolor;
    mat.ColorSpecular = texcolor;
    
    float4 normal = input.normal * (1 - isFront * 2);
    
    return ComputePhongColor(input.position.xyz, normal, mat);
}