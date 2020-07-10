#pragma once

#include "../D3DLab.Cuda/Draw2dTexture.h"
#include "DX11GraphicsAdapter.h"

#include <d3d11.h>

using namespace D3DLab::CLI::SDX;

//using namespace DirectX;
using namespace System;
using namespace SharpDX;
using namespace SharpDX::Direct3D11;

namespace D3DLab {
	namespace CLI {
		namespace Toolkit {
			public ref class CudaTexture2DTechnique
			{

			public:
				CudaTexture2DTechnique();
				!CudaTexture2DTechnique();
				~CudaTexture2DTechnique();
				void Render(const DX11GraphicsAdapter^ graphics);

			private:
				D3DLab::Cuda::Draw2dTexture* texture2d;
			};
		}

	}
}

