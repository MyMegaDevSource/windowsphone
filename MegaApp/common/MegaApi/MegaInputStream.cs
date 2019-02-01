﻿using System.IO;
using mega;

#if CAMERA_UPLOADS_SERVICE
namespace ScheduledCameraUploadTaskAgent.MegaApi
#else
namespace MegaApp.MegaApi
#endif
{
    class MegaInputStream : MInputStream
    {
        private Stream inputStream;
        private long offset;

        public MegaInputStream(Stream stream)
        {
            inputStream = stream;
            offset = 0;
        }

        public virtual ulong Length()
        {
            return (ulong)inputStream.Length;
        }

        public virtual bool Read(byte[] buffer, ulong size)
        {
            if ((offset + (long)size) > inputStream.Length || (buffer != null && buffer.Length < (int)size))
            {
                return false;
            }

            if (buffer == null)
            {
                offset += (long)size;
                return true;
            }

            inputStream.Seek(offset, SeekOrigin.Begin);

            int numBytesToRead = (int)size;
            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                int n = inputStream.Read(buffer, numBytesRead, numBytesToRead);                
                if (n == 0)
                {
                    return false;
                }

                numBytesRead += n;
                numBytesToRead -= n;
            }

            offset += numBytesRead;
            return true;
        }
    }
}
