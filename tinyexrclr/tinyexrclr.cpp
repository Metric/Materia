#include "pch.h"
#include "tinyexrclr.h"
#include <malloc.h>

using namespace tinyexrclr;

int TinyExr::LoadExr(float** rgba, int* width, int* height, const char* filename, const char** err)
{
	return LoadEXR(rgba, width, height, filename, err);
}

void TinyExr::FreeData(void* mem)
{
	free(mem);
}

void TinyExr::FreeErrorMessage(const char* msg)
{
	FreeEXRErrorMessage(msg);
}
