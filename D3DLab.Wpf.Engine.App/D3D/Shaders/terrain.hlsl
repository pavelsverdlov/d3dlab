@vertex@

#include "Game"
#include "Light"

struct VSOut
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float4 positionOrig : TEXCOORD1;
};
VSOut main(float4 position : POSITION, float3 normal : NORMAL, float4 color : COLOR, float2 tex : TEXCOORD) {
	VSOut output = (VSOut)0;

	output.positionOrig = position;
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
	float4 positionOrig : TEXCOORD1;
};

Texture2D seafloorTexture : register(t0);
Texture2D shoreTexture : register(t1);
Texture2D sandTexture : register(t2);
Texture2D grassTexture : register(t3);
Texture2D dirtTexture : register(t4);
Texture2D rockTexture  : register(t5);
Texture2D snowTexture  : register(t6);

Texture2D slopeTexture : register(t7);


SamplerState SampleType;

float4 blendColors(float4 color1, float4 color2, float slope) {
	float4 blendAmount;
	float4 color;
	if (slope < 0.2)
	{
		blendAmount = slope / 0.2f;
		color = lerp(color1, color2, blendAmount);
	}
	if ((slope < 0.7) && (slope >= 0.2f)) {
		blendAmount = (slope - 0.2f) * (1.0f / (0.7f - 0.2f));
		color = lerp(color2, color1, blendAmount);
	}
	if (slope >= 0.7)
	{
		color = color1;
	}
	return color;
}
float4 biome(float e, float2 tex, float slope) {
	float4 color;

	if (e < -0.25) {
		color = blendColors(seafloorTexture.Sample(SampleType, tex), slopeTexture.Sample(SampleType, tex), slope);
	}
	else if (e < 0) {
		color = blendColors(seafloorTexture.Sample(SampleType, tex), slopeTexture.Sample(SampleType, tex), slope);
	}
	else if (e < 0.0625) {
		color = blendColors(shoreTexture.Sample(SampleType, tex), slopeTexture.Sample(SampleType, tex), slope);
	}
	else if (e < 0.125) {
		color = blendColors(sandTexture.Sample(SampleType, tex), slopeTexture.Sample(SampleType, tex), slope);
	}
	else if (e < 0.375) {
		color = blendColors(grassTexture.Sample(SampleType, tex), slopeTexture.Sample(SampleType, tex), slope);
	}
	else if (e < 0.75) {
		color = blendColors(rockTexture.Sample(SampleType, tex), slopeTexture.Sample(SampleType, tex), slope);
	}
	else {
		color = blendColors(rockTexture.Sample(SampleType, tex), slopeTexture.Sample(SampleType, tex), slope);
		float4 blendAmount;
		if (slope < 0.2)
		{
			color = lerp(float4(1, 1, 1, 1), color, 0.2);
		}
	}
	return color;
}

float4 main(PSIn input) : SV_TARGET{
	float4 color = input.color;
	float2 tex = input.tex;
	float3 normal = input.normal;
	float3 orig = input.positionOrig;

	float slope = 1.0f - normal.y;

	return color * biome(orig.y / 50, tex, slope);
}

float4 main1(PSIn input) : SV_TARGET{
	float4 color = input.color;
	float2 tex = input.tex;
	float3 normal = input.normal;

	

	//return color * grassTexture.Sample(SampleType, tex);

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