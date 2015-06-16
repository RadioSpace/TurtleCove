
#if !defined(__TYPES)
#define __TYPES


//vertex used for rendering a simple quad
struct VS_IN_QUAD
{
	float4 pos : POSITION0;
	float2 tex : TEXCOORD0;
};

//vertex used for rendering a cube
struct VS_IN_CUBE
{
	float4 pos : POSITION0;
	float2 texnorm : TEXCOORD0;
	float2 texbi : TEXCOORD1;
	float2 textan : TEXCOORD2;
};

//vertex used for rendering without a vertex buffer
struct VS_IN_INDEX
{
	uint v_id : SV_VertexID;
};

//standard input for a pixel shader
struct VS_OUT_STANDARD
{
	float4 pos : SV_POSITION;	
	float2 tex : TEXCOORD;
};

//for rendering without a deppth buffer
struct PS_OUT_TARGET
{
	float4 target : SV_TARGET;
};

//for rendering with a depth buffer
struct PS_OUT_TARGETDEPTH
{
	float4 target : SV_TARGET;
	float4 depth : SV_DEPTH;
};

//for deferred rendering with a depth buffer
struct PS_OUT_2TARGETDEPTH
{
	float4 target0 : SV_TARGET0;
	float4 target1 : SV_TARGET1;

	float4 depth : SV_DEPTH;
};
//for deferred rendering with a depth buffer
struct PS_OUT_3TARGETDEPTH
{
	float4 target0 : SV_TARGET0;
	float4 target1 : SV_TARGET1;
	float4 target2 : SV_TARGET2;

	float4 depth : SV_DEPTH;
};
//for deferred rendering with a depth buffer
struct PS_OUT_4TARGETDEPTH
{
	float4 target0 : SV_TARGET0;
	float4 target1 : SV_TARGET1;
	float4 target2 : SV_TARGET2;
	float4 target3 : SV_TARGET3;

	float4 depth : SV_DEPTH;
};



//for deferred rendering without a depth buffer
struct PS_OUT_2TARGET
{
	float4 target0 : SV_TARGET0;
	float4 target1 : SV_TARGET1;


};
//for deferred rendering without a depth buffer
struct PS_OUT_3TARGET
{
	float4 target0 : SV_TARGET0;
	float4 target1 : SV_TARGET1;
	float4 target2 : SV_TARGET2;

};
//for deferred rendering without a depth buffer
struct PS_OUT_4TARGET
{
	float4 target0 : SV_TARGET0;
	float4 target1 : SV_TARGET1;
	float4 target2 : SV_TARGET2;
	float4 target3 : SV_TARGET3;
};
#endif