// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Imports for the ftd2xx.dll as well as libft4222
    /// </summary>
    internal class FtFunction
    {
        /// <summary>
        /// Create Device Information List
        /// </summary>
        /// <param name="numdevs">number of devices</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx", EntryPoint = "FT_CreateDeviceInfoList")]
        public static extern FtStatus FT_CreateDeviceInfoList(out uint numdevs);

        /// <summary>
        /// Get Device Information Detail
        /// </summary>
        /// <param name="index">Index of the device</param>
        /// <param name="flags">Flags</param>
        /// <param name="chiptype">Device type</param>
        /// <param name="id">ID</param>
        /// <param name="locid">Location ID</param>
        /// <param name="serialnumber">Serial Number</param>
        /// <param name="description">Description</param>
        /// <param name="ftHandle">Handle</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_GetDeviceInfoDetail(uint index, out uint flags, out FtDeviceType chiptype, out uint id, out uint locid, in byte serialnumber, in byte description, out IntPtr ftHandle);

        /// <summary>
        /// Open a device
        /// </summary>
        /// <param name="pvArg1">The device element identifying the device, depends on the flag</param>
        /// <param name="dwFlags">The flag how to open the device</param>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_OpenEx(uint pvArg1, FtOpenType dwFlags, out SafeFtHandle ftHandle);

        /// <summary>
        /// Close the device
        /// </summary>
        /// <param name="ftHandle">The device handle</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_Close(IntPtr ftHandle);

        [DllImport("ftd2xx")]
        public static extern FtStatus FT_SetTimeouts(SafeFtHandle ftHandle, uint dwReadTimeout, uint dwWriteTimeout);

        [DllImport("ftd2xx")]
        public static extern FtStatus FT_SetLatencyTimer(SafeFtHandle ftHandle, byte ucLatency);

        [DllImport("ftd2xx")]
        public static extern FtStatus FT_SetFlowControl(SafeFtHandle ftHandle, ushort usFlowControl, byte uXon, byte uXoff);
        
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_SetBitMode(SafeFtHandle ftHandle, byte ucMask, byte ucMode);
        
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_GetQueueStatus(SafeFtHandle ftHandle, ref uint lpdwAmountInRxQueue);
        
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_Read(SafeFtHandle ftHandle, in byte lpBuffer, uint dwBytesToRead, ref uint lpdwBytesReturned);

        [DllImport("ftd2xx")]
        public static extern FtStatus FT_Write(SafeFtHandle ftHandle, in byte lpBuffer, uint dwBytesToWrite, ref uint lpdwBytesWritten);

        [DllImport("ftd2xx")]
        public static extern FtStatus FT_SetChars(SafeFtHandle ftHandle, byte uEventCh, byte uEventChEn, byte uErrorCh, byte uErrorChEn);

        [DllImport("ftd2xx")]
        public static extern FtStatus FT_SetUSBParameters(SafeFtHandle ftHandle, uint dwInTransferSize, uint dwOutTransferSize);
    }
}
