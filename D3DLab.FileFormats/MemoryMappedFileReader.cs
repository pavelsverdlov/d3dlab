using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace D3DLab.FileFormats {
    public sealed class MemoryMappedFileReader : IDisposable {
        readonly MemoryMappedFile mm;
        MemoryMappedViewStream va;
        SafeMemoryMappedViewHandle mma;
        bool disposedValue;

        public MemoryMappedFileReader(FileInfo file) {
            if(file == null) {
                throw new ArgumentNullException("file");
            }
            mm = MemoryMappedFile.CreateFromFile(file.FullName, FileMode.Open);
        }
        public MemoryMappedFileReader(string filePath) {
            if (string.IsNullOrWhiteSpace(filePath)) {
                throw new ArgumentNullException("filePath");
            }
            mm = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open);
        }

        public ReadOnlySpan<byte> ReadSpan() {
            va = mm.CreateViewStream();
            mma = va.SafeMemoryMappedViewHandle;

            ReadOnlySpan<byte> bytes;
            unsafe {
                byte* ptrMemMap = (byte*)0;
                mma.AcquirePointer(ref ptrMemMap);
                bytes = new ReadOnlySpan<byte>(ptrMemMap, (int)mma.ByteLength);
                mma.ReleasePointer();
            }

            return bytes;
        }

        void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    mm.Dispose();
                    va.Dispose();
                }
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer

                mma.Dispose();
                mma = null;
                
                disposedValue = true;
            }
        }
        ~MemoryMappedFileReader() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
