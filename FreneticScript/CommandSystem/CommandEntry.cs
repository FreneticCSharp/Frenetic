//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Text;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.ScriptSystems;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// All the information for a command being currently run.
    /// </summary>
    public class CommandEntry
    {
        /// <summary>
        /// The named argument ID for the name of a variable to save into.
        /// </summary>
        public const string SAVE_NAME_ARG_ID = "\0varname";

        /// <summary>
        /// The system controlling this CommandEntry.
        /// </summary>
        public ScriptEngine System;

        /// <summary>
        /// The relevant tag system.
        /// </summary>
        public TagHandler TagSystem
        {
            get
            {
                return System.TagSystem;
            }
        }

        /// <summary>
        /// The index of this entry in its block.
        /// </summary>
        public int OwnIndex;

        /// <summary>
        /// The original command input.
        /// </summary>
        public string CommandLine;

        /// <summary>
        /// A list of all commands that were inside this command originally.
        /// </summary>
        public List<CommandEntry> InnerCommandBlock = null;

        /// <summary>
        /// All 'named' arguments on this command entry.
        /// </summary>
        public Dictionary<string, Argument> NamedArguments;

        /// <summary>
        /// The start of this command's braced block.
        /// </summary>
        public int BlockStart;

        /// <summary>
        /// The end of this command's braced block.
        /// </summary>
        public int BlockEnd;

        /// <summary>
        /// Whether the &amp;waitable command entry should be waited for.
        /// <para>Simple property, equivalent to checking if <see cref="Prefix"/> is equal to <see cref="CommandPrefix.WAIT"/>.</para>
        /// </summary>
        public bool WaitFor
        {
            get
            {
                return Prefix == CommandPrefix.WAIT;
            }
        }

        /// <summary>
        /// The name of the creating script.
        /// </summary>
        public string ScriptName;

        /// <summary>
        /// The line number in the creating script.
        /// </summary>
        public int ScriptLine;

        /// <summary>
        /// The inner command block as its own script, generated by command execute methods where needed.
        /// </summary>
        public CommandScript BlockScript = null;

        /// <summary>
        /// The debug mode for this specific entry.
        /// </summary>
        public DebugMode DBMode = DebugMode.FULL;

        /// <summary>
        /// Gets the correct debug mode for this entry and a specified queue.
        /// </summary>
        /// <param name="queue">The relevant queue.</param>
        /// <returns>The correct debug mode (whatever gives least output).</returns>
        public DebugMode CorrectDBMode(CommandQueue queue)
        {
            if (queue.CurrentStackEntry.Debug.ShowsLessThan(DBMode))
            {
                return queue.CurrentStackEntry.Debug;
            }
            return DBMode;
        }
        
        /// <summary>
        /// Full constructor, recommended.
        /// </summary>
        public CommandEntry(string _commandline, int bstart, int bend, AbstractCommand _command, List<Argument> _arguments,
            string _name, CommandPrefix _prefix, string _script, int _line, string fairtabs, ScriptEngine sys)
            : this(_commandline, bstart, bend, _command, _arguments, _name, _prefix, _script, _line, fairtabs, new Dictionary<string, Argument>(), sys)
        {
        }

        /// <summary>
        /// Full constructor, recommended.
        /// </summary>
        public CommandEntry(string _commandline, int bstart, int bend, AbstractCommand _command, List<Argument> _arguments,
            string _name, CommandPrefix _prefix, string _script, int _line, string fairtabs, Dictionary<string, Argument> nameds, ScriptEngine sys)
        {
            BlockStart = bstart;
            BlockEnd = bend;
            CommandLine = _commandline;
            Command = _command;
            Arguments = _arguments;
            Name = _name;
            Prefix = _prefix;
            ScriptName = _script;
            ScriptLine = _line;
            FairTabulation = fairtabs;
            NamedArguments = nameds;
            System = sys;
            if (Command == null)
            {
                throw new Exception("Invalid Command (null!)");
            }
        }

        /// <summary>
        /// Use at own risk.
        /// </summary>
        public CommandEntry()
        {
        }

        /// <summary>
        /// Gets the full command string that represents this command.
        /// </summary>
        /// <returns>The full command string.</returns>
        public string FullString()
        {
            if (InnerCommandBlock == null)
            {
                return FairTabulation + CommandLine + ";\n";
            }
            else
            {
                string b = FairTabulation + CommandLine + "\n"
                    + FairTabulation + "{\n";
                foreach (CommandEntry entr in InnerCommandBlock)
                {
                    b += entr.FullString();
                }
                b += FairTabulation + "}\n";
                return b;
            }
        }

        /// <summary>
        /// Space to include in front of this entry when outputting it as text.
        /// </summary>
        public string FairTabulation = "";

        /// <summary>
        /// The command name input by the user.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// The command that should execute this input.
        /// </summary>
        public AbstractCommand Command;

        /// <summary>
        /// The arguments input by the user.
        /// </summary>
        public List<Argument> Arguments;

        /// <summary>
        /// What command prefix was used for this command, if any.
        /// </summary>
        public CommandPrefix Prefix;

        /// <summary>
        /// The save variable location, if any.
        /// </summary>
        public int SaveLoc = -1;

        /// <summary>
        /// Gets the save name for this entry, without parsing it.
        /// </summary>
        /// <param name="defaultval">The default value.</param>
        /// <param name="id">The ID of the saver.</param>
        /// <returns>The save name.</returns>
        public string GetSaveNameNoParse(string defaultval, string id = SAVE_NAME_ARG_ID)
        {
            if (NamedArguments.TryGetValue(id, out Argument arg))
            {
                return arg.ToString().ToLowerFast();
            }
            return defaultval;
        }

        /// <summary>
        /// Returns whether the entry is able to save a result.
        /// </summary>
        public bool CanSave
        {
            get
            {
                return SaveLoc >= 0;
            }
        }

        /// <summary>
        /// Saves the result of this command, if <see cref="AbstractCommand.SaveMode"/> is set.
        /// <para>Check <see cref="CanSave"/> to determine if a save is expected.</para>
        /// </summary>
        /// <param name="queue">The relevant queue.</param>
        /// <param name="resultObj">The result object.</param>
        public void SaveResult(CommandQueue queue, TemplateObject resultObj)
        {
            if (!CanSave)
            {
                return;
            }
            queue.SetLocalVar(SaveLoc, resultObj);
        }
        
        /// <summary>
        /// Gets a named argument with a specified name, handling any tags.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="name">The argument name.</param>
        /// <returns>The parsed argument.</returns>
        public TemplateObject GetNamedArgumentObject(CommandQueue queue, string name)
        {
            if (NamedArguments.TryGetValue(name, out Argument arg))
            {
                return arg.Parse(queue.Error, queue.CurrentStackEntry);
            }
            return null;
        }

        /// <summary>
        /// Gets an argument at a specified place, handling any tags.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="place">The argument place number.</param>
        /// <returns>The parsed argument.</returns>
        public TemplateObject GetArgumentObject(CommandQueue queue, int place)
        {
            return Arguments[place].Parse(queue.Error, queue.CurrentStackEntry);
        }

        /// <summary>
        /// Gets an argument at a specified place, handling any tags - returning a string.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="place">The argument place number.</param>
        /// <returns>The parsed argument as a string.</returns>
        public string GetArgument(CommandQueue queue, int place)
        {
            return Arguments[place].Parse(queue.Error, queue.CurrentStackEntry).ToString();
        }

        /// <summary>
        /// Generates an appropriate queue + entry error handling action. Use this to retain an error handler after command execution. Otherwise, use <see cref="CommandQueue.Error"/>.
        /// </summary>
        /// <param name="queue">The queue for the context.</param>
        /// <returns>The Action object.</returns>
        public Action<string> ContextualErrorHandler(CommandQueue queue)
        {
            return (s) => queue.HandleError(this, s);
        }

        /// <summary>
        /// Gets all arguments piled together into a string.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="index">The index to start at.</param>
        /// <returns>The combined string.</returns>
        public string AllArguments(CommandQueue queue, int index = 0)
        {
            StringBuilder result = new StringBuilder(CommandLine.Length);
            for (int i = index; i < Arguments.Count; i++)
            {
                result.Append(GetArgument(queue, i));
                if (i + 1 < Arguments.Count)
                {
                    result.Append(" ");
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets all arguments (without parsing) piled together into a string.
        /// </summary>
        /// <param name="index">The index to start at.</param>
        /// <returns>The combined string.</returns>
        public string AllOriginalArguments(int index = 0)
        {
            StringBuilder result = new StringBuilder(CommandLine.Length);
            for (int i = index; i < Arguments.Count; i++)
            {
                result.Append(Arguments[i]);
                if (i + 1 < Arguments.Count)
                {
                    result.Append(" ");
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Used to output requested information.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="text">The text to output.</param>
        public void InfoOutput(CommandQueue queue, string text)
        {
            queue.Engine.Context.WriteLine(text);
            if (queue.Outputsystem != null)
            {
                queue.Outputsystem.Invoke(text, MessageType.INFO);
            }
        }

        /// <summary>
        /// A helpful matcher to output short simple names.
        /// </summary>
        public static AsciiMatcher OutputableNameMatcher = new AsciiMatcher(
            (c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'));

        /// <summary>
        /// Generates the output prefix for debug output.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <returns>A proper output prefix.</returns>
        public string OutputPrefix(CommandQueue queue)
        {
            string outputableScriptName = OutputableNameMatcher.TrimToMatches(ScriptName);
            if (outputableScriptName.Length > 6)
            {
                outputableScriptName = outputableScriptName.Substring(0, 6);
            }
            return TextStyle.Minor + "[Q:" + TextStyle.Separate + queue.ID
                + TextStyle.Minor + ",S:" + TextStyle.Separate + outputableScriptName
                + TextStyle.Minor + ",L:" + TextStyle.Separate + ScriptLine + TextStyle.Minor + "] ";
        }

        /// <summary>
        /// Used to output a success message.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="text">The text to output.</param>
        public void GoodOutput(CommandQueue queue, string text)
        {
            if (CorrectDBMode(queue).ShouldShow(DebugMode.FULL))
            {
                text = OutputPrefix(queue) + TextStyle.Outgood + text;
                queue.Engine.Context.GoodOutput(text);
                if (queue.Outputsystem != null)
                {
                    queue.Outputsystem.Invoke(text, MessageType.GOOD);
                }
            }
        }

        /// <summary>
        /// Sets the data associated with this entry in the queue.
        /// </summary>
        /// <param name="queue">The queue holding the data.</param>
        /// <param name="x">The data to set to.</param>
        /// <returns>The entry data.</returns>
        public void SetData(CommandQueue queue, AbstractCommandEntryData x)
        {
            queue.CurrentStackEntry.EntryData[OwnIndex] = x;
        }

        /// <summary>
        /// Gets the data associated with this entry in the queue.
        /// </summary>
        /// <param name="queue">The queue holding the data.</param>
        /// <returns>The entry data.</returns>
        public AbstractCommandEntryData GetData(CommandQueue queue)
        {
            return queue.CurrentStackEntry.EntryData[OwnIndex];
        }

        /// <summary>
        /// Returns whether commands should output 'good' results.
        /// </summary>
        /// <param name="queue">The relevant queue.</param>
        /// <returns>Whether commands should output 'good' results.</returns>
        public bool ShouldShowGood(CommandQueue queue)
        {
            return CorrectDBMode(queue).ShouldShow(DebugMode.FULL);
        }

        /// <summary>
        /// Used to output a failure message. This is considered a 'warning' and will not induce an error.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="text">The text to output.</param>
        public void BadOutput(CommandQueue queue, string text)
        {
            if (CorrectDBMode(queue).ShouldShow(DebugMode.MINIMAL))
            {
                text = OutputPrefix(queue) + TextStyle.Outbad + "WARNING in script '" + TextStyle.Separate + ScriptName
                    + TextStyle.Outbad + "' on line " + TextStyle.Separate + (ScriptLine + 1) + TextStyle.Outbad + ": " + text;
                queue.Engine.Context.BadOutput(text);
                if (queue.Outputsystem != null)
                {
                    queue.Outputsystem.Invoke(text, MessageType.BAD);
                }
            }
        }
        
        /// <summary>
        /// Perfectly duplicates the command entry.
        /// </summary>
        /// <returns>The duplicate entry.</returns>
        public CommandEntry Duplicate()
        {
            return (CommandEntry)MemberwiseClone();
        }
        
        /// <summary>
        /// A lookup table for CIL variables.
        /// </summary>
        public Dictionary<string, SingleCILVariable> VarLookup;

        /// <summary>
        /// Gets the location of a variable from its name.
        /// Returns -1 if not found.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>The location of the variable, or -1.</returns>
        public int VarLoc(string name)
        {
            if (VarLookup.TryGetValue(name, out SingleCILVariable locVar))
            {
                return locVar.Index;
            }
            return -1;
        }
    }
}
