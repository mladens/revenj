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

using System.Collections.Generic;
using Revenj.Extensibility.Autofac.Core;

namespace Revenj.Extensibility.Autofac.Configuration
{

    /// <summary>
    /// A collection of parameter elements.
    /// </summary>
    public class ParameterElementCollection : NamedConfigurationElementCollection<ParameterElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterElementCollection"/> class.
        /// </summary>
        public ParameterElementCollection()
            : base("parameter", ParameterElement.Key)
        {
        }

        /// <summary>
        /// Convert to the Autofac parameter type.
        /// </summary>
        /// <returns>The parameters represented by this collection.</returns>
        public IEnumerable<Parameter> ToParameters()
        {
            foreach (var parameter in this)
            {
                var localParameter = parameter;
                yield return new ResolvedParameter(
                    (pi, c) => pi.Name == localParameter.Name,
                    (pi, c) => TypeManipulation.ChangeToCompatibleType(localParameter.CoerceValue(), pi.ParameterType));
            }
        }
    }

}
