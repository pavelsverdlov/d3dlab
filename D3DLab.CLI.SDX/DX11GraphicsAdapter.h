#pragma once

#include <d3d11.h>

//using namespace DirectX;
using namespace System;
using namespace SharpDX;
using namespace SharpDX::Direct3D11;

namespace D3DLab {
	namespace CLI {
		namespace SDX {
			public ref class DX11GraphicsAdapter
			{
			public:
				ID3D11Device* deviceRef;

				DX11GraphicsAdapter(Device^ device);
				!DX11GraphicsAdapter();
				~DX11GraphicsAdapter();
			
			};
		}
	}
}

