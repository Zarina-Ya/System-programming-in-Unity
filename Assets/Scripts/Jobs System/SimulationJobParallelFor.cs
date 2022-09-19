using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class SimulationJobParallelFor : MonoBehaviour
{
     NativeArray<Vector3> positions;
     NativeArray<Vector3> velocities;
     NativeArray<Vector3> finalPositions;
    void Start()
    {
        positions = new NativeArray<Vector3>(5, Allocator.Persistent);
        velocities = new NativeArray<Vector3>(4, Allocator.Persistent);
        finalPositions = new NativeArray<Vector3>(5, Allocator.Persistent);

        RandomArray(velocities);
        Debug.Log($"Velocities Random: {Print(velocities)}");
        RandomArray(positions);
        Debug.Log($"Positions Random: {Print(positions)}");
        Debug.Log($"FinalPositions Random: {Print(finalPositions)}");
        InitJobStruct();
        Debug.Log($"FinalPositions Random: {Print(finalPositions)}");

    }
    private void OnDestroy()
    {
        positions.Dispose();
        velocities.Dispose();   
        finalPositions.Dispose();
    }

    private void InitJobStruct()
    {
        JobParallelForStruct jobStruct = new JobParallelForStruct()
        {
            Positions = positions, 
            Velocities = velocities,
            FinalPositions = finalPositions
        };

        JobHandle jobHandle = jobStruct.Schedule(finalPositions.Length, 0);
        jobHandle.Complete();

    }
    private void RandomArray(NativeArray<Vector3> vector)
    {
        for (int i = 0; i < vector.Length; i++)
            vector[i] = new Vector3(Random.Range(0, 25), Random.Range(0, 25), Random.Range(0, 25));
        
    }
    private string Print(NativeArray<Vector3> vector)
    {
        StringBuilder str = new StringBuilder();
        foreach (var i in vector)
            str.Append($"Vector: {i.x},{i.y},{i.z} {'\n'}");

        return str.ToString();
    }
}

public struct JobParallelForStruct : IJobParallelFor
{
    public NativeArray<Vector3> Positions;
    public NativeArray<Vector3> Velocities;
    public NativeArray<Vector3> FinalPositions;
    public void Execute(int index)
    {
        FinalPositions[index] += (index < Positions.Length) ? Positions[index] : Vector3.zero;
        FinalPositions[index] += (index < Velocities.Length) ? Velocities[index] : Vector3.zero;
    }
}
