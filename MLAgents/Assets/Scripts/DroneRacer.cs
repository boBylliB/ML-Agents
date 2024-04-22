using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class DroneRacer : Agent
{
    public List<Gate> gates = null;
    public int gateIndex = 0;
    public int loopGateIndex = 0;
    public int maxStepCount = 0;
    public List<Gate> prevGates = new List<Gate>();

    [SerializeField] private Rigidbody rb;
    [SerializeField] private float maxThrust = 1f;
    [SerializeField] private float rotateSpeed = 20f;

    private Vector3 velocity = Vector3.zero;
    private int stepOffset = 0;

    public bool IsNextGate(Gate gate)
    {
        return gates[gateIndex] == gate;
    }
    public bool IsFutureGate(Gate gate)
    {
        for (int idx = 1; idx <= 3; ++idx)
        {
            int index = gateIndex + idx;
            if (index > gates.Count)
                index = loopGateIndex;
            if (gates[index] == gate)
                return true;
        }
        return false;
    }

    public override void Initialize()
    {
        Gate[] tempGates = FindObjectsOfType<Gate>();
        List<Gate> unsortedGates = new List<Gate>();
        foreach (Gate gate in tempGates)
            unsortedGates.Add(gate);
        gates = unsortedGates.OrderBy(x => x.GetOrderIndex()).ToList();
        Physics.IgnoreLayerCollision(3, 3);
    }
    void Update()
    {
        if (StepCount - stepOffset > maxStepCount && maxStepCount > 0)
        {
            AddReward(-0.5f);
            EndEpisode();
        }
    }
    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        velocity = Vector3.zero;
        SetReward(0f);
        gateIndex = 0;
        stepOffset = 0;
        prevGates = new List<Gate>();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.localRotation);
        int idx = gateIndex;
        sensor.AddObservation(gates[idx].transform.localPosition);
        sensor.AddObservation(gates[idx].transform.localRotation);
        ++idx;
        if (idx > gateIndex) idx = loopGateIndex;
        sensor.AddObservation(gates[idx].transform.localPosition);
        sensor.AddObservation(gates[idx].transform.localRotation);
        ++idx;
        if (idx > gateIndex) idx = loopGateIndex;
        sensor.AddObservation(gates[idx].transform.localPosition);
        sensor.AddObservation(gates[idx].transform.localRotation);
        ++idx;
        if (idx > gateIndex) idx = loopGateIndex;
        sensor.AddObservation(gates[idx].transform.localPosition);
        sensor.AddObservation(gates[idx].transform.localRotation);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float throttle = actions.ContinuousActions[0];
        float rotateX = actions.ContinuousActions[1];
        float rotateY = actions.ContinuousActions[2];
        float rotateZ = -actions.ContinuousActions[3];
        //Debug.Log($"Throttle: {throttle}; Pitch: {rotateX}; Yaw: {rotateY}; Roll: {rotateZ}");

        transform.Rotate(new Vector3(rotateX, rotateY, rotateZ) * rotateSpeed * Time.deltaTime, Space.Self);
        //rb.AddRelativeTorque(new Vector3(rotateX, rotateY, rotateZ) * rotateSpeed * Time.deltaTime);
        //rb.AddRelativeForce(Vector3.up * throttle * maxThrust);
        velocity += transform.up * throttle * maxThrust * Time.deltaTime;
        velocity += Physics.gravity * Time.deltaTime;
        transform.localPosition += velocity * Time.deltaTime;
        if (transform.localPosition.y < 0)
        {
            velocity = Vector3.zero;
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[3] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
        continuousActions[2] = Input.GetAxisRaw("Twist");
        continuousActions[0] = Input.GetAxisRaw("Jump");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Gate>(out Gate gate))
        {
            if (gates[gateIndex] == gate)
            {
                AddReward(1f);
                stepOffset = StepCount;
                ++gateIndex;
                prevGates.Add(gate);
                if (prevGates.Count > 3)
                    prevGates.RemoveAt(0);
                if (gateIndex >= gates.Count)
                    gateIndex = loopGateIndex;
            }
            else if (prevGates.Count > 0 && !prevGates.Contains(gate))
            {
                AddReward(-1f);
            }
        }
        else if (other.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-2f);
            EndEpisode();
        }
    }
}
