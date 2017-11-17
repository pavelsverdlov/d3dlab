using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using D3DLab.Helix.Properties;
using global::SharpDX;
using global::SharpDX.D3DCompiler;
using global::SharpDX.Direct3D;
using global::SharpDX.Direct3D11;
using HelixToolkit.Wpf.SharpDX.WinForms;
using Adapter = SharpDX.DXGI.Adapter;
using Format = global::SharpDX.DXGI.Format;

namespace HelixToolkit.Wpf.SharpDX
{
	public sealed class EffectsManager : IDisposable
	{
		private const string STR_Layout = "Layout";

		public EffectsManager(Device device)
		{
			this.device = device;
			this.InitEffects();
		}

		private global::SharpDX.Direct3D11.Device device;

		private Dictionary<string, object> data = new Dictionary<string, object>();

		~EffectsManager()
		{
			this.Dispose();
		}

		private void InitEffects()
		{
			try {
			    var def = File.ReadAllText(@"C:\Storage\projects\sv\D3DLab\D3DLab.Helix\Shaders\Default.fx"); 
                // ------------------------------------------------------------------------------------
                RegisterEffect(def, new[] 
	                { 
	                    Techniques.RenderPhong, 
                        Techniques.RenderPhongPointSampler,
						Techniques.RenderPhongColorByBox,
	                    Techniques.RenderBlinn, 
	                    Techniques.RenderCubeMap, 
	                    Techniques.RenderColors, 
	                    Techniques.RenderWires,
						Techniques.RenderVertex,
	                    Techniques.RenderLines,  
						Techniques.RenderBackground,
						Techniques.RenderPhongWithAmbient,
                        //custom
						Techniques.RenderPhongWithSphereColoring,
                        Techniques.RenderPhongColorByOutOfBox,
	                });

				// ------------------------------------------------------------------------------------                
				var cubeMapInputLayout = new InputLayout(device,
						GetShaderBytecode(Techniques.RenderCubeMap),
					new[] 
	                { 
							new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
	                });

				RegisterLayout(new[] 
	                { 
	                    Techniques.RenderCubeMap, 
	                }, cubeMapInputLayout);

				// ------------------------------------------------------------------------------------
				var linesInputLayout = new InputLayout(device,
					GetShaderBytecode(Techniques.RenderLines),
					new[] 
	                {
	                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
	                    new InputElement("COLOR",    0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
	           
	                    //INSTANCING: die 4 texcoords sind die matrix, die mit jedem buffer reinwandern
	                    new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),                 
	                    new InputElement("TEXCOORD", 2, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
	                    new InputElement("TEXCOORD", 3, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
	                    new InputElement("TEXCOORD", 4, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
	                });
				RegisterLayout(new[] 
	                { 
	                    Techniques.RenderLines 
	                }, linesInputLayout);

				// ------------------------------------------------------------------------------------
				var defaultInputLayout = new InputLayout(device,
					GetShaderBytecode(Techniques.RenderPhong),
					new[]
	                {
	                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
	                    new InputElement("COLOR",    0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
	                    new InputElement("TEXCOORD", 0, Format.R32G32_Float,       InputElement.AppendAligned, 0),
	                    new InputElement("NORMAL",   0, Format.R32G32B32_Float,    InputElement.AppendAligned, 0),
	                    new InputElement("TANGENT",  0, Format.R32G32B32_Float,    InputElement.AppendAligned, 0),
	                    new InputElement("BINORMAL", 0, Format.R32G32B32_Float,    InputElement.AppendAligned, 0),

	                    //INSTANCING: die 4 texcoords sind die matrix, die mit jedem buffer reinwandern
	                    new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),                 
	                    new InputElement("TEXCOORD", 2, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
	                    new InputElement("TEXCOORD", 3, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
	                    new InputElement("TEXCOORD", 4, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
	                });
				RegisterLayout(new[] 
	                { 
	                    // put here techniques which use the vertex layout below
	                    Techniques.RenderPhong, 
	                    Techniques.RenderBlinn,
	                    Techniques.RenderPhongPointSampler,  
 						Techniques.RenderPhongColorByBox,
	                    Techniques.RenderColors, 
	                    Techniques.RenderWires, 
						Techniques.RenderVertex,
						Techniques.RenderBackground, 
						Techniques.RenderPhongWithAmbient,
                        //custom
						Techniques.RenderPhongWithSphereColoring,
                        Techniques.RenderPhongColorByOutOfBox,
	                }, defaultInputLayout);
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(string.Format("Error registering effect: {0}", ex.Message), "Error");
				throw;
			}
		}

		private ShaderBytecode GetShaderBytecode(RenderTechnique renderTechnique)
		{
			return GetEffect(renderTechnique)
				.GetTechniqueByName(renderTechnique.Name)
				.GetPassByIndex(0)
				.Description
				.Signature;
		}

		public static void CompileShaders()
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="shaderEffectString"></param>
		/// <param name="techniqueNames"></param>
		/// <param name="sFlags"></param>
		/// <param name="eFlags"></param>
		internal void RegisterEffect(string shaderEffectString, RenderTechnique[] techniqueNames, ShaderFlags sFlags = ShaderFlags.None, EffectFlags eFlags = EffectFlags.None)
		{
#if PRECOMPILED_SHADERS

	            try
	            {
	                var shaderBytes = Techniques.TechniquesSourceDict[techniqueNames[0]];
	                this.RegisterEffect(shaderBytes, techniqueNames);
	            }
	            catch (Exception ex)
	            {
	                System.Windows.MessageBox.Show(string.Format("Error registering effect: {0}", ex.Message), "Error");
	            }
#else

#if DEBUG
			sFlags |= ShaderFlags.Debug;
			eFlags |= EffectFlags.None;
#else
	            sFlags |= ShaderFlags.OptimizationLevel3;
	            eFlags |= EffectFlags.None;       
#endif

			var filePath = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
			filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filePath);
			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			var precompileFileName = shaderEffectString.GetHashCode() + ".fx_bin";

			filePath = Path.Combine(filePath, precompileFileName);

			if (File.Exists(filePath))
			{
				var shaderBytes = new ShaderBytecode(File.ReadAllBytes(filePath));
				this.RegisterEffect(shaderBytes, techniqueNames);
			}
			else
			{
				var preprocess = ShaderBytecode.Preprocess(shaderEffectString, new ShaderMacro[0], new IncludeHandler());
				var hashCode = preprocess.GetHashCode();
				string fileName = hashCode.ToString();
				if (File.Exists(fileName))
				{
					try
					{
						var shaderBytes = ShaderBytecode.FromFile(fileName);
						this.RegisterEffect(shaderBytes, techniqueNames);
						File.WriteAllBytes(filePath, shaderBytes.Data);
					}
					finally
					{
						File.Delete(fileName);
					}
				}
				else
				{
					try
					{
						var shaderBytes = ShaderBytecode.Compile(preprocess, "fx_5_0", sFlags, eFlags);
						shaderBytes.Bytecode.Save(hashCode.ToString());
						this.RegisterEffect(shaderBytes.Bytecode, techniqueNames);
						File.WriteAllBytes(filePath, shaderBytes.Bytecode.Data);
					}
					catch (Exception ex)
					{
						System.Windows.MessageBox.Show(string.Format("Error compiling effect: {0}", ex.Message), "Error");
						throw;
					}
				}
			}
#endif
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="shaderEffectBytecode"></param>
		/// <param name="techniques"></param>
		/// <param name="eFlags"></param>
		internal void RegisterEffect(byte[] shaderEffectBytecode, RenderTechnique[] techniques, EffectFlags eFlags = EffectFlags.None)
		{
			var effect = new Effect(device, shaderEffectBytecode, eFlags);
			//effect.GetTechniqueByIndex()
			foreach (var tech in techniques)
				data[tech.Name] = effect;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="technique"></param>
		/// <param name="layout"></param>
		internal void RegisterLayout(RenderTechnique technique, InputLayout layout)
		{
			data[technique.Name + STR_Layout] = layout;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="techniques"></param>
		/// <param name="layout"></param>
		internal void RegisterLayout(RenderTechnique[] techniques, InputLayout layout)
		{
			foreach (var tech in techniques)
				RegisterLayout(tech, layout);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="technique"></param>
		/// <returns></returns>
		public Effect GetEffect(RenderTechnique technique)
		{
			return (Effect)data[technique.Name];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="technique"></param>
		/// <returns></returns>
		public InputLayout GetLayout(RenderTechnique technique)
		{
			return (InputLayout)data[technique.Name + STR_Layout];
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			if (data != null)
			{
				foreach (var item in data.OfType<IDisposable>())
					item.Dispose();
				this.data.Clear();
				this.data = null;
			}
			this.device = null;
		}

		/// <summary>
		/// 
		/// </summary>
		private class IncludeHandler : Include, ICallbackable, IDisposable
		{
			public void Close(Stream stream)
			{
				stream.Close();
			}

			public Stream Open(IncludeType type, string fileName, Stream parentStream)
			{
//                throw new NotImplementedException();
			    var name = Path.GetFileNameWithoutExtension(fileName);
                var codeString =  File.ReadAllText(@"C:\Storage\projects\sv\D3DLab\D3DLab.Helix\Shaders\" + name+ ".fx");//Resources.ResourceManager.GetString(name, System.Globalization.CultureInfo.InvariantCulture);

				MemoryStream stream = new MemoryStream();
				StreamWriter writer = new StreamWriter(stream);
				writer.Write(codeString);
				writer.Flush();
				stream.Position = 0;
				return stream;
			}

			public IDisposable Shadow
			{
				get
				{
					return this.stream;
				}
				set
				{
					if (this.stream != null)
						this.stream.Dispose();
					this.stream = value as Stream;
				}
			}

			public void Dispose()
			{
				stream.Dispose();
			}

			private Stream stream;
		}
	}
}
