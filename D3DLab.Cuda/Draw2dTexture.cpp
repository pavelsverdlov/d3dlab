#include "Draw2dTexture.h"



#include <cuda_runtime_api.h>
#include <cuda_d3d11_interop.h>
#include <d3dcompiler.h>
#include <stdio.h>

using namespace D3DLab;
using namespace Cuda;

struct ConstantBuffer
{
	float   vQuadRect[4];
	int     UseCase;
};
struct
{
	ID3D11Texture2D* pTexture;
	ID3D11ShaderResourceView* pSRView;
	cudaGraphicsResource* cudaResource;
	void* cudaLinearMemory;
	size_t                  pitch;
	int                     width;
	int                     height;
} g_texture_2d;


extern "C"
{
	bool cuda_texture_2d(void* surface, size_t width, size_t height, size_t pitch, float t);
	cudaError_t addWithCuda(int* c, const int* a, const int* b, unsigned int size);
}


bool  D3DLab::Cuda::Draw2dTexture::RunCuda(ID3D11Device* d3dDevice)
{
	cudaStream_t stream = 0;
	const int nbResources = 1;
	cudaGraphicsResource* ppResources[nbResources] =
	{
		g_texture_2d.cudaResource
	};
	cudaGraphicsMapResources(nbResources, ppResources, stream);

	RunKernels();

	// unmap the resources
	cudaGraphicsUnmapResources(nbResources, ppResources, stream);
	
	ID3D11DeviceContext* d3dContext;
	d3dDevice->GetImmediateContext(&d3dContext);

	d3dContext->AddRef();
	d3dContext->PSSetShaderResources(0, 1, &g_texture_2d.pSRView);
	d3dContext->Release();

	return true;
}

HRESULT D3DLab::Cuda::Draw2dTexture::CreateTexture(ID3D11Device* d3dDevice)
{
	if(!textureCreated) {
		g_texture_2d.width = 256;
		g_texture_2d.height = 256;

		D3D11_TEXTURE2D_DESC desc;
		ZeroMemory(&desc, sizeof(D3D11_TEXTURE2D_DESC));
		desc.Width = g_texture_2d.width;
		desc.Height = g_texture_2d.height;
		desc.MipLevels = 1;
		desc.ArraySize = 1;
		desc.Format = DXGI_FORMAT_R32G32B32A32_FLOAT;
		desc.SampleDesc.Count = 1;
		desc.Usage = D3D11_USAGE_DEFAULT;
		desc.BindFlags = D3D11_BIND_SHADER_RESOURCE;

		if (FAILED(d3dDevice->CreateTexture2D(&desc, NULL, &g_texture_2d.pTexture)))
		{
			return E_FAIL;
		}

		if (FAILED(d3dDevice->CreateShaderResourceView(g_texture_2d.pTexture, NULL, &g_texture_2d.pSRView)))
		{
			return E_FAIL;
		}

		this->textureCreated = true;
	}

	

	return S_OK;
}

bool D3DLab::Cuda::Draw2dTexture::CalcKernels() {
	const int arraySize = 5;
	const int a[arraySize] = { 1, 2, 3, 4, 5 };
	const int b[arraySize] = { 10, 20, 30, 40, 50 };
	int c[arraySize] = { 0 };

	// Add vectors in parallel.
	cudaError_t cudaStatus = addWithCuda(c, a, b, arraySize);
	if (cudaStatus != cudaSuccess) {
		//fprintf(stderr, "addWithCuda failed!");
		return false;
	}

	printf("{1,2,3,4,5} + {10,20,30,40,50} = {%d,%d,%d,%d,%d}\n",
		 c[0], c[1], c[2], c[3], c[4]);

		 // cudaDeviceReset must be called before exiting in order for profiling and
		 // tracing tools such as Nsight and Visual Profiler to show complete traces.
	//cudaStatus = cudaDeviceReset();
	//if (cudaStatus != cudaSuccess) {
	//    //fprintf(stderr, "cudaDeviceReset failed!");
	//    return false;
	//}

	return true;
}

bool D3DLab::Cuda::Draw2dTexture::RunKernels()
{
	static float t = 0.0f;

	cudaArray* cuArray;
	cudaGraphicsSubResourceGetMappedArray(&cuArray, g_texture_2d.cudaResource, 0, 0);
	//getLastCudaError("cudaGraphicsSubResourceGetMappedArray (cuda_texture_2d) failed");

	// kick off the kernel and send the staging buffer cudaLinearMemory as an argument to allow the kernel to write to it
	cuda_texture_2d(g_texture_2d.cudaLinearMemory, g_texture_2d.width, g_texture_2d.height, g_texture_2d.pitch, t);
	//getLastCudaError("cuda_texture_2d failed");

	// then we want to copy cudaLinearMemory to the D3D texture, via its mapped form : cudaArray
	cudaMemcpy2DToArray(
		cuArray, // dst array
		0, 0,    // offset
		g_texture_2d.cudaLinearMemory, g_texture_2d.pitch,       // src
		g_texture_2d.width * 4 * sizeof(float), g_texture_2d.height, // extent
		cudaMemcpyDeviceToDevice); // kind
   // getLastCudaError("cudaMemcpy2DToArray failed");

	t += 0.1f;

	//fprintf(stdout, "Assert unsatisfied in %s at %s:%d\n", __FUNCTION__, __FILE__, __LINE__); 
	//OutputDebugStringW(L"RunKernels");
	//TRACE();
	return true;
}