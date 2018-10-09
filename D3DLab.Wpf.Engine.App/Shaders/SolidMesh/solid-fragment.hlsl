struct PSInput
{
	float4 p : SV_POSITION;
	float4 wp : POSITION0;
	float4 sp : TEXCOORD1;
	float3 n : NORMAL;
	float2 t : TEXCOORD0;
	float4 c : COLOR;
};

float4 FS(PSInput input) : SV_Target
{
	return input.c;
}