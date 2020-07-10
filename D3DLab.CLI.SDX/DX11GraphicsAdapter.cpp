#include "../D3DLab.Cuda/Draw2dTexture.h"

#include "DX11GraphicsAdapter.h"

#include <stdio.h>

using namespace D3DLab;
using namespace CLI;
using namespace SDX;


DX11GraphicsAdapter::DX11GraphicsAdapter(Device^ device)
{
	IntPtr ptr = device->NativePointer;
	deviceRef = reinterpret_cast<ID3D11Device*>(ptr.ToPointer());

	D3D_FEATURE_LEVEL level = deviceRef->GetFeatureLevel();

	//sdxDevice = device;

	//D3DLab:Cuda::Draw2dTexture* test = new D3DLab::Cuda::Draw2dTexture();
	/*if (FAILED(test->CreateTexture(deviceRef))) {
		printf("FAILED!");
	}
	else {
		printf("OK!");
	}*/
}

D3DLab::CLI::SDX::DX11GraphicsAdapter::!DX11GraphicsAdapter()
{
	/*if (deviceRef) {
		delete deviceRef;
		deviceRef = nullptr;
	}*/
	/*if (sdxDevice) {
		delete sdxDevice;
		sdxDevice = nullptr;
	}*/
}

D3DLab::CLI::SDX::DX11GraphicsAdapter::~DX11GraphicsAdapter()
{
	this->!DX11GraphicsAdapter();
}
