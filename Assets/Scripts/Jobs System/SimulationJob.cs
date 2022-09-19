using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class SimulationJob : MonoBehaviour
{
    private NativeArray<int> array;

    private void Start()
    {
        array = new NativeArray<int>(20, Allocator.Persistent);
        for (int i = 0; i < array.Length; i++)   
            array[i] = Random.Range(0,100);
        Debug.Log(Print());
        InitJobArray();
        Debug.Log(Print());

    }
    private string Print()
    {
        StringBuilder str = new StringBuilder();
        foreach(int i in array)
            str.Append(i + " ");

        return str.ToString();
    }

    private void InitJobArray()
    {
        JobStruct jobStruct = new JobStruct()
        {
            jobArray = array
        };
        JobHandle jobHandle = jobStruct.Schedule();
        jobHandle.Complete();
    }

    private void OnDestroy()
    {
        if(array.IsCreated)
            array.Dispose();
    }
}

public struct JobStruct : IJob
{
    public NativeArray<int> jobArray;
    public void Execute()
    {
       for(int i = 0; i< jobArray.Length; i++)
            if (jobArray[i] > 10)
                jobArray[i] = 0;
            
    }
}
