﻿// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using Revenj.Extensibility.Autofac.Builder;

namespace Revenj.Extensibility.Autofac.Core.Registration
{
	/// <summary>
	/// Pulls registrations from another component registry.
	/// Excludes most auto-generated registrations - currently has issues with
	/// collection registrations.
	/// </summary>
	class ExternalRegistrySource : IRegistrationSource
	{
		readonly IComponentRegistry _registry;

		/// <summary>
		/// Create an external registry source that draws components from
		/// <paramref name="registry"/>.
		/// </summary>
		/// <param name="registry">Component registry to pull registrations from.</param>
		public ExternalRegistrySource(IComponentRegistry registry)
		{
			_registry = registry;
		}

		/// <summary>
		/// Retrieve registrations for an unregistered service, to be used
		/// by the container.
		/// </summary>
		/// <param name="service">The service that was requested.</param>
		/// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
		/// <returns>Registrations providing the service.</returns>
		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{
#if !WINDOWS_PHONE
			var seenRegistrations = new HashSet<IComponentRegistration>();
			var seenServices = new HashSet<Service>();
#else
			var seenRegistrations = new List<IComponentRegistration>();
			var seenServices = new List<Service>();
#endif
			var lastRunServices = new LinkedList<Service>();
			lastRunServices.AddFirst(service);

			while (lastRunServices.Count > 0)
			{
				var nextService = lastRunServices.First.Value;
				lastRunServices.RemoveFirst();
				seenServices.Add(nextService);
				foreach (var registration in _registry.RegistrationsFor(nextService))
				{
					if (registration.IsAdapting() || seenRegistrations.Contains(registration))
						continue;

					seenRegistrations.Add(registration);
					foreach (var serv in registration.Services)
					{
						if (!seenServices.Contains(serv))
							lastRunServices.AddLast(serv);
					}

					var r = registration;
					yield return RegistrationBuilder.ForDelegate((c, p) => c.ResolveComponent(service, r, p))
						.Targeting(r)
						.As(r.Services.ToArray())
						.ExternallyOwned()
						.CreateRegistration();
				}
			}
		}

		/// <summary>
		/// In this case because the components that are adapted do not come from the same
		/// logical scope, we must return false to avoid duplicating them.
		/// </summary>
		public bool IsAdapterForIndividualComponents
		{
			get { return false; }
		}
	}
}
