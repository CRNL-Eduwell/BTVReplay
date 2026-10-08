using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Tools.CSharp.EEG
{
    public class Note : DLL.CppDLLImportBase
    {
        #region Properties
        /// <summary>
        /// Description of the note
        /// </summary>
        public string Description
        {
            get
            {
                // The wrapper converts every description to UTF-8 (Utility::toUTF8), whatever the
                // file format. PtrToStringAnsi only decodes UTF-8 on macOS/Linux; on Windows it can
                // use the ANSI code page and garble accented text.
                return Marshal.PtrToStringUTF8(GetNoteDescription(_handle));
            }
        }
        /// <summary>
        /// Sample of the note
        /// </summary>
        public long Sample
        {
            get
            {
                return DLL.NativeCLong.Is32Bit ? GetNoteSample32(_handle) : GetNoteSample64(_handle);
            }
        }
        #endregion

        #region Memory Management
        /// <summary>
        /// File constructor with an already allocated dll File
        /// </summary>
        /// <param name="filePtr"></param>
        public Note(IntPtr filePtr) : base(filePtr) { }
        /// <summary>
        /// Allocate DLL memory
        /// </summary>
        protected override void create_DLL_class()
        {
            throw new Exception("Note can not be created outside of a file");
        }
        /// <summary>
        /// Clean DLL memory
        /// </summary>
        protected override void delete_DLL_class()
        {
        }
        #endregion

        #region DLLImport
        [DllImport("EEGFormat", EntryPoint = "GetNoteDescription", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr GetNoteDescription(HandleRef electrode);
        // Native signature: long GetNoteSample(INote*). C long is 32 bits on Windows and 64 bits on
        // macOS/Linux, so one extern per width; Sample picks the one matching NativeCLong.
        [DllImport("EEGFormat", EntryPoint = "GetNoteSample", CallingConvention = CallingConvention.Cdecl)]
        static private extern int GetNoteSample32(HandleRef electrode);
        [DllImport("EEGFormat", EntryPoint = "GetNoteSample", CallingConvention = CallingConvention.Cdecl)]
        static private extern long GetNoteSample64(HandleRef electrode);
        #endregion
    }
}