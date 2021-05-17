using CodeMonkey;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BodyJoint : MonoBehaviour
{
    public Transform BodyMesh;
    public event EventHandler OnDied;

    private bool hasOnDiedEvent;

    public void SetHasOnDiedEvent(bool value = true) => hasOnDiedEvent = value;
    public bool GetHasOnDiedEvent() => hasOnDiedEvent;

    private static List<BodyJoint> _joints;

    private void Awake()
    {
        if (_joints == null)
            _joints = new List<BodyJoint>();

        if (!_joints.Contains(this))
            _joints.Add(this);
    }

    private void Update()
    {
        BodyMesh.position = Vector3.Lerp(BodyMesh.position, transform.position, Time.deltaTime * 15.0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Pipe"))
        {
            if (Level.GetInstance().GetState() != GameState.Dead)
                SoundManager.PlaySound(Sounds.Lose);
            OnDied?.Invoke(this, EventArgs.Empty);
        }
    }

    public static List<BodyJoint> GetJoints() => _joints;
}
