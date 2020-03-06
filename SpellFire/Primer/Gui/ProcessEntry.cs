﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellFire.Primer.Gui
{
	public class ProcessEntry
	{
		private Process process;

		public ProcessEntry(Process process)
		{
			this.process = process;
		}

		public Process GetProcess()
		{
			return process;
		}

		public override string ToString()
		{
			return $"pid: {process.Id}";
		}

		public override bool Equals(object obj)
		{
			var processEntry = obj as ProcessEntry;

			if (processEntry == null)
			{
				return false;
			}

			return this.process.Equals(processEntry.process);
		}
	}
}
