﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// An abstract class, implementations are used as tag bases.
    /// </summary>
    public abstract class TemplateTagBase
    {
        /// <summary>
        /// The name of the tag base.
        /// </summary>
        public string Name = null;
        
        /// <summary>
        /// TEMPORARY. // TODO: REMOVE ME!
        /// </summary>
        public bool CanSingle = false;
        
        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public virtual TemplateObject HandleOne(TagData data)
        {
            return Handle(data);
        }

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public abstract TemplateObject Handle(TagData data);

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
