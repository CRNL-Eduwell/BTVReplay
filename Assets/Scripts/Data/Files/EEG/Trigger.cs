using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Tools.CSharp.EEG
{
    public class Trigger : DLL.CppDLLImportBase
    {
        #region
        /// <summary>
        /// Code of the event
        /// </summary>
        public int Code
        {
            get
            {
                return GetTriggerCode(_handle);
            }
        }
        /// <summary>
        /// Sample of the event
        /// </summary>
        public long Sample
        {
            get
            {
                return DLL.NativeCLong.Is32Bit ? GetTriggerSample32(_handle) : GetTriggerSample64(_handle);
            }
        }
        #endregion

        #region Memory Management
        /// <summary>
        /// File constructor with an already allocated dll File
        /// </summary>
        /// <param name="filePtr"></param>
        public Trigger(IntPtr filePtr) : base(filePtr) { }
        /// <summary>
        /// Allocate DLL memory
        /// </summary>
        protected override void create_DLL_class()
        {
            throw new Exception("Trigger can not be created outside of a file");
        }
        /// <summary>
        /// Clean DLL memory
        /// </summary>
        protected override void delete_DLL_class()
        {
        }
        #endregion

        #region DLLImport
        [DllImport("EEGFormat", EntryPoint = "GetTriggerCode", CallingConvention = CallingConvention.Cdecl)]
        static private extern int GetTriggerCode(HandleRef electrode);
        // Native signature: long GetTriggerSample(ITrigger*). C long is 32 bits on Windows and 64 bits
        // on macOS/Linux, so one extern per width; Sample picks the one matching NativeCLong.
        [DllImport("EEGFormat", EntryPoint = "GetTriggerSample", CallingConvention = CallingConvention.Cdecl)]
        static private extern int GetTriggerSample32(HandleRef electrode);
        [DllImport("EEGFormat", EntryPoint = "GetTriggerSample", CallingConvention = CallingConvention.Cdecl)]
        static private extern long GetTriggerSample64(HandleRef electrode);
        #endregion
    }
}