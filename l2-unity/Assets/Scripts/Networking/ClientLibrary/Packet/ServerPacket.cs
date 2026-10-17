using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class ServerPacket : Packet
{
    protected byte minimumLength;
    protected int _iterator;

    public ServerPacket(byte[] d) : base(d)
    {
        ReadB();
    }

    protected byte ReadB()
    {
        if (_packetData == null || _iterator >= _packetData.Length) return 0;
        return _packetData[_iterator++];
    }

    protected byte[] ReadB(int length)
    {
        byte[] data = new byte[length];
        if (_packetData == null || _iterator >= _packetData.Length) return data;
        int avail = Math.Min(length, _packetData.Length - _iterator);
        Array.Copy(_packetData, _iterator, data, 0, avail);
        _iterator += avail;

        return data;
    }

    protected virtual int ReadI()
    {
        if (_packetData == null || _iterator + 4 > _packetData.Length)
        {
            _iterator = _packetData != null ? _packetData.Length : 0;
            return 0;
        }
        int value = BitConverter.ToInt32(_packetData, _iterator);
        _iterator += 4;
        return value;
    }

    protected virtual long ReadL()
    {
        if (_packetData == null || _iterator + 8 > _packetData.Length)
        {
            _iterator = _packetData != null ? _packetData.Length : 0;
            return 0;
        }
        long value = BitConverter.ToInt64(_packetData, _iterator);
        _iterator += 8;
        return value;
    }

    protected virtual float ReadF()
    {
        if (_packetData == null || _iterator + 4 > _packetData.Length)
        {
            _iterator = _packetData != null ? _packetData.Length : 0;
            return 0f;
        }
        float value = BitConverter.ToSingle(_packetData, _iterator);
        _iterator += 4;
        return value;
    }

    protected virtual double ReadD()
    {
        if (_packetData == null || _iterator + 8 > _packetData.Length)
        {
            _iterator = _packetData != null ? _packetData.Length : 0;
            return 0.0;
        }
        double value = BitConverter.ToDouble(_packetData, _iterator);
        _iterator += 8;
        return value;
    }

    protected virtual int ReadH()
    {
        if (_packetData == null || _iterator + 2 > _packetData.Length)
        {
            _iterator = _packetData != null ? _packetData.Length : 0;
            return 0;
        }
        short value = BitConverter.ToInt16(_packetData, _iterator);
        _iterator += 2;
        return (int)value;
    }

    protected virtual string ReadS()
    {
        List<char> chars = new List<char>();

        while (_packetData != null && _iterator + 2 <= _packetData.Length)
        {
            char c = BitConverter.ToChar(_packetData, _iterator);
            _iterator += 2;

            if (c == '\0')
                break;

            chars.Add(c);
        }

        return new string(chars.ToArray());
    }

    public abstract void Parse();
}
