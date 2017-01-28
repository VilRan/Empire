#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 WorldViewProjection;
float4x4 WorldInverseTranspose;
float4 AmbientIntensity;
float4 DiffuseColor;
float3 LightDirection;
float Shininess;
float4 SpecularColor;
float3 ViewDirection;

struct VertexShaderInput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float3 Normal : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);

	float4 normal = mul(input.Normal, WorldInverseTranspose);
	output.Normal = normal;

	float lightIntensity = dot(normal, LightDirection);
	lightIntensity = max(AmbientIntensity, lightIntensity);
	output.Color = saturate(input.Color * DiffuseColor * lightIntensity);

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float3 light = LightDirection;
	float3 normal = input.Normal;
	float3 r = normalize(2 * dot(light, normal) * normal - light);
	float3 v = normalize(mul(ViewDirection, World));

	float dotProduct = dot(r, v);
	float4 specular = SpecularColor * max(pow(dotProduct, Shininess), 0) * length(input.Color);

	return saturate(input.Color + specular);
}

technique PlanetShader
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};