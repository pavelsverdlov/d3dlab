#pragma once

#include <d3d11.h>

namespace D3DLab {
	namespace Cuda {

		class Draw2dTexture
		{
		public:
			HRESULT CreateTexture(ID3D11Device* d3dDevice);
						
			bool RunCuda(ID3D11Device* d3dDevice);

		private:
			bool textureCreated;
			bool RunKernels();
			bool CalcKernels();
		};
	}
}
