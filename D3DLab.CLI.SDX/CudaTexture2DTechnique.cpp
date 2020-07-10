#include "../D3DLab.Cuda/Draw2dTexture.h"

#include "CudaTexture2DTechnique.h"

#include <stdio.h>

using namespace D3DLab;
using namespace CLI;
using namespace Toolkit;

CudaTexture2DTechnique::CudaTexture2DTechnique()
{
	texture2d = new D3DLab::Cuda::Draw2dTexture();
}

D3DLab::CLI::Toolkit::CudaTexture2DTechnique::!CudaTexture2DTechnique()
{
	if (texture2d)
		delete texture2d;
}

D3DLab::CLI::Toolkit::CudaTexture2DTechnique::~CudaTexture2DTechnique()
{
	this->!CudaTexture2DTechnique();
}

void D3DLab::CLI::Toolkit::CudaTexture2DTechnique::Render(const DX11GraphicsAdapter^ graphics)
{
	texture2d->CreateTexture(graphics->deviceRef);
	texture2d->RunCuda(graphics->deviceRef);

}

