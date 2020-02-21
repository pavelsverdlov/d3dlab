struct Light
{
    float4 Color;
    float4 Direction;
    float Intensity;
    float Type;
};
struct Material
{
    float4 ColorAmbient;
    float4 ColorDiffuse;
    float4 ColorSpecular;
    float4 ColorReflection;
    float SpecularFactor;    
};

/*
    BUFFERS
*/

cbuffer Game : register(b0)
{
    float4 LookDirection;
    float4 CameraPosF4;
	
    float4x4 View;
    float4x4 Projection;
};
cbuffer Lights : register(b1)
{
    Light lights[3];
}
cbuffer Transformation : register(b2)
{
    float4x4 World;
    float4x4 WorldInverse;
}
cbuffer MaterialBuff : register(b3)
{
    Material CurrentMaterial;
};

/*
    FUNCTIONS
*/

float4 toScreen(float3 v)
{
    float4 p = float4(v, 1);
    p = mul(View, p);
    p = mul(Projection, p);
    return p;
}
float4 toWVP(float4 position)
{
	// Change the position vector to be 4 units for proper matrix calculations.
    position.w = 1.0f;

    position = mul(position, World);
    position = mul(position, View);
    position = mul(position, Projection);

    return position;
}

float4 ComputePhongColor(float3 P, float4 N, Material mat)
{
    float4 traceRay = LookDirection;
    float3 finalColor = mat.ColorAmbient;
    float intensity = 0;
    for (int i = 0; i < 3; ++i)
    {
        Light l = lights[i];
        if (l.Type == 0)
        {
            continue;
        }
        if (l.Type == 1)
        { //ambient
            finalColor *= l.Intensity;
        }
        else
        {
            float4 L;
            if (l.Type == 2)
            { //point
               // L = l.LightPosV3 - P; TODO: IMPLEMENT POINT LIGHT
                L = l.Direction;
               // finalColor += l.Intensity * MaterialColorDiffuse;
            }
            else if (l.Type == 3)
            { //directional
                L = l.Direction;
            }
            float diffIntensity = dot(N, -L); //-L because N & L points out to inverted direction
            if (diffIntensity > 0)
            { //diffuse
                finalColor += l.Intensity * saturate(diffIntensity) * mat.ColorDiffuse.rgb;
            }
            
            if (mat.SpecularFactor > 0)
            { //specular
                float4 R = reflect(L, N);
                finalColor += pow(saturate(dot(N, R)), mat.SpecularFactor) * mat.ColorSpecular.rgb;
            }
        }
    }
    
    return float4(finalColor, mat.ColorDiffuse.a);
}