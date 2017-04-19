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

using System.Configuration;

namespace Revenj.Extensibility.Autofac.Configuration
{

    /// <summary>
    /// Element describing an additional configuration file.
    /// </summary>
    public class FileElement : ConfigurationElement
    {
        const string FilenameAttributeName = "name";
        const string SectionAttributeName = "section";
        internal const string Key = FilenameAttributeName;

        /// <summary>
        /// Gets the filename of the file.
        /// </summary>
        /// <value>The filename.</value>
        [ConfigurationProperty(FilenameAttributeName, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this[FilenameAttributeName];
            }
        }

        /// <summary>
        /// Gets the section name of the section in the configuration
        /// file.
        /// </summary>
        /// <value>The section name.</value>
        [ConfigurationProperty(SectionAttributeName, IsRequired = false)]
        public string Section
        {
            get
            {
                return (string)this[SectionAttributeName];
            }
        }
    }

}
