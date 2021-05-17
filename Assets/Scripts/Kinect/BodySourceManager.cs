using UnityEngine;
using Windows.Kinect;

public class BodySourceManager : MonoBehaviour
{
    private KinectSensor _sensor;
    private BodyFrameReader _reader;
    private Body[] _data = null;

    public Body[] GetData() => _data;

    public void Start()
    {
        _sensor = KinectSensor.GetDefault();

        if (_sensor != null)
        {
            _reader = _sensor.BodyFrameSource.OpenReader();

            if (!_sensor.IsOpen)
                _sensor.Open();
        }
    }

    public void Update()
    {
        if (_reader != null)
        {
            var frame = _reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_data == null)
                    _data = new Body[_sensor.BodyFrameSource.BodyCount];

                frame.GetAndRefreshBodyData(_data);

                frame.Dispose();
            }
        }
    }

    public void OnApplicationQuit()
    {
        if (_reader != null)
        {
            _reader.Dispose();
            _reader = null;
        }

        if (_sensor != null)
        {
            if (_sensor.IsOpen)
                _sensor.Close();

            _sensor = null;
        }
    }
}
