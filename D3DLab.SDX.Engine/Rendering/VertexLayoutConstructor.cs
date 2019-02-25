using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.SDX.Engine.Rendering {
    public class VertexLayoutConstructor {
        const string SemanticPositionName = "POSITION";
        const string SemanticNormalName = "NORMAL";
        const string SemanticColorName = "COLOR";
        const string SemanticTexCoorName = "TEXCOORD";
        const InputClassification perverxdata = InputClassification.PerVertexData;
        const Format Vector3 = Format.R32G32B32_Float;
        const Format Vector4 = Format.R32G32B32A32_Float;
        const Format Vector2 = Format.R32G32_Float;

        readonly List<InputElement> elements;
        public VertexLayoutConstructor() {
            elements = new List<InputElement>();
        }

        int GetOffset() {
            return elements.Count == 0 ? 0 : InputElement.AppendAligned;
        }

        public InputElement[] ConstuctElements() {
            return elements.ToArray();
        }

        /*
         * InputElement.InstanceDataStepRate 
         * The number of instances to draw using the same per-instance data before advancing in the buffer by one element. 
         * This value must be 0 for an element that contains per-vertex data (PerVertexData)
         */

        internal VertexLayoutConstructor AddPositionElementAsVector3() {
            elements.Add(new InputElement(SemanticPositionName, 0, Vector3, GetOffset(), 0, perverxdata, 0));
            return this;
        }

        internal VertexLayoutConstructor AddNormalElementAsVector3() {
            elements.Add(new InputElement(SemanticNormalName, 0, Vector3, GetOffset(), 0, perverxdata, 0));
            return this;
        }

        internal VertexLayoutConstructor AddColorElementAsVector4() {
            elements.Add(new InputElement(SemanticColorName, 0, Vector4, GetOffset(), 0, perverxdata, 0));
            return this;
        }
        internal VertexLayoutConstructor AddTexCoorElementAsVector2() {
            elements.Add(new InputElement(SemanticTexCoorName, 0, Vector2, GetOffset(), 0, perverxdata, 0));
            return this;
        }
    }
}
