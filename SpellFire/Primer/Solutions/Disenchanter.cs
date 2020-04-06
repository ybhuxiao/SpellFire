﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SpellFire.Primer.Gui;
using SpellFire.Well.Controller;
using SpellFire.Well.Lua;
using SpellFire.Well.Util;

namespace SpellFire.Primer.Solutions
{
	class Disenchanter : Solution
	{
		private readonly LuaEventListener eventListener;

		private readonly string DisenchantLuaScript;

		public Disenchanter(ControlInterface ci, Memory memory) : base(ci, memory)
		{
			eventListener = new LuaEventListener(ci);
			eventListener.Bind("LOOT_OPENED", LootOpenedHandler);

			DisenchantLuaScript = Encoding.UTF8.GetString(File.ReadAllBytes("Scripts/Disenchant.lua"));

			this.Active = true;
		}

		private void LootOpenedHandler(LuaEventArgs luaEventArgs)
		{
			ci.remoteControl.FrameScript__Execute("for i = 1, GetNumLootItems() do LootSlot(i) ConfirmLootSlot(i) end", 0, 0);
		}

		public override void Tick()
		{
			ci.remoteControl.FrameScript__Execute(DisenchantLuaScript, 0, 0);

			/* Disenchant cast time is 3s, so give some idle margin to simulate human behaviour */
			DateTime tiemoutStart = DateTime.Now;
			Func<Int32, bool> timeout = (timeoutSeconds) => (DateTime.Now - tiemoutStart).Seconds < timeoutSeconds;
			while (timeout(4))
			{
				Thread.Sleep(100);
				if ( ! Active)
				{
					/* cut sleep when solution got turned off */
					return;
				}
			}
		}

		public override void Finish()
		{
			eventListener.Dispose();
		}

		private void CastSpell(string spellName)
		{
			ci.remoteControl.FrameScript__Execute($"CastSpellByName('{spellName}')", 0, 0);
		}
	}
}