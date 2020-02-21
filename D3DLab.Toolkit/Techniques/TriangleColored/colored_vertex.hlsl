@vertex@

#include "Common"

struct VSOut
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
};


VSOut main(float4 position : POSITION, float3 normal : NORMAL)
{
    VSOut output = (VSOut) 0;

    output.position = toWVP(position);

    output.normal = mul(normal, World);
    output.normal = normalize(output.normal);

    return output;
}

@fragment@

#include "Common"

float4 ComputePhongColor1(float3 P, float4 N, Material mat)
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

struct PSIn
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
};
float4 main(PSIn input, bool isFront : SV_IsFrontFace) : SV_TARGET
{
    float4 normal = input.normal * (1 - isFront * 2);
    float4 color = ComputePhongColor1(input.position.xyz, normal, CurrentMaterial);
    
    return color;
}

