using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public bool enabler = false;
    public LineRenderer rope;
    public LayerMask collMask;

    public List<Vector3> ropePositions { get; set; } = new List<Vector3>();

    private void Awake() => AddPosToRope(Vector3.zero);

    private void Update()
    {
        if (enabler)
        {
            UpdateRopePositions();
            LastSegmentGoToPlayerPos();

            DetectCollisionEnter();
            if (ropePositions.Count > 2) DetectCollisionExits();
        }
    }

    private void DetectCollisionEnter()
    {
        RaycastHit hit;
        if (Physics.Linecast(startPoint.position, rope.GetPosition(ropePositions.Count - 2), out hit, collMask))
        {
            ropePositions.RemoveAt(ropePositions.Count - 1);
            AddPosToRope(hit.point);
        }
    }

    private void DetectCollisionExits()
    {
        RaycastHit hit;
        if (!Physics.Linecast(startPoint.position, rope.GetPosition(ropePositions.Count - 3), out hit, collMask))
        {
            ropePositions.RemoveAt(ropePositions.Count - 2);
        }
    }

    private void AddPosToRope(Vector3 _pos)
    {
        ropePositions.Add(_pos);
        ropePositions.Add(startPoint.position); //Always the last pos must be the player
    }

    private void UpdateRopePositions()
    {
        rope.positionCount = ropePositions.Count;
        rope.SetPositions(ropePositions.ToArray());
        rope.SetPosition(0, endPoint.position);
    }

    private void LastSegmentGoToPlayerPos() => rope.SetPosition(rope.positionCount - 1, startPoint.position);
}