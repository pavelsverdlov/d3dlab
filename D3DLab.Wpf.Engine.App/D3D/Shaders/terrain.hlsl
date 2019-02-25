@vertex@

#include "Game"
#include "Light"

struct VSOut
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
};
VSOut main(float4 position : POSITION, float3 normal : NORMAL, float4 color : COLOR, float2 tex : TEXCOORD) {
	VSOut output = (VSOut)0;

	output.position = toWVP(position);

	output.tex = tex;

	normal = mul(World, normal);
	normal = normalize(normal);

	output.color = color * computeLight(output.position.xyz, normal, -LookDirection.xyz, 1000);
	output.normal = normal;
	
	return output;
}

@fragment@

struct PSIn
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

Texture2D grassTexture : register(t0);
Texture2D slopeTexture : register(t1);
Texture2D rockTexture  : register(t2);

SamplerState SampleType;

float4 main(PSIn input) : SV_TARGET{
	float4 color = input.color;
	float2 tex = input.tex;
	float3 normal = input.normal;

	float blendAmount;
	float4 textureColor;

	float4 grassColor = grassTexture.Sample(SampleType, tex);
	float4 slopeColor = slopeTexture.Sample(SampleType, tex);
	float4 rockColor = rockTexture.Sample(SampleType, tex);

	// Calculate the slope of this point.
	float slope = 1.0f - normal.y;

	// Determine which texture to use based on height.
	if (slope < 0.2)
	{
		blendAmount = slope / 0.2f;
		textureColor = lerp(grassColor, slopeColor, blendAmount);
	}

	if ((slope < 0.7) && (slope >= 0.2f))
	{
		blendAmount = (slope - 0.2f) * (1.0f / (0.7f - 0.2f));
		textureColor = lerp(slopeColor, rockColor, blendAmount);
	}

	if (slope >= 0.7)
	{
		textureColor = rockColor;
	}

	return color * textureColor;
}

@geometry@