using D3DLab.Std.Engine.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D3DLab.Plugin.Contracts.Parsers {
    public interface IParseResultVisiter {
        void Handle(AbstractGeometry3D mesh);
        void Handle(IEnumerable<AbstractGeometry3D> mesh);
    }

    public interface IFileParserPlugin {
        string Name { get; }
        bool IsSupport(string fileExtention);
        void Parse(Stream stream, IParseResultVisiter visiter);
    }
}
