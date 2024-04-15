using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private Vector3 spawnMin;
    [SerializeField] private Vector3 spawnMax;
    [SerializeField] private float radius;

    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    public override void OnEpisodeBegin()
    {
        Vector3 middle = (spawnMax - spawnMin) / 2 + spawnMin;
        transform.localPosition = new Vector3(Random.Range(spawnMin.x, spawnMax.x), 0, Random.Range(spawnMin.z, spawnMax.z));
        bool rightSide = transform.localPosition.x > middle.x;
        if (rightSide)
            targetTransform.localPosition = new Vector3(Random.Range(spawnMin.x, middle.x - radius), 0, Random.Range(spawnMin.z, spawnMax.z));
        else
            targetTransform.localPosition = new Vector3(Random.Range(middle.x + radius, spawnMax.x), 0, Random.Range(spawnMin.z, spawnMax.z));
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
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
