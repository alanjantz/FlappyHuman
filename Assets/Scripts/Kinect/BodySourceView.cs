using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

public class BodySourceView : MonoBehaviour
{
    private static BodySourceView _instance;

    public BodySourceManager BodySourceManager;
    public GameObject JointObject;
    public Material BoneMaterial;

    private Color _color = Color.green;
    private Dictionary<ulong, GameObject> _bodies = new Dictionary<ulong, GameObject>();
    private List<JointType> _joints = new List<JointType>
    {
        JointType.FootLeft,
        JointType.AnkleLeft,
        JointType.KneeLeft,
        JointType.HipLeft,

        JointType.FootRight,
        JointType.AnkleRight,
        JointType.KneeRight,
        JointType.HipRight,

        JointType.HandTipLeft,
        JointType.ThumbLeft,
        JointType.HandLeft,
        JointType.WristLeft,
        JointType.ElbowLeft,
        JointType.ShoulderLeft,


        JointType.HandTipRight,
        JointType.ThumbRight,
        JointType.HandRight,
        JointType.WristRight,
        JointType.ElbowRight,
        JointType.ShoulderRight,


        JointType.SpineBase,
        JointType.SpineMid,
        JointType.SpineShoulder,
        JointType.Neck,
        JointType.Head,
    };

    private Dictionary<JointType, JointType> _boneMap = new Dictionary<JointType, JointType>()
    {
        { JointType.FootLeft, JointType.AnkleLeft },
        { JointType.AnkleLeft, JointType.KneeLeft },
        { JointType.KneeLeft, JointType.HipLeft },
        { JointType.HipLeft, JointType.SpineBase },

        { JointType.FootRight, JointType.AnkleRight },
        { JointType.AnkleRight, JointType.KneeRight },
        { JointType.KneeRight, JointType.HipRight },
        { JointType.HipRight, JointType.SpineBase },

        { JointType.HandTipLeft, JointType.HandLeft },
        { JointType.ThumbLeft, JointType.HandLeft },
        { JointType.HandLeft, JointType.WristLeft },
        { JointType.WristLeft, JointType.ElbowLeft },
        { JointType.ElbowLeft, JointType.ShoulderLeft },
        { JointType.ShoulderLeft, JointType.SpineShoulder },

        { JointType.HandTipRight, JointType.HandRight },
        { JointType.ThumbRight, JointType.HandRight },
        { JointType.HandRight, JointType.WristRight },
        { JointType.WristRight, JointType.ElbowRight },
        { JointType.ElbowRight, JointType.ShoulderRight },
        { JointType.ShoulderRight, JointType.SpineShoulder },

        { JointType.SpineBase, JointType.SpineMid },
        { JointType.SpineMid, JointType.SpineShoulder },
        { JointType.SpineShoulder, JointType.Neck },
        { JointType.Neck, JointType.Head },
    };

    private void Awake()
    {
        _instance = this;
    }

    public void Update()
    {
        (var data, var trackedIds) = GetKinectData();

        DeleteUntrackedBodies(trackedIds);

        CreateOrUpdateBodies(data);
    }

    private (Body[], List<ulong>) GetKinectData()
    {
        var bodiesData = BodySourceManager.GetData() ?? new Body[] { };
        var trackedIds = new List<ulong>();

        if (bodiesData != null)
        {
            bodiesData = bodiesData.Where(body => body != null).ToArray();
            trackedIds = bodiesData.Select(body => body.TrackingId).ToList();
        }

        return (bodiesData, trackedIds);
    }

    private void DeleteUntrackedBodies(List<ulong> trackedIds)
    {
        var knownIds = new List<ulong>(_bodies.Keys);

        for (int i = 0; i < knownIds.Count; i++)
        {
            if (!trackedIds.Contains(knownIds[i]))
            {
                Destroy(_bodies[knownIds[i]]);
                _bodies.Remove(knownIds[i]);
            }
        }
    }

    private void CreateOrUpdateBodies(Body[] data)
    {
        foreach (var body in data.Where(body => body.IsTracked))
        {
            if (!_bodies.ContainsKey(body.TrackingId))
                _bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);

            UpdateBodyObject(body, _bodies[body.TrackingId]);

            if (Level.IsInitializingOrDead)
            {
                _bodies[body.TrackingId].gameObject.SetActive(false);

                if (body.HandLeftState == HandState.Closed && body.HandRightState == HandState.Closed)
                    Level.GetInstance()?.Restart();
            }
            else
                _bodies[body.TrackingId].gameObject.SetActive(true);
        }
    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject($"Body:{id}");

        foreach (JointType joint in _joints)
        {
            GameObject newJoint = Instantiate(JointObject);
            newJoint.name = joint.ToString();
            newJoint.transform.parent = body.transform;

            LineRenderer lr = newJoint.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = BoneMaterial;
            lr.startWidth = lr.endWidth = .2f;
            lr.sortingOrder = 99;
        }

        return body;
    }

    private void UpdateBodyObject(Body body, GameObject bodyObject)
    {
        foreach (JointType joint in _joints)
        {
            Joint sourceJoint = body.Joints[joint];
            Joint? targetJoint = null;

            if (_boneMap.ContainsKey(joint))
                targetJoint = body.Joints[_boneMap[joint]];

            Transform jointObject = bodyObject.transform.Find(joint.ToString());
            jointObject.position = GetVector3FromJoint(sourceJoint);

            LineRenderer lr = jointObject.GetComponent<LineRenderer>();

            if (targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObject.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.startColor = lr.endColor = this._color;
            }
            else
                lr.enabled = false;
        }
    }

    public static BodySourceView GetInstance() => _instance;

    public IEnumerable<GameObject> GetBodies() => _bodies.Values;

    private Vector3 GetVector3FromJoint(Joint joint, int? x = null, int? y = null, int? z = null)
        => new Vector3(x ?? (joint.Position.X * 10), y ?? (joint.Position.Y * 10), z ?? (joint.Position.Z * 10));
}
