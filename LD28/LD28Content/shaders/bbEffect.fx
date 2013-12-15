//------- XNA interface --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float3 xCamPos;
float3 xAllowedRotDir;

//------- Texture Samplers --------
Texture xBillboardTexture;
sampler textureSampler = sampler_state { texture = <xBillboardTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = CLAMP; AddressV = CLAMP;};

struct BBVertexToPixel
{
	float4 Position : POSITION;
	float2 TexCoord	: TEXCOORD0;
	float4 ClipDistances : TEXCOORD1;
};

//------- Technique: CylBillboard --------
BBVertexToPixel CylBillboardVS(float3 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
	BBVertexToPixel Output = (BBVertexToPixel)0;	
	
	float3 center = mul(inPos, xWorld);
	float3 eyeVector = center - xCamPos;	
	
	float3 upVector = xAllowedRotDir;
	upVector = normalize(upVector);
	float3 sideVector = cross(eyeVector,upVector);
	sideVector = normalize(sideVector);
	
	float3 finalPosition = center;
	finalPosition += (inTexCoord.x*3.0f-1.5f)*sideVector;
	finalPosition += (1.5f-inTexCoord.y*3.0f)*upVector;	
	
	float4 finalPosition4 = float4(finalPosition, 1);
		
	float4x4 preViewProjection = mul (xView, xProjection);
	Output.Position = mul(finalPosition4, preViewProjection);
	
	Output.TexCoord = inTexCoord;
	
	return Output;
}

float4 BillboardPS(BBVertexToPixel PSIn) : COLOR0
{
	return tex2D(textureSampler, PSIn.TexCoord);
}

technique CylBillboard
{
	pass Pass0
    {          
    	VertexShader = compile vs_2_0 CylBillboardVS();
        PixelShader  = compile ps_2_0 BillboardPS();        
    }
}