﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.TagHandlers
{
    /// <summary>
    /// An abstract class, implementations are used as tag bases.
    /// </summary>
    public abstract class TemplateTags // TODO: Tags -> TagBase
    {
        /// <summary>
        /// The name of the tag set.
        /// </summary>
        public string Name = null;

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public abstract string Handle(TagData data);

        /// <summary>
        /// Usually returns the name of this tag base.
        /// </summary>
        /// <returns>The name of this tag base.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
