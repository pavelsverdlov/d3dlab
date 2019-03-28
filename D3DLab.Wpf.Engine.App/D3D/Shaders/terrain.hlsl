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
	float3 tangent : TANGENT;
	float3 binormal : BINORMAL;
	float2 normalMap: TEXCOORD2;
	float4 depthPosition : TEXCOORD3;
};

VSOut main(float4 position : POSITION, float3 normal : NORMAL, float2 tex : TEXCOORD0, float2 normalMapTexCoor : TEXCOORD1, float3 tangent : TANGENT, float3 binormal : BINORMAL, float4 color : COLOR) {
	VSOut output = (VSOut)0;

	output.positionOrig = position;
	output.position = toWVP(position);

	output.tex = tex;
	output.normalMap = normalMapTexCoor;

	normal = mul(World, normal);
	normal = normalize(normal);

	output.color = color * computeLight(output.position.xyz, normal, -LookDirection.xyz, 1000);
	output.normal = normal;
	
	output.tangent = mul(World, tangent);
	output.tangent = normalize(output.tangent);
	
	output.binormal = mul(World, binormal);
	output.binormal = normalize(output.binormal);

	output.depthPosition = output.position;

	return output;
}

@fragment@

#include "Math"
#include "Game"
//#include "Light"

struct PSIn
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float4 positionOrig : TEXCOORD1;
	float3 tangent : TANGENT;
	float3 binormal : BINORMAL;
	float2 normalMap: TEXCOORD2;
	float4 depthPosition : TEXCOORD3;
};

Texture2D seafloorTexture : register(t0);
Texture2D shoreTexture : register(t1);
Texture2D sandTexture : register(t2);
Texture2D grassTexture : register(t3);
Texture2D dirtTexture : register(t4);
Texture2D rockTexture  : register(t5);
Texture2D slopeTexture : register(t6);

Texture2D rockNormalMapTexture : register(t7);
Texture2D snowNormalMapTexture : register(t8);
Texture2D distanceNormalMapTexture : register(t9);




SamplerState SampleType;


//static const float seafloor = 0;//-0.25
//static const float shore = 0.0625;
//static const float sand = 0.125;
//static const float grass = 0.375;
//static const float rock = 0.75;

static const float seafloor = 0.05;
static const float shore = 0.105;
static const float sand = 0.2;
static const float grass = 0.35;
static const float rock = 0.45;


float4 normalMapping(float4 colorFirst, PSIn input) {//Texture2D normalMapBakinTexture
	float4 bumpMap;
	float3 bumpNormal;
	float lightIntensity;

	float depthValue = input.depthPosition.z / input.depthPosition.w;
	// Select the normal map for the first material based on the distance.
	if (depthValue > 0.998f) {
		bumpMap = distanceNormalMapTexture.Sample(SampleType, input.normalMap);
	}
	else {
		bumpMap = rockNormalMapTexture.Sample(SampleType, input.tex);
	}

	bumpMap = (bumpMap * 2.0f) - 1.0f;
	bumpNormal = (bumpMap.x * input.tangent) + (bumpMap.y * input.binormal) + (bumpMap.z * input.normal);
	bumpNormal = normalize(bumpNormal);

	/*float intensity = 0.0;
	for (int i = 0; i < 3; ++i) {
		Light l = lights[i];

	}*/

	lightIntensity = saturate(dot(bumpNormal, float3(0, 1.2, 0.5)));

	return saturate(colorFirst * lightIntensity);
}


float3 blend(float depth, float4 texture1, float a1, float4 texture2, float a2) {
	float ma = max(texture1.a + a1, texture2.a + a2) - depth;

	float b1 = max(texture1.a + a1 - ma, 0);
	float b2 = max(texture2.a + a2 - ma, 0);

	return (texture1.rgb * b1 + texture2.rgb * b2) / (b1 + b2);
}


float4 blendColorsForSlope(float4 color1, float4 color2, float slope) {
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

//float4 test(PSIn input) {
//	float4 position = input.position;
//	float3 blendWeights = abs(input.normal);
//	// blendWeights = blendWeights - plateauSize;
//	float transitionSpeed = 1;
//	blendWeights = pow(max(blendWeights, 0), transitionSpeed);
//	float2 coord1 = (position.yz + nLength) * texScale;
//	float2 coord2 = (position.zx + nLength) * texScale;
//	float2 coord3 = (position.xy + nLength) * texScale;
//	float4 col1 = tex2D(texFromX, coord1);
//	float4 col2 = tex2D(texFromY, coord2);
//	float4 col3 = tex2D(texFromZ, coord3);
//	float4 textColour = float4(col1.xyz * blendWeights.x +
//		col2.xyz * blendWeights.y +
//		col3.xyz * blendWeights.z, 1);
//}

float4 interpolateColorsEdge(float elevation, float4 colorToMin, float4 colorToMax, float rangeMin, float rangeMax) {
	float blendAmount = toNewRange(elevation, rangeMin, rangeMax, 0.0, 1.0);

	return float4(blend(0.5, colorToMin, 1 - blendAmount, colorToMax, blendAmount), 1);
	//the same result as blend
	return lerp(colorToMin, colorToMax, blendAmount);
}


float4 biome(float e, PSIn input, float slope) {
	float4 color;
	float2 tex = input.tex;
	float4 slopeColor = slopeTexture.Sample(SampleType, tex);

	if (e < seafloor) {
		color = interpolateColorsEdge(e, seafloorTexture.Sample(SampleType, tex), shoreTexture.Sample(SampleType, tex), 0, seafloor);
		color = blendColorsForSlope(color, slopeColor, slope);
	}
	else if (e < shore) {
		color = interpolateColorsEdge(e, shoreTexture.Sample(SampleType, tex), grassTexture.Sample(SampleType, tex), seafloor, shore);
		color = blendColorsForSlope(color, slopeColor, slope);
	}
	/*else if (e < sand) {
		color = blendColorsForSlope(sandTexture.Sample(SampleType, tex), slopeTexture.Sample(SampleType, tex), slope);
	}*/
	else if (e < grass) {
		color = interpolateColorsEdge(e, grassTexture.Sample(SampleType, tex), rockTexture.Sample(SampleType, tex), shore, grass);
		color = blendColorsForSlope(color, slopeColor, slope);
	}
	else if (e < rock) {
		color = interpolateColorsEdge(e, rockTexture.Sample(SampleType, tex), float4(1.0f, 1.0f, 1.0f, 1.0f), grass, rock);
		color = blendColorsForSlope(color, slopeColor, slope);
	}
	else { // snow
		color = blendColorsForSlope(rockTexture.Sample(SampleType, tex), slopeColor, slope);
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

	return color * biome(orig.y / 50, input, slope);
}

//float4 calculateBumpColor(float4 color, PSIn input) {
//	float4 bumpMap = (color * 2.0f) - 1.0f;
//
//	float3 bumpNormal = (bumpMap.x * input.tangent) + (bumpMap.y * input.binormal) + (bumpMap.z * input.normal);
//	bumpNormal = normalize(bumpNormal);
//
//	float lightIntensity = computeLight(input.position.xyz, bumpNormal, -LookDirection.xyz, 1000);// saturate(dot(bumpNormal, lightDir));
//
//	return saturate(color * lightIntensity);
//}


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