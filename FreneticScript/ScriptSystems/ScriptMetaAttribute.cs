﻿//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2018 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticScript.ScriptSystems
{
    /// <summary>
    /// Base class for script meta attribute types.
    /// </summary>
    public abstract class ScriptMetaAttribute : Attribute
    {
        /// <summary>
        /// The group this meta documentation piece belongs to.
        /// </summary>
        public string Group;

        /// <summary>
        /// Any other information/notes for this meta documentation piece.
        /// </summary>
        public string[] Others;
    }
}