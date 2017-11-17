/*float4 main(float4 position : POSITION) : SV_POSITION {
	return position;
}
*/
cbuffer ConstantBuffer : register(b0)
{
	matrix World;
	matrix View;
	matrix Projection;
}


struct VSOut {
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

VSOut main(float4 position : POSITION, float4 color : COLOR) {
	VSOut output;
	output.position = mul(position, World);
	output.position = mul(output.position, View);
	output.position = mul(output.position, Projection);
	output.color = color;
	
	return output;
}