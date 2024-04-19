using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class DroneAgent : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float maxThrust = 1f;
    [SerializeField] private float rotateSpeed = 20f;
    [SerializeField] private Vector3 spawnMin;
    [SerializeField] private Vector3 spawnMax;
    [SerializeField] private float radius;

    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    private Vector3 velocity = Vector3.zero;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(spawnMin.x, spawnMax.x), 0, Random.Range(spawnMin.z, spawnMax.z));
        //transform.localPosition = new Vector3(0, 0, 0);
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        velocity = Vector3.zero;
        // Spawn the target right above the agent
        //targetTransform.localPosition = transform.localPosition + Vector3.up * 5f;
        // Spawn the target above the agent with a bit of randomness
        //targetTransform.localPosition = new Vector3(Random.Range(spawnMin.x, spawnMax.x), 5f, Random.Range(spawnMin.z, spawnMax.z));
        // Spawn the target somewhere outside of the given spawn radius around the agent
        Vector3 range = spawnMax - spawnMin - Vector3.one * 2 * radius;
        targetTransform.localPosition = new Vector3(Random.Range(0, range.x), Random.Range(0, range.y), Random.Range(0, range.z)) + spawnMin;
        if (targetTransform.localPosition.x >= transform.localPosition.x - radius)
            targetTransform.localPosition += Vector3.right * 2 * radius;
        if (targetTransform.localPosition.y >= transform.localPosition.y - radius)
            targetTransform.localPosition += Vector3.up * 2 * radius;
        if (targetTransform.localPosition.z >= transform.localPosition.z - radius)
            targetTransform.localPosition += Vector3.forward * 2 * radius;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.localRotation);
        sensor.AddObservation(targetTransform.localPosition);
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
        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(1f);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-1f);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }
}
