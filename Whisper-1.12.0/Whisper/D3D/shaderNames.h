// This header is generated by a tool
#pragma once
#include <stdint.h>

namespace DirectCompute
{
	enum struct eComputeShader: uint16_t
	{
		add = 0,
		addInPlace = 1,
		addRepeat = 2,
		addRepeatEx = 3,
		addRepeatGelu = 4,
		addRepeatScale = 5,
		addRows = 6,
		convolutionMain = 7,
		convolutionMain2 = 8,
		convolutionMain2Fixed = 9,
		convolutionPrep1 = 10,
		convolutionPrep2 = 11,
		copyConvert = 12,
		copyTranspose = 13,
		dbgFindNaN = 14,
		diagMaskInf = 15,
		flashAttention = 16,
		flashAttentionCompat1 = 17,
		flashAttentionCompat2 = 18,
		flashAttentionCompat3 = 19,
		fmaRepeat1 = 20,
		fmaRepeat2 = 21,
		matReshapePanels = 22,
		mulMatByRow = 23,
		mulMatByRowTiled = 24,
		mulMatByRowTiledEx = 25,
		mulMatByScalar = 26,
		mulMatDotMain = 27,
		mulMatDotReshape = 28,
		mulMatMadMain = 29,
		mulMatTiled = 30,
		mulMatTiledEx = 31,
		norm = 32,
		normCompat = 33,
		normFixed = 34,
		scaleInPlace = 35,
		softMax = 36,
		softMaxCompat = 37,
		softMaxFixed = 38,
		softMaxLong = 39,
		zeroMemory = 40,
	};

	const char* computeShaderName( eComputeShader cs );
}