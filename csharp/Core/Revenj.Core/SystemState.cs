﻿using System;
using Revenj.Extensibility;

namespace Revenj.Core
{
	internal class SystemState : ISystemState
	{
		public SystemState()
		{
			IsBooting = true;
		}

		public bool IsBooting { get; internal set; }
		public bool IsReady { get; private set; }
		public event Action<IObjectFactory> Ready = f => { };
		public void Started(IObjectFactory factory)
		{
			IsBooting = false;
			IsReady = true;
			Ready(factory);
		}
		public event Action<SystemEvent> Change = f => { };
		public void Notify(SystemEvent value)
		{
			Change(value);
		}
	}
}
