using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;
        
        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _writePos-_readPos; } } //데이터 들어온 공간
        public int FreeSize { get { return _buffer.Count - _writePos; } } //남은 공간

        public ArraySegment<byte> ReadSegment //데이터 정보 가져옴
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }
        public ArraySegment<byte> WriteSegment //빈 공간 가져옴
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }
        
        public void Clean() //밀린 r, w를 땡겨옴
        {
            int dataSize = DataSize;
            if(dataSize == 0)
            {
                //남은 데이터가 없으면 복사하지 않고 처음으로 땡겨옴
                _readPos = _writePos = 0;
            }
            else
            {
                //남은 데이터가 있으면 시작 위치로 복사
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }
        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }
    }
}
