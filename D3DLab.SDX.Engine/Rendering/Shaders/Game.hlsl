cbuffer Game : register(b0) {
	float4x4 View;
	float4x4 Projection;
};

cbuffer Transformation : register(b2) {
	float4x4 World;
}