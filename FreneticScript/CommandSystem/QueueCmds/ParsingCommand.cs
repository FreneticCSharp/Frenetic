﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class ParsingCommand: AbstractCommand
    {
        public ParsingCommand()
        {
            Name = "parsing";
            Arguments = "on/off";
            Description = "Sets whether the current queue should parse tags.";
            IsFlow = true;
            Asyncable = true;
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
            }
            else
            {
                bool modechoice = entry.GetArgument(0).ToLowerInvariant() == "on";
                entry.Queue.ParseTags = modechoice;
                if (entry.ShouldShowGood())
                {
                    entry.Good("Queue parsing <{text_color.emphasis}>" + (modechoice ? "enabled" : "disabled") + "<{text_color.base}>.");
                }
            }
        }
    }
}
