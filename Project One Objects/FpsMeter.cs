using System.Diagnostics;

namespace Project_One_Objects;

public class FpsMeter
{
    private const int BufferSize = 3_000;
    private readonly DateTime[] _buffer;
    private DateTime _lastTime = DateTime.Now;
    private int _index = 0;
    private int _entireIndex = 0;
    private int _osa = 0; // one second age frame index

    public FpsMeter()
    {
        _buffer = new DateTime[BufferSize];
    }

    public void Tick()
    {
        _lastTime = DateTime.Now;
        _buffer[_index++] = _lastTime;
        _index %= BufferSize;
        _entireIndex++;
    }

    public double GetFps()
    {
        while (_buffer[_osa] < _lastTime.AddSeconds(-1))
            _osa = (_osa + 1) % BufferSize;
        return (_index - _osa).Mod(BufferSize);
    }

    public double GetAverageFps()
    {
        if (_entireIndex < BufferSize)
            return _entireIndex / (_lastTime - _buffer[0]).TotalSeconds;

        return BufferSize / (_lastTime - _buffer[_index]).TotalSeconds;
    }
}