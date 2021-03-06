﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SpellFire.Well.Util
{
	public class Memory
	{
		private readonly IntPtr processHandle;
		private Int32 lastOpProcessedBytes;

		private const Int32 StringChunkLength = 20;

		public Memory(Process process)
		{
			if (process == null)
			{
				throw new InvalidOperationException("Process is invalid.");
			}

			this.processHandle = SystemWin32.OpenProcess(SystemWin32.PROCESS_ALL_ACCESS, false, process.Id);
		}

		public byte[] Read(IntPtr address, Int32 size)
		{
			byte[] buffer = new byte[size];
			SystemWin32.ReadProcessMemory(processHandle, address, buffer, buffer.Length, ref lastOpProcessedBytes);
			return buffer;
		}

		public bool Write(IntPtr address, byte[] buffer)
		{
			return SystemWin32.WriteProcessMemory(processHandle, address, buffer, buffer.Length, ref lastOpProcessedBytes);
		}

		public string ReadString(IntPtr address)
		{
			List<byte> chars = new List<byte>();

			while (true)
			{
				byte[] chunk = Read(address, StringChunkLength);

				foreach (var character in chunk)
				{
					chars.Add(character);

					if (character == '\0')
					{
						goto end;
					}
				}
			}

			end:
			return Encoding.UTF8.GetString(chars.ToArray());
		}

		public Ty ReadStruct<Ty>(IntPtr address) where Ty : struct
		{
			Type type = typeof(Ty).IsEnum ? Enum.GetUnderlyingType(typeof(Ty)) : typeof(Ty);

			byte[] data = Read(address, Marshal.SizeOf(type));

			Ty result;
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			try
			{
				result = (Ty) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
			}
			finally
			{
				handle.Free();
			}

			return result;
		}

		public Int64 ReadInt64(IntPtr address)
		{
			return BitConverter.ToInt64(Read(address, sizeof(Int64)), 0);
		}

		public Int32 ReadInt32(IntPtr address)
		{
			return BitConverter.ToInt32(Read(address, sizeof(Int32)) , 0);
		}

		public IntPtr ReadPointer32(IntPtr address)
		{
			return new IntPtr(ReadInt32(address));
		}
	}
}
