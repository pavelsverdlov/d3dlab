struct VSInput
{
	float4 p : POSITION0;
	float3 n : NORMAL0;
	float4 c : COLOR0;
	float2 t : TEXCOORD0;
};
struct PSInput
{
	float4 p : SV_POSITION;
	float4 wp : POSITION0;
	float4 sp : TEXCOORD1;
	float3 n : NORMAL;
	float2 t : TEXCOORD0;
	float4 c : COLOR;
};


cbuffer ProjectionBuffer : register(b0)
{
    float4x4 Projection;
}

cbuffer ViewBuffer : register(b1)
{
    float4x4 View;
}

cbuffer WorldBuffer : register(b2)
{
    float4x4 World;
}

PSInput VShaderDefault(VSInput input)
{
	PSInput output = (PSInput)0;
	float4 inputp = input.p;

	//set position into camera clip space	
	output.p = mul(World, inputp);
	output.wp = output.p;
	output.p = mul(View, output.p);
	output.p = mul(Projection, output.p);

	//set position into light-clip space
	//if (bHasShadowMap)
	{
		//for (int i = 0; i < 1; i++)
		{
			output.sp = mul(inputp, mWorld);
			output.sp = mul(output.sp, mLightView[0]);
			output.sp = mul(output.sp, mLightProj[0]);
		}
	}

	//set texture coords and color
	output.t = input.t;
	output.c = input.c;

	//set normal for interpolation	
	output.n = normalize(mul(input.n, (float3x3)mWorld));

	/*
	if (bHasNormalMap)
	{
	// transform the tangents by the world matrix and normalize
	output.t1 = normalize(mul(input.t1, (float3x3)mWorld));
	output.t2 = normalize(mul(input.t2, (float3x3)mWorld));
	}
	else
	{
	output.t1 = 0.0f;
	output.t2 = 0.0f;
	}
	*/
	
	return output;
}