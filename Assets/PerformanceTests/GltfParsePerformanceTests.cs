using System;
using System.IO;
using NUnit.Framework;
using UniGLTF;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Profiling;
using UniVRM10;

public class GltfParsePerformanceTests
{
    static string AliciaPath => Path.GetFullPath(
        Path.Combine(Application.dataPath, "../Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm"));

    static Vrm10Instance CreateVrm10Instance(byte[] bytes)
    {
        return Vrm10.LoadBytesAsync(
            bytes,
            canLoadVrm0X: true,
            awaitCaller: new ImmediateCaller(),
            materialGenerator: new BuiltInVrm10MaterialDescriptorGenerator()
        ).Result;
    }

    [Test, Performance]
    public void ParseGlb()
    {
        var bytes = File.ReadAllBytes(AliciaPath);

        Measure.Method(() =>
            {
                using var data = new GlbLowLevelParser(AliciaPath, bytes).Parse();
            })
            .WarmupCount(3)
            .MeasurementCount(20)
            .GC()
            .Run();
    }

    [Test, Performance]
    public void LoadVrm10Instance()
    {
        var bytes = File.ReadAllBytes(AliciaPath);

        Measure.Method(() =>
            {
                var instance = CreateVrm10Instance(bytes);
                UnityEngine.Object.DestroyImmediate(instance.gameObject);
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
    }

    [Test, Performance]
    public void AllocatedBytes()
    {
        var bytes = File.ReadAllBytes(AliciaPath);

        var parseBytes = new SampleGroup("Parse.GC.AllocatedBytes", SampleUnit.Kilobyte);
        var loadBytes = new SampleGroup("Load.GC.AllocatedBytes", SampleUnit.Kilobyte);

        // warmup
        using (new GlbLowLevelParser(AliciaPath, bytes).Parse()) { }
        UnityEngine.Object.DestroyImmediate(CreateVrm10Instance(bytes).gameObject);

        for (var i = 0; i < 5; ++i)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var before = GC.GetAllocatedBytesForCurrentThread();
            using (new GlbLowLevelParser(AliciaPath, bytes).Parse()) { }
            var after = GC.GetAllocatedBytesForCurrentThread();
            Measure.Custom(parseBytes, (after - before) / 1024.0);

            before = GC.GetAllocatedBytesForCurrentThread();
            var instance = CreateVrm10Instance(bytes);
            after = GC.GetAllocatedBytesForCurrentThread();
            UnityEngine.Object.DestroyImmediate(instance.gameObject);
            Measure.Custom(loadBytes, (after - before) / 1024.0);
        }
    }

    [Test, Performance]
    public void NativeAllocatedBytes()
    {
        var bytes = File.ReadAllBytes(AliciaPath);

        var parseNative = new SampleGroup("Parse.Native.AllocatedBytes", SampleUnit.Kilobyte);
        var loadNative = new SampleGroup("Load.Native.AllocatedBytes", SampleUnit.Kilobyte);

        // warmup
        using (new GlbLowLevelParser(AliciaPath, bytes).Parse()) { }
        UnityEngine.Object.DestroyImmediate(CreateVrm10Instance(bytes).gameObject);

        for (var i = 0; i < 5; ++i)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var before = Profiler.GetTotalAllocatedMemoryLong();
            using (new GlbLowLevelParser(AliciaPath, bytes).Parse())
            {
                var after = Profiler.GetTotalAllocatedMemoryLong();
                Measure.Custom(parseNative, (after - before) / 1024.0);
            }

            before = Profiler.GetTotalAllocatedMemoryLong();
            var instance = CreateVrm10Instance(bytes);
            var afterLoad = Profiler.GetTotalAllocatedMemoryLong();
            UnityEngine.Object.DestroyImmediate(instance.gameObject);
            Measure.Custom(loadNative, (afterLoad - before) / 1024.0);
        }
    }
}