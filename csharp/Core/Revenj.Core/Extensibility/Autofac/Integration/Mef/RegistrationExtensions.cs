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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Revenj.Extensibility.Autofac.Builder;
using Revenj.Extensibility.Autofac.Core;
using Revenj.Extensibility.Autofac.Core.Registration;

namespace Revenj.Extensibility.Autofac.Integration.Mef
{
	/// <summary>
	/// Extension methods that add MEF hosting capabilities to the container building classes.
	/// </summary>
	public static class RegistrationExtensions
	{
		static IEnumerable<Service> DefaultExposedServicesMapper(ExportDefinition ed)
		{
			yield return MapService(ed);
		}

		static Service MapService(ExportDefinition ed)
		{
			var ct = FindType(ed.ContractName);
			if (ct != null)
				return new TypedService(ct);

			var et = FindType((string)ed.Metadata[CompositionConstants.ExportTypeIdentityMetadataName]);
			return new KeyedService(ed.ContractName, et);
		}

		static Type FindType(string exportTypeIdentity)
		{
#if SL4
			var assemblies = System.Windows.Deployment.Current.Parts
				.Select(r => System.Windows.Application.GetResourceStream(new Uri(r.Source, UriKind.Relative)))
				.Select(s => new System.Windows.AssemblyPart().Load(s.Stream));
#else
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
#endif
			return assemblies
				.Select(a => a.GetType(exportTypeIdentity, false))
				.Where(t => t != null)
				.SingleOrDefault();
		}

		/// <summary>
		/// Expose the registered service to MEF parts as an export.
		/// </summary>
		/// <param name="registration">The component being registered.</param>
		/// <param name="configurationAction">Action on an object that configures the export.</param>
		/// <returns>A registration allowing registration to continue.</returns>
		public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
			Exported<TLimit, TActivatorData, TSingleRegistrationStyle>(
				this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
				Action<ExportConfigurationBuilder> configurationAction)
			where TSingleRegistrationStyle : SingleRegistrationStyle
		{
			if (registration == null) throw new ArgumentNullException("registration");
			if (configurationAction == null) throw new ArgumentNullException("configurationAction");

			var configuration = new ExportConfigurationBuilder();
			configurationAction(configuration);
			registration.OnRegistered(e => AttachExport(e.ComponentRegistry, e.ComponentRegistration, configuration));

			return registration;
		}

		/// <summary>
		/// Register a MEF-attributed type as a component.
		/// </summary>
		/// <param name="builder">The container builder.</param>
		/// <param name="partType">The attributed type to register.</param>
		/// <remarks>
		/// A simple heuristic/type scanning technique will be used to determine which MEF exports
		/// are exposed to other components in the Autofac container.
		/// </remarks>
		public static void RegisterComposablePartType(
			this ContainerBuilder builder,
			Type partType)
		{
			RegisterComposablePartType(builder, partType, DefaultExposedServicesMapper);
		}

		/// <summary>
		/// Register a MEF-attributed type as a component.
		/// </summary>
		/// <param name="builder">The container builder.</param>
		/// <param name="partType">The attributed type to register.</param>
		/// <param name="exposedServicesMapper">A mapping function to transform ExportDefinitions into Services.</param>
		public static void RegisterComposablePartType(
			this ContainerBuilder builder,
			Type partType,
			Func<ExportDefinition, IEnumerable<Service>> exposedServicesMapper)
		{
			if (builder == null) throw new ArgumentNullException("builder");
			if (partType == null) throw new ArgumentNullException("partType");
			if (exposedServicesMapper == null) throw new ArgumentNullException("exposedServicesMapper");

			RegisterComposablePartDefinition(
				builder,
				AttributedModelServices.CreatePartDefinition(partType, null, true),
				exposedServicesMapper);
		}

		/// <summary>
		/// Register a MEF catalog.
		/// </summary>
		/// <param name="builder">The container builder.</param>
		/// <param name="catalog">The catalog to register.</param>
		/// <remarks>
		/// A simple heuristic/type scanning technique will be used to determine which MEF exports
		/// are exposed to other components in the Autofac container.
		/// </remarks>
		public static void RegisterComposablePartCatalog(this ContainerBuilder builder, ComposablePartCatalog catalog)
		{
			if (builder == null) throw new ArgumentNullException("builder");
			if (catalog == null) throw new ArgumentNullException("catalog");

			RegisterComposablePartCatalog(builder, catalog, DefaultExposedServicesMapper);
		}

		/// <summary>
		/// Register a MEF catalog.
		/// </summary>
		/// <param name="builder">The container builder.</param>
		/// <param name="catalog">The catalog to register.</param>
		/// <param name="interchangeServices">The services that will be exposed to other components in the container.</param>
		/// <remarks>
		/// Named and typed services only can be matched in the <paramref name="interchangeServices"/> collection.
		/// </remarks>
		public static void RegisterComposablePartCatalog(this ContainerBuilder builder, ComposablePartCatalog catalog, params Service[] interchangeServices)
		{
			if (builder == null) throw new ArgumentNullException("builder");
			if (catalog == null) throw new ArgumentNullException("catalog");
			if (interchangeServices == null) throw new ArgumentNullException("interchangeServices");

			RegisterComposablePartCatalog(builder, catalog, ed =>
				interchangeServices
					.OfType<TypedService>()
					.Where(s => ed.ContractName == AttributedModelServices.GetContractName(s.ServiceType))
					.Cast<Service>()
					.Union(
						interchangeServices
							.OfType<KeyedService>()
							.Where(s => ed.ContractName == (string)s.ServiceKey)
					));
		}

		/// <summary>
		/// Register a MEF catalog.
		/// </summary>
		/// <param name="builder">The container builder.</param>
		/// <param name="catalog">The catalog to register.</param>
		/// <param name="exposedServicesMapper">A mapping function to transform ExportDefinitions into Services.</param>
		public static void RegisterComposablePartCatalog(this ContainerBuilder builder, ComposablePartCatalog catalog, Func<ExportDefinition, IEnumerable<Service>> exposedServicesMapper)
		{
			if (builder == null) throw new ArgumentNullException("builder");
			if (catalog == null) throw new ArgumentNullException("catalog");
			if (exposedServicesMapper == null) throw new ArgumentNullException("exposedServicesMapper");

			// Support disposal of the catalog.
			builder.RegisterInstance(catalog).As(new UniqueService());

			foreach (var part in catalog.Parts)
			{
				RegisterComposablePartDefinition(builder, part, exposedServicesMapper);
			}
		}

		/// <summary>
		/// Register a MEF part definition.
		/// </summary>
		/// <param name="builder">The container builder.</param>
		/// <param name="partDefinition">The part definition to register.</param>
		/// <param name="exposedServicesMapper">A mapping function to transform ExportDefinitions into Services.</param>
		public static void RegisterComposablePartDefinition(this ContainerBuilder builder, ComposablePartDefinition partDefinition, Func<ExportDefinition, IEnumerable<Service>> exposedServicesMapper)
		{
			if (builder == null) throw new ArgumentNullException("builder");
			if (partDefinition == null) throw new ArgumentNullException("partDefinition");
			if (exposedServicesMapper == null) throw new ArgumentNullException("exposedServicesMapper");

			var partId = new UniqueService();
			var partRegistration = builder.Register(c => partDefinition.CreatePart())
				.OnActivating(e => SetPrerequisiteImports(e.Context, e.Instance))
				.OnActivated(e => SetNonPrerequisiteImports(e.Context, e.Instance))
				.As(partId);

			if (IsSharedInstance(partDefinition))
				partRegistration.SingleInstance();

			foreach (var iterExportDef in partDefinition.ExportDefinitions)
			{
				var exportDef = iterExportDef;
				var contractService = new ContractBasedService(exportDef.ContractName, GetTypeIdentity(exportDef));

				var exportId = new UniqueService();
				builder.Register(c =>
					{
						var p = ((ComposablePart)c.ResolveService(partId));
						return new Export(exportDef, () => p.GetExportedValue(exportDef));
					})
					.As(exportId, contractService)
					.ExternallyOwned()
					.WithMetadata(exportDef.Metadata);

				var additionalServices = exposedServicesMapper(exportDef).ToArray();

				if (additionalServices.Length > 0)
				{
					builder.Register(c => ((Export)c.ResolveService(exportId)).Value)
						.As(additionalServices)
						.ExternallyOwned()
						.WithMetadata(exportDef.Metadata);
				}
			}
		}

		static string GetTypeIdentity(ExportDefinition exportDef)
		{
			object typeIdentity;

			if (exportDef.Metadata.TryGetValue(CompositionConstants.ExportTypeIdentityMetadataName, out typeIdentity))
				return (string)typeIdentity;

			return string.Empty;
		}

		/// <summary>
		/// Locate all of the MEF exports registered as supplying contract type T.
		/// </summary>
		/// <typeparam name="T">The contract type.</typeparam>
		/// <param name="context">The context to resolve exports from.</param>
		/// <returns>A list of exports.</returns>
		public static IEnumerable<Export> ResolveExports<T>(this IComponentContext context)
		{
			return context.ResolveExports<T>(AttributedModelServices.GetContractName(typeof(T)));
		}

		/// <summary>
		/// Locate all of the MEF exports registered as supplying contract type T.
		/// </summary>
		/// <param name="contractName">The contract name.</param>
		/// <param name="context">The context to resolve exports from.</param>
		/// <returns>A list of exports.</returns>
		public static IEnumerable<Export> ResolveExports<T>(this IComponentContext context, string contractName)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			var ts = new TypedService(typeof(T));

			return context.ComponentRegistry
				.RegistrationsFor(new ContractBasedService(contractName, AttributedModelServices.GetTypeIdentity(typeof(T))))
				.Select(cpt => context.ResolveComponent(ts, cpt, Enumerable.Empty<Parameter>()))
				.Cast<Export>();
		}

		// Here we use the MEF default of Shared, but using the Autofac default may make more sense.
		static bool IsSharedInstance(ComposablePartDefinition part)
		{
			if (part.Metadata != null)
			{
				object pcp;

				if (part.Metadata.TryGetValue(CompositionConstants.PartCreationPolicyMetadataName, out pcp))
				{
					if (pcp != null && (CreationPolicy)pcp == CreationPolicy.NonShared)
						return false;
				}
			}

			return true;
		}

		static void AttachExport(IComponentRegistry registry, IComponentRegistration registration, ExportConfigurationBuilder exportConfiguration)
		{
			var contractService = new ContractBasedService(exportConfiguration.ContractName, exportConfiguration.ExportTypeIdentity);

			var service = registration.Services.FirstOrDefault();
			var rb =
				RegistrationBuilder.ForDelegate((c, p) =>
					new Export(
						new ExportDefinition(exportConfiguration.ContractName, exportConfiguration.Metadata),
						() => c.ResolveComponent(service, registration, new Parameter[0])))
				.As(contractService)
				.ExternallyOwned()
				.WithMetadata(exportConfiguration.Metadata);

			registry.Register(rb.CreateRegistration());
		}

		static void SetNonPrerequisiteImports(IComponentContext context, ComposablePart composablePart)
		{
			SetImports(context, composablePart, false);
			composablePart.Activate();
		}

		static void SetPrerequisiteImports(IComponentContext context, ComposablePart composablePart)
		{
			SetImports(context, composablePart, true);
		}

		static void SetImports(IComponentContext context, ComposablePart composablePart, bool prerequisite)
		{
			foreach (var import in composablePart
				.ImportDefinitions
				.Where(id => id.IsPrerequisite == prerequisite))
			{
				var cbid = import as ContractBasedImportDefinition;
				if (cbid == null)
					throw new NotSupportedException(string.Format("Import '{0}' is not supported: only contract-based imports are supported.", import));

				var exportsForContract = context.ResolveExports(cbid);
				composablePart.SetImport(import, exportsForContract);
			}
		}

		static Export[] ResolveExports(this IComponentContext context, ContractBasedImportDefinition cbid)
		{
			var componentsForContract = context.ComponentsForContract(cbid);

			var ts = new TypedService(typeof(Export));
			var exportsForContract = new Export[componentsForContract.Count];
			var emptyParams = Enumerable.Empty<Parameter>();
			for (int i = 0; i < componentsForContract.Count; i++)
				exportsForContract[i] = (Export)context.ResolveComponent(ts, componentsForContract[i], emptyParams);
			return exportsForContract;
		}

		static List<IComponentRegistration> ComponentsForContract(this IComponentContext context, ContractBasedImportDefinition cbid)
		{
			var contractService = new ContractBasedService(cbid.ContractName, cbid.RequiredTypeIdentity);
			var componentsForContract = context
				.ComponentRegistry
				.RegistrationsFor(contractService)
				.Where(cpt =>
					!cbid.RequiredMetadata
						.Except(cpt.Metadata.Select(m => new KeyValuePair<string, Type>(m.Key, m.Value.GetType())))
						.Any())
				.ToList();

			if (cbid.Cardinality == ImportCardinality.ExactlyOne && componentsForContract.Count == 0)
				throw new ComponentNotRegisteredException(contractService);

			return componentsForContract;
		}
	}
}
