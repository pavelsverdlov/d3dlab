cbuffer Game : register(b0) {
	float4 LookDirection;
	
	float4x4 View;
	float4x4 Projection;

	
};

cbuffer Transformation : register(b2) {
	float4x4 World;
}


float4 toScreen(float3 v) {
	float4 p = float4(v, 1);
	p = mul(View, p);
	p = mul(Projection, p);
	return p;
}