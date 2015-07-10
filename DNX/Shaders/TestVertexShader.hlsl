//test linkage
/*
#include "Types.hlsli"

IShape shape;

cbuffer args : register(b0)
{
	float3 campos;
	bool facist;

};

struct VS_OUT
{
	float4 pos:SV_POSITION;
	float3 tex:TEXCOORD;
};

VS_OUT main( uint v_id : SV_VertexID )
{
	POSTEX info = shape.getPOSTEX(v_id,1);

	VS_OUT output;
	output.pos = info.pos;
	output.tex = info.tex;
	

	return output;
}
*/





#include  "Types.hlsli"



cbuffer SimpleArgs : register(b0)
{//projection
	float4x4 p;
	dword dickhead;

};

cbuffer DifferentArgs : register(b1)
{//world view
	float4x4 wv;
	vector<int,4> e;
	unorm float ufl;
	snorm float sfl;
	float ffl;

};

cbuffer looploop: register(b2)
{
	float scale;
	float buttScale;
	float4 pos;
	float4 tex;
	float3 pos3D;
};




VS_OUT_STANDARD main(VS_IN_QUAD input)
{
	VS_OUT_STANDARD output;


	output.pos = mul(mul(input.pos,wv),p);
	output.tex = input.tex;	

	return output;
}



/*
#include  "Types.hlsli"


cbuffer Args0 : register(b0)
{//projection
	float4x4 p;
};

cbuffer Args1 : register(b1)
{//world view
	float4x4 wv;

}

struct VS_IN_TEST
{
	float a1 : POSITION0;
	float2 a2 : POSITION1;
	float3 a3 : POSITION2;
	float4 a4 : POSITION3;

	int b1 : TEXCOORD0;
	int2 b2 : TEXCOORD1;
	int3 b3 : TEXCOORD2;
	int4 b4 : TEXCOORD3;

	uint c1 : TEXCOORD4;
	uint2 c2 : TEXCOORD5;
	uint3 c3 : TEXCOORD6;
	uint4 c4 : TEXCOORD7;
};

VS_OUT_STANDARD main(VS_IN_TEST input)
{
	VS_OUT_STANDARD output;
	

	output.pos = 0;
	output.tex = 0;
	

	return output;
}
*/