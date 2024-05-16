using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

using size_t = System.UIntPtr;

namespace Femyou
{
  class Callbacks : IDisposable
  {
    FMI2.fmi2CallbackFunctions functions;
    public Callbacks(Instance instance, ICallbacks cb)
    {
      Instance = instance;
      CB = cb;
      handle = GCHandle.Alloc(this);
      functions = new FMI2.fmi2CallbackFunctions
      {
        logger = LoggerCallback,
        allocateMemory = Marshalling.AllocateMemory,
        freeMemory = Marshalling.FreeMemory,
        stepFinished = StepFinishedCallback,
        componentEnvironment = GCHandle.ToIntPtr(handle)
      };
      Structure = Marshalling.AllocateMemory((size_t)1, (size_t)Marshal.SizeOf(functions));
      Marshal.StructureToPtr(functions, Structure, false);
    }
    public readonly Instance Instance;
    public readonly ICallbacks CB;
    public readonly IntPtr Structure;
    private readonly GCHandle handle;

    public void Dispose()
    {
      Marshalling.FreeMemory(Structure);
      handle.Free();
    }

    public static void LoggerCallback(IntPtr componentEnvironment, string instanceName, int status, string category, string message)
    {
      var self = (Callbacks) GCHandle.FromIntPtr(componentEnvironment).Target;
      self.CB?.Logger(self.Instance, (Status)status, category, message);
    }

    public static void StepFinishedCallback(IntPtr componentEnvironment, int status)
    {
    }
  }
}