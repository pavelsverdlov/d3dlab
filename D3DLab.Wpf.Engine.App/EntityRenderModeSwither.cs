using D3DLab.Std.Engine;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
using D3DLab.Std.Engine.Render;
using D3DLab.Std.Engine.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Wpf.Engine.App {
    public class EntityRenderModeSwither {

        #region shader

        const string VertexShaderText =
@"
struct VSInputLS
{
	float4 p	: POSITION0;
	float4 c	: COLOR0;
};

struct GSInputLS
{
	float4 p	: POSITION;
	float4 wp   : POSITION1;
	float4 c	: COLOR;
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
GSInputLS VShaderBoundingBox(VSInputLS input)
{
	GSInputLS output;
	float4 inputp = input.p;

	output.p = mul(World, inputp);
    output.wp = output.p;
	output.p = mul(View, output.p);
	output.p = mul(Projection, output.p);
	output.c = input.c;

	return output;
}";
        const string PixelShaderText =
@"
struct PSInputLS
{
	float4 p	: SV_POSITION;
	float4 wp   : POSITION1;
	noperspective
		float3 t	: TEXCOORD;
	float4 c	: COLOR;
};

float4 vLineParams = float4(4, 1, 0, 0);

float4 PShaderBoundingBox(PSInputLS input) : SV_Target
{
	return input.c;
/*
    // Compute distance of the fragment to the edges    
	//float dist = min(abs(input.t[0]), abs(input.t[1]));	
	float dist = abs(input.t.y);
	// Cull fragments too far from the edge.
	//if (dist > 0.5*vLineParams.x+1) discard;

	// Map the computed distance to the [0,2] range on the border of the line.
	//dist = clamp((dist - (0.5*vLineParams.x - 1)), 0, 2);

	// Alpha is computed from the function exp2(-2(x)^2).
	float sigma = 2.0f / (vLineParams.y + 1e-6);
	dist *= dist;
	float alpha = exp2(-2 * dist / sigma);

	//if(alpha<0.1) discard;

	// Standard wire color
	float4 color = input.c;

	//color = texDiffuseMap.Sample(SSLinearSamplerWrap, input.t.xy);	
	color.a = alpha;
	*/
	return color;
}
";
        const string GeometryShaderText =
@"
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

[maxvertexcount(4)]
void GShaderLines(line GSInputLS input[2], inout TriangleStream<PSInputLS> outStream)
{



}";
        #endregion

        static ShaderTechniquePass GetBoundingBoxTechnique() {
            var name = "BoundingBoxShader";

            var compiler = new D3DShaderCompilator(null);
            
            var vertex = new ShaderInMemoryInfo(name + "Vertex", VertexShaderText, 
                compiler.Compile(VertexShaderText, "", ""), "", "");
            var pixel = new ShaderInMemoryInfo(name + "Pixel", PixelShaderText, 
                compiler.Compile(PixelShaderText, "", ""), "", "");
            var geometry = new ShaderInMemoryInfo(name + "Geometry", GeometryShaderText,
                compiler.Compile(GeometryShaderText, "", ""), "", "");

            return new ShaderTechniquePass(new IShaderInfo[] {vertex , pixel } );
        }

        public enum Modes {
            BoundingBox,
            PointsCloud,
        }

        readonly GraphicEntity entity;

        public EntityRenderModeSwither(GraphicEntity entity) {
            this.entity = entity;
        }

        public void TurnOn(Modes mode) {
            entity.AddComponent(new BoundingBoxRenderableComponent(GetBoundingBoxTechnique()));
        }

        public void TurnOff(Modes mode) {
            entity.RemoveComponentsOfType<BoundingBoxRenderableComponent>();
        }
    }
}
