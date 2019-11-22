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
        static public extern int ToHilbert(float[] Signal, int Length, int SamplingFrequency, float[] filteredData, int FilteredLength, int DownsampledFrequency, string freqBands );

        [DllImport("Framework", EntryPoint = "Convolution", CallingConvention = CallingConvention.Cdecl)]
        static public extern void Convolution(float[] iinputData, int length, float[] outputData, int coeff);

        //pearson à remettre
        //pearson2 à remettre

        [DllImport("Framework", EntryPoint = "PearsonCorrelationCoefficients", CallingConvention = CallingConvention.Cdecl)]
        static public extern float PearsonCorrelationCoefficients(float[] baseline, float[] channel, int[] sizes);

        [DllImport("Framework", EntryPoint = "Median", CallingConvention = CallingConvention.Cdecl)]
        static public extern float Median(float[] DataArray, int Size);
    }
}
