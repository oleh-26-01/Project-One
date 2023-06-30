namespace Project_One_Objects.Helpers;

public class FpsMeter
{
    private const int BufferSize = 3_000;
    private readonly DateTime[] _buffer;
    private int _entireIndex;
    private int _index;
    private DateTime _lastTime = DateTime.Now;
    private int _osa; // one second age frame index

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
        while (_buffer[_osa] < _lastTime.AddSeconds(-1)) _osa = (_osa + 1) % BufferSize;

        return (_index - _osa).Mod(BufferSize);
    }

    public double GetAverageFps()
    {
        return _entireIndex < BufferSize
            ? _entireIndex / (_lastTime - _buffer[0]).TotalSeconds
            : BufferSize / (_lastTime - _buffer[_index]).TotalSeconds;
    }
}