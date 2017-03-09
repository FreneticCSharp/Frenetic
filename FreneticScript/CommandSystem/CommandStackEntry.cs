﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Represents a single entry in a command stack.
    /// TODO: Scrap this in favor of Compiled variant.
    /// </summary>
    public abstract class CommandStackEntry
    {
        /// <summary>
        /// The index of the currently running command.
        /// </summary>
        public int Index;

        /// <summary>
        /// All available commands.
        /// </summary>
        public CommandEntry[] Entries;
        
        /// <summary>
        /// All entry data available in this CommandStackEntry.
        /// </summary>
        public AbstractCommandEntryData[] EntryData;

        /// <summary>
        /// Run this when the CommandStackEntry STOPs.
        /// </summary>
        public Action Callback;
        
        /// <summary>
        /// Handles an error as appropriate to the situation, in the current queue, from the current command.
        /// </summary>
        /// <param name="queue">The associated queue.</param>
        /// <param name="entry">The command entry that errored.</param>
        /// <param name="message">The error message.</param>
        public void HandleError(CommandQueue queue, CommandEntry entry, string message)
        {
            StringBuilder stacktrace = new StringBuilder();
            stacktrace.Append("ERROR: \"" + message + "\"\n    in script '" + entry.ScriptName + "' at line " + (entry.ScriptLine + 1)
                + ": (" + entry.Name + ")\n");
            queue.WaitingOn = null;
            CommandStackEntry cse = queue.CommandStack.Count > 0 ? queue.CommandStack.Peek() : null;
            DebugMode dbmode = cse == null ? DebugMode.FULL : cse.Debug;
            while (cse != null)
            {
                for (int i = cse.Index; i < cse.Entries.Length; i++)
                {
                    CommandEntry entr = queue.GetCommand(i);
                    if (entr.Command is TryCommand &&
                        entr.Arguments[0].ToString() == "\0CALLBACK")
                    {
                        entry.Good(queue, "Force-exiting try block.");
                        // TODO: queue.SetVariable("stack_trace", new TextTag(stacktrace.ToString().Substring(0, stacktrace.Length - 1)));
                        cse.Index = i + 2;
                        throw new ErrorInducedException();
                    }
                }
                cse.Index = cse.Entries.Length + 1;
                queue.CommandStack.Pop();
                if (queue.CommandStack.Count > 0)
                {
                    cse = queue.CommandStack.Peek();
                    if (cse.Index <= cse.Entries.Length)
                    {
                        stacktrace.Append("    in script '" + cse.Entries[cse.Index - 1].ScriptName + "' at line " + (cse.Entries[cse.Index - 1].ScriptLine + 1)
                            + ": (" + cse.Entries[cse.Index - 1].Name + ")\n");
                    }
                }
                else
                {
                    cse = null;
                    break;
                }
            }
            message = stacktrace.ToString().Substring(0, stacktrace.Length - 1);
            if (dbmode <= DebugMode.MINIMAL)
            {
                queue.CommandSystem.Output.BadOutput(message);
                if (queue.Outputsystem != null)
                {
                    queue.Outputsystem.Invoke(message, MessageType.BAD);
                }
            }
            throw new ErrorInducedException("");
        }
        
        /// <summary>
        /// How much debug information this portion of the stack should show.
        /// </summary>
        public DebugMode Debug;
    }

    /// <summary>
    /// Represents the return value from a command stack run call.
    /// </summary>
    public enum CommandStackRetVal : byte
    {
        /// <summary>
        /// Tells the queue to continue.
        /// </summary>
        CONTINUE = 1,
        /// <summary>
        /// Tells the queue to wait a while.
        /// </summary>
        BREAK = 2,
        /// <summary>
        /// Tells the queue to stop entirely.
        /// </summary>
        STOP = 3
    }
}
