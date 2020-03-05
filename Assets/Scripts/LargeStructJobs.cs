using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

using Unity.Mathematics;
using static Unity.Mathematics.math;

public struct LargeStruct
{
    public float h1;
    public float h2;
    public float h3;
    public float h4;
    public float h5;
    public float h6;
    public float h7;
    public float h8;
}

public struct HalfStruct
{
    public float h1;
    public float h2;
    public float h3;
    public float h4;
}

public class LargeStructJobs : MonoBehaviour
{
    [BurstCompile]
    public struct SetNew : IJobParallelFor
    {
        [WriteOnly] public NativeArray<LargeStruct> largeStructs;

        public void Execute(int i)
        {
            largeStructs[i] = new LargeStruct();
        }
    }

    [BurstCompile]
    public struct SingleValueSet : IJobParallelFor
    {
        [WriteOnly] public NativeArray<LargeStruct> largeStructs;

        public void Execute(int i)
        {
            LargeStruct ls = new LargeStruct();
            ls.h1 = 1;
            largeStructs[i] = ls;
        }
    }

    [BurstCompile]
    public struct MultiValueSet : IJobParallelFor
    {
        [WriteOnly] public NativeArray<LargeStruct> largeStructs;

        public void Execute(int i)
        {
            LargeStruct ls = new LargeStruct();
            ls.h1 = 1;
            ls.h2 = 1;
            ls.h3 = 1;
            ls.h4 = 1;
            largeStructs[i] = ls;
        }
    }

    [BurstCompile]
    public struct MultiValueRWSet : IJobParallelFor
    {
        public NativeArray<LargeStruct> largeStructs;

        public void Execute(int i)
        {
            LargeStruct ls = largeStructs[i];
            ls.h1 = 1;
            ls.h2 = 1;
            ls.h3 = 1;
            ls.h4 = 1;
            largeStructs[i] = ls;
        }
    }

    [BurstCompile]
    public struct HalfStructMultiValueSet : IJobParallelFor
    {
        [WriteOnly] public NativeArray<HalfStruct> largeStructs;

        public void Execute(int i)
        {
            HalfStruct ls = new HalfStruct();
            ls.h1 = 1;
            ls.h2 = 1;
            ls.h3 = 1;
            ls.h4 = 1;
            largeStructs[i] = ls;
        }
    }

    [BurstCompile]
    public struct CopyHalfToHalf : IJobParallelFor
    {
        [ReadOnly] public NativeArray<HalfStruct> readStructs;
        [WriteOnly] public NativeArray<HalfStruct> writeStructs;

        public void Execute(int i)
        {
            writeStructs[i] = readStructs[i];
        }
    }

    [BurstCompile]
    public struct CopyToInJob : IJob
    {
        [ReadOnly] public NativeArray<HalfStruct> readStructs;
        [WriteOnly] public NativeArray<HalfStruct> writeStructs;

        public void Execute()
        {
            readStructs.CopyTo(writeStructs);
        }
    }
}
