#include  "Types.hlsli"


cbuffer Args0 : register(b0)
{//projection
	float4x4 p;
};

cbuffer Args1 : register(b1)
{//world view
	float4x4 wv;
}



VS_OUT_STANDARD main(VS_IN_CUBE input)
{
	VS_OUT_STANDARD output;

	output.pos = mul(mul(input.pos,wv),p);
	output.tex = input.texnorm;	

	return output;
}