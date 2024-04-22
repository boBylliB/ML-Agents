using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Gate : MonoBehaviour
{
    public Material goalMaterial;
    public Material futureMaterial;
    public Material prevMaterial;
    public Material wallMaterial;

    private DroneRacer racer = null;

    void Start()
    {
        racer = FindObjectOfType<DroneRacer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (racer != null && racer.gates != null)
        {
            if (racer.IsNextGate(this))
                GetComponent<MeshRenderer>().material = goalMaterial;
            else if (racer.IsFutureGate(this))
                GetComponent<MeshRenderer>().material = futureMaterial;
            else if (racer.prevGates.Count > 0 && racer.prevGates.Contains(this))
                GetComponent<MeshRenderer>().material = prevMaterial;
            else
                GetComponent<MeshRenderer>().material = wallMaterial;
        }
    }

    public int GetOrderIndex()
    {
        int start = name.IndexOf('(')+1;
        if (start <= 0)
            return 0;
        else
        {
            int end = name.IndexOf(')');
            return int.Parse(name.Substring(start, end > 0 ? end-start : name.Length-start));
        }
    }
}
