using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using global::SharpDX;
using global::SharpDX.D3DCompiler;
using global::SharpDX.Direct3D;
using global::SharpDX.Direct3D11;
using global::SharpDX.DXGI;
using HelixToolkit.Wpf.SharpDX.Controls.CustomTechniques;

namespace HelixToolkit.Wpf.SharpDX
{
	public sealed class Techniques
	{
		static Techniques()
		{
			/// <summary>
			/// Names of techniques which are implemented by default
			/// </summary>
			RenderBlinn = new RenderTechnique("RenderBlinn");
			RenderPhong = new RenderTechnique("RenderPhong");
            RenderPhongPointSampler = new RenderTechnique("RenderPhongPointSampler");

			RenderPhongColorByBox = new RenderPhongColorByBox("RenderPhongColorByBox");
            RenderPhongColorByOutOfBox = new PhongColorByOutOfBoxRenderTechnique();

			RenderBackground = new RenderTechnique("RenderBackground");
			//RenderDiffuse = new RenderTechnique("RenderDiffuse");
			RenderColors = new RenderTechnique("RenderColors");
			//RenderPositions = new RenderTechnique("RenderPositions");
			//RenderNormals = new RenderTechnique("RenderNormals");
			//RenderPerturbedNormals = new RenderTechnique("RenderPerturbedNormals");
			//RenderTangents = new RenderTechnique("RenderTangents");
			//RenderTexCoords = new RenderTechnique("RenderTexCoords");
			RenderWires = new RenderTechnique("RenderWires");
			RenderVertex = new RenderTechnique("RenderVertex");
			RenderPhongWithAmbient = new RenderTechnique("RenderPhongWithAmbient");
			RenderPhongWithSphereColoring = new RenderTechnique("RenderPhongWithSphereColoring");

#if DEFERRED
				RenderDeferred = new RenderTechnique("RenderDeferred");
				RenderGBuffer = new RenderTechnique("RenderGBuffer");
				RenderDeferredLighting = new RenderTechnique("RenderDeferredLighting");
				RenderScreenSpace = new RenderTechnique("RenderScreenSpace");
#endif

#if TESSELLATION
				RenderPNTriangs = new RenderTechnique("RenderPNTriangs");
				RenderPNQuads = new RenderTechnique("RenderPNQuads");
#endif
			RenderCubeMap = new RenderTechnique("RenderCubeMap");
			RenderLines = new RenderTechnique("RenderLines");

			RenderTechniques = new List<RenderTechnique>
	            { 
	                RenderBlinn,
	                RenderPhong, 
                    RenderPhongPointSampler,
					RenderPhongColorByBox,
                    RenderPhongColorByOutOfBox,
					RenderBackground,
					RenderPhongWithAmbient,
					RenderPhongWithSphereColoring,
	                RenderColors,
	                //RenderDiffuse,
	                //RenderPositions,
	                //RenderNormals,
	                //RenderPerturbedNormals,
	                //RenderTangents, 
	                //RenderTexCoords,
	                RenderWires,
					RenderVertex,
	#if DEFERRED
	                RenderDeferred,
	                RenderGBuffer,  
	#endif
	                
	#if TESSELLATION 
	                RenderPNTriangs,
	                RenderPNQuads,
	#endif
	            };

		    var _default = File.ReadAllBytes(@"C:\Storage\projects\sv\D3DLab\D3DLab.Helix\Resources\_default.bfx");


            TechniquesSourceDict = new Dictionary<RenderTechnique, byte[]>()
	            {
	                {     Techniques.RenderPhong,      _default}, 
                    {     Techniques.RenderPhongPointSampler,      _default}, 
					 {     Techniques.RenderPhongColorByBox,      _default}, 
	                {     Techniques.RenderBlinn,      _default}, 
	                {     Techniques.RenderCubeMap,    _default}, 
	                {     Techniques.RenderColors,     _default}, 
	                //{     Techniques.RenderDiffuse,    _default}, 
	                //{     Techniques.RenderPositions,  _default}, 
	                //{     Techniques.RenderNormals,    _default},
	                //{     Techniques.RenderPerturbedNormals,    _default}, 
	                //{     Techniques.RenderTangents,   _default}, 
	                //{     Techniques.RenderTexCoords,  _default}, 
	                {     Techniques.RenderWires,      _default}, 
					{     Techniques.RenderVertex,     _default}, 
	                {     Techniques.RenderLines,      _default}, 
	    #if TESSELLATION                                        
	                {     Techniques.RenderPNTriangs,  _default}, 
	                {     Techniques.RenderPNQuads,    _default}, 
	    #endif 
	    #if DEFERRED            
	                {     Techniques.RenderDeferred,   Properties.Resources._deferred},
	                {     Techniques.RenderGBuffer,    Properties.Resources._deferred},
	                {     Techniques.RenderDeferredLighting , Properties.Resources._deferred},
	    #endif
	            };
            
		}



		internal static readonly Techniques Instance = new Techniques();

		internal static readonly Dictionary<RenderTechnique, byte[]> TechniquesSourceDict;

		private Techniques()
		{

		}

		/// <summary>
		/// Names of techniques which are implemented by default
		/// </summary>
		public static RenderTechnique RenderBlinn { get; private set; }// = new RenderTechnique("RenderBlinn");
		public static RenderTechnique RenderPhong { get; private set; }

		public static RenderTechnique RenderPhongColorByBox { get; private set; }
        /// <summary>
        /// Technique for detecting objects that out of established bounding box
        /// </summary>
        public static RenderTechnique RenderPhongColorByOutOfBox { get; private set; }
		public static RenderTechnique RenderBackground { get; private set; }
        public static RenderTechnique RenderPhongPointSampler { get; private set; }

		public static RenderTechnique RenderPhongWithAmbient { get; set; }
		public static RenderTechnique RenderPhongWithSphereColoring { get; set; }

		//public static RenderTechnique RenderDiffuse { get; private set; }
		public static RenderTechnique RenderColors { get; private set; }
		//public static RenderTechnique RenderPositions { get; private set; }
		//public static RenderTechnique RenderNormals { get; private set; }
		//public static RenderTechnique RenderPerturbedNormals { get; private set; }
		//public static RenderTechnique RenderTangents { get; private set; }
		//public static RenderTechnique RenderTexCoords { get; private set; }
		public static RenderTechnique RenderWires { get; private set; }
		public static RenderTechnique RenderVertex { get; private set; }
		public static RenderTechnique RenderCubeMap { get; private set; }
		public static RenderTechnique RenderLines { get; private set; }

#if TESSELLATION
			public static RenderTechnique RenderPNTriangs { get; private set; }
			public static RenderTechnique RenderPNQuads { get; private set; }
#endif

#if DEFERRED
			public static RenderTechnique RenderDeferred { get; private set; }
			public static RenderTechnique RenderGBuffer { get; private set; }
			public static RenderTechnique RenderDeferredLighting { get; private set; }
			public static RenderTechnique RenderScreenSpace { get; private set; }
#endif
		public static IEnumerable<RenderTechnique> RenderTechniques { get; private set; }
	}
}
