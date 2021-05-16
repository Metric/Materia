#pragma once

using namespace System;

namespace tinyexrclr {
	public ref class TinyExr
	{
	public:
		static int LoadExr(float** rgba, int* width, int* height, const char* filename, const char** err);
		static void FreeData(void* mem);
		static void FreeErrorMessage(const char* msg);
	};
}
