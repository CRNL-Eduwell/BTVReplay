using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Tools.CSharp.EEG;
using UnityEngine;

namespace BTV.Services.CalculationService
{
    public static class CalculationService
    {
        [DllImport("Framework", EntryPoint = "ToHilbert", CallingConvention = CallingConvention.Cdecl)]
        static public extern int ToHilbert(float[] Signal, int Length, int SamplingFrequency, int DownsampledFrequency, string freqBands, float[] filteredData);

        [DllImport("Framework", EntryPoint = "Convolution", CallingConvention = CallingConvention.Cdecl)]
        static public extern void Convolution(float[] iinputData, int length, float[] outputData, int coeff);
    }
}
