using System;
using System.Buffers;
using System.IO;

public class PoolMemoryStream : Stream
{

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => true;
    private long _length;
    public override long Length => _length;
    private long _position;
    public override long Position { get => _position; set => _position = value; }
    private byte[] _innerBuffer;
    public PoolMemoryStream() : this(128)
    {

    }

    public PoolMemoryStream(int size)
    {
        _innerBuffer = ArrayPool<byte>.Shared.Rent(size);
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var canFillSize = Math.Min(count, buffer.Length - offset);
        var readSize = Math.Min(canFillSize, _length - _position);
        Array.Copy(_innerBuffer, _position, buffer, offset, readSize);
        _position += readSize;
        return (int)readSize;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                if (offset < 0 || offset > _length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _position = offset;
                break;
            case SeekOrigin.Current:
            case SeekOrigin.End:
                var newPosition = _position + offset;
                if (newPosition < 0 || newPosition > _length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _position = newPosition;
                break;
        }
        return _position;
    }

    public override void SetLength(long value)
    {
        _length = value;
        _position = value;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (offset + count > buffer.Length)
        {
            throw new ArgumentOutOfRangeException();
        }
        var length = _position + count;
        if (length > _innerBuffer.LongLength)
        {
            var newBuffer = ArrayPool<byte>.Shared.Rent((int)Math.Max(length, _innerBuffer.LongLength * 2));
            Array.Copy(_innerBuffer, newBuffer, _length);
            ArrayPool<byte>.Shared.Return(_innerBuffer);
            _innerBuffer = newBuffer;
        }
        Array.Copy(buffer, offset, _innerBuffer, _position, count);
        _length += count;
        _position += count;
    }

    private bool disposed = false;
    protected override void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }
        ArrayPool<byte>.Shared.Return(_innerBuffer);
        disposed = true;
    }
}