struct GSInputLS
{
	float4 p	: POSITION;
	float4 wp   : POSITION1;
	float4 c	: COLOR;
};
struct PSInputLS
{
	float4 p	: SV_POSITION;
	float4 wp   : POSITION1;
	noperspective
		float3 t	: TEXCOORD;
	float4 c	: COLOR;
};

float4 vLineParams = float4(4, 0, 0, 0);
float4 vViewport = float4(960, 540, 0, 0);


//--------------------------------------------------------------------------------------
// From projection frame to window pixel pos.
//--------------------------------------------------------------------------------------
float2 projToWindow(in float4 pos)
{
	return float2(vViewport.x * 0.5 * (1.0 + (pos.x / pos.w)), vViewport.y * 0.5 * (1.0 - (pos.y / pos.w)));
}
//--------------------------------------------------------------------------------------
// From window pixel pos to projection frame at the specified z (view frame). 
//--------------------------------------------------------------------------------------
float4 windowToProj(in float2 pos, in float z, in float w)
{
	return float4(((pos.x * 2.0 / vViewport.x) - 1.0) * w,
		((pos.y * 2.0 / vViewport.y) - 1.0) * -w,
		z, w);
}
//--------------------------------------------------------------------------------------
// Make a a ribbon line of the specified pixel width from 2 points in the projection frame.
//--------------------------------------------------------------------------------------
void makeLine(out float4 points[4], in float4 posA, in float4 posB, in float width)
{
	width /= 2.0;

	// Bring A and B in window space
	float2 Aw = projToWindow(posA);
	float2 Bw = projToWindow(posB);

	// Compute tangent and binormal of line AB in window space
	// Binormal is scaled by line width 
	float2 tangent = normalize(Bw.xy - Aw.xy);
	float2 binormal = width * float2(tangent.y, -tangent.x);

	// Compute the corners of the ribbon in window space
	float2 A1w = (Aw - binormal);
	float2 A2w = (Aw + binormal);
	float2 B1w = (Bw - binormal);
	float2 B2w = (Bw + binormal);

	// bring back corners in projection frame
	points[0] = windowToProj(A1w, posA.z, posA.w);
	points[1] = windowToProj(A2w, posA.z, posA.w);
	points[2] = windowToProj(B1w, posB.z, posB.w);
	points[3] = windowToProj(B2w, posB.z, posB.w);
}

[maxvertexcount(4)]
void GShaderLines(line GSInputLS input[2], inout TriangleStream<PSInputLS> outStream)
{
	PSInputLS output = (PSInputLS)0;

	float4 lineCorners[4];
	makeLine(lineCorners, input[0].p, input[1].p, vLineParams.x);

	output.p = lineCorners[0];
	output.wp = input[0].wp;
	output.c = input[0].c;
	output.t[0] = +1;
	output.t[1] = +1;
	output.t[2] = 1;
	outStream.Append(output);

	output.p = lineCorners[1];
	output.wp = input[0].wp;
	output.c = input[0].c;
	output.t[0] = +1;
	output.t[1] = -1;
	output.t[2] = 1;
	outStream.Append(output);

	output.p = lineCorners[2];
	output.wp = input[1].wp;
	output.c = input[1].c;
	output.t[0] = -1;
	output.t[1] = +1;
	output.t[2] = 1;
	outStream.Append(output);

	output.p = lineCorners[3];
	output.wp = input[1].wp;
	output.c = input[1].c;
	output.t[0] = -1;
	output.t[1] = -1;
	output.t[2] = 1;
	outStream.Append(output);

	outStream.RestartStrip();
}