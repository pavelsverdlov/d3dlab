@vertex@

#include "Common"

struct VSOut {
	float4 position : SV_Position;
	float4 color: COLOR;
};
VSOut main(float4 position : POSITION, float4 color : COLOR) {
    VSOut output;

	output.position = mul(position, World);
	//output.position = mul(View, output.position);
	//output.position = mul(Projection, output.position);
	output.color = color;
	return output;
}

@fragment@

#include "Common"

float4 main(float4 position : SV_POSITION, float4 color : COLOR, float4 normal : NORMAL) : SV_TARGET{
    Material material = (Material)0;
	material.ColorAmbient = color;
	material.ColorDiffuse = color;
	material.ColorSpecular = color;
	material.ColorReflection = color;
	material.SpecularFactor = 400;

	return ComputePhongColor(position.xyz, normal, material);
}

@geometry@

#include "Common"

float radius = 1.5f;

struct GSIn {
    float4 position : SV_Position;
    float4 color: COLOR;
};
struct GSOut {
    float4 position : SV_Position;
    float4 color: COLOR;
    float4 normal : NORMAL;
};
//float PI = 3.14159265359f;

GSOut createVertex(in float3 sphCenter, in float3 p, in float4 color) {
    GSOut fs = (GSOut)0;
    fs.position = toScreen(p);
    fs.normal = float4(p - sphCenter, 0);
    fs.color = color;
    return fs;
}

[maxvertexcount(75)]//75
void main(point GSIn points[1], inout TriangleStream<GSOut> output) {
    float PI = 3.14159265359f;
    float radius = 2.5;

    float3 look = -normalize(v4LookDirection.xyz);
    float i = 10 * (PI / 180);
    GSOut fs = (GSOut)0;
    float3 sphCenter = points[0].position.xyz;
    float4 color = points[0].color;
    float3 center = sphCenter + look * radius;
    float3 tangent = cross(look, float3(1, 0, 0));

    float3 N = look;
    float3x3 TBN = fromAxisAngle3x3(i, N);

    output.Append(createVertex(sphCenter, center, color));
    output.Append(createVertex(sphCenter, center + tangent * radius, color));
    tangent = normalize(mul(tangent, TBN));
    output.Append(createVertex(sphCenter, center + tangent * radius, color));

    for (float angle = 10; angle < 360; angle += 10) {
        output.Append(createVertex(sphCenter, center, color));

        tangent = normalize(mul(tangent, TBN));

        output.Append(createVertex(sphCenter, center + tangent * radius, color));
    }
}