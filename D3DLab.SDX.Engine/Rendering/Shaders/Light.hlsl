struct Light {
    uint Type;
    float Intensity;
    float3 Position;   
    float3 Direction;
    float4 Color;
};

cbuffer Lights : register(b1) {
	Light lights[3];
}

float computeLight(float3 position, float3 normal, float3 traceRay, float specular) {
	float intensity = 0.0;
	for (int i = 0; i < 3; ++i) {
		Light l = lights[i];
		if (l.Type == 0) {
			continue;
		}
		if (l.Type == 1) { //ambient
			intensity += l.Intensity;
		} else {
			float3 lightDir;
			if (l.Type == 2) { //point
				lightDir = l.Position - position;
			}
			else if (l.Type == 3) { //directional
				lightDir = l.Direction;
			}
			float dotval = dot(normal, lightDir);
			if (dotval > 0) {//diffuse
				intensity += l.Intensity * dotval / (length(normal) * length(lightDir));
			}
			//specular

		}
	}
	return intensity;
}