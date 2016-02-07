﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name repeat
    // @Arguments <times to repeat>/stop/next
    // @Short Executes the following block of commands a specified number of times.
    // @Updated 2014/06/23
    // @Authors mcmonkey
    // @Group Queue
    // @Braces true
    // @Description
    // The repeat command will loop the given number of times and execute the included command block
    // each time it loops.
    // It can also be used to stop the looping via the 'stop' argument, or to jump to the next
    // entry in the list and restart the command block via the 'next' argument.
    // TODO: Explain more!
    // @Example
    // // This example runs through the list and echos "1/3", then "2/3", then "3/3" back to the console.
    // repeat 3
    // {
    //     echo "<{var[repeat_index]}>/<{var[repeat_total]}>"
    // }
    // @Example
    // // This example runs through the list and echos "1", then "1r", then "2", then "3", then "3r" back to the console.
    // repeat 3
    // {
    //     echo "<{var[repeat_index]}>"
    //     if <{var[repeat_index].equals[2]}>
    //     {
    //         repeat next
    //     }
    //     echo "<{var[repeat_index]}>r"
    // }
    // @Example
    // // This example runs through the list and echos "1", then "2", then stops early.
    // repeat 3
    // {
    //     if <{var[repeat_index].equals[3]}>
    //     {
    //         repeat stop
    //     }
    //     echo "<{var[repeat_index]}>"
    // }
    // @Example
    // // TODO: More examples!
    // @Var repeat_index TextTag returns what iteration (numeric) the repeat is on.
    // @Var repeat_total TextTag returns what iteration (numeric) the repeat is aiming for, and will end on if not stopped early.
    // -->
    class RepeatCommandData : AbstractCommandEntryData
    {
        public int Index;
        public int Total;
        public override AbstractCommandEntryData Duplicate()
        {
            RepeatCommandData toret = new RepeatCommandData();
            toret.Index = Index;
            toret.Total = Total;
            return toret;
        }
    }

    class RepeatCommand : AbstractCommand
    {
        public RepeatCommand()
        {
            Name = "repeat";
            Arguments = "<times to repeat>/stop/next";
            Description = "Executes the following block of commands a specified number of times.";
            IsFlow = true;
            Asyncable = true;
        }

        public static int StringToInt(string input)
        {
            int output = 0;
            int.TryParse(input, out output);
            return output;
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
            }
            else
            {
                string count = entry.GetArgument(0);
                if (count == "\0CALLBACK")
                {
                    if (entry.BlockOwner.Command.Name == "repeat" || entry.BlockOwner.Block == null || entry.BlockOwner.Block.Count == 0
                        || entry.BlockOwner.Block[entry.BlockOwner.Block.Count - 1] != entry)
                    {
                        RepeatCommandData data = (RepeatCommandData)entry.BlockOwner.Data;
                        data.Index++;
                        if (data.Index > data.Total)
                        {
                            entry.Good("Repeating ending, reached target.");
                        }
                        else
                        {
                            entry.Good("Repeating at index <{text_color.emphasis}>" + data.Index + "/" + data.Total + "<{text_color.base}>...");
                            entry.Queue.SetVariable("repeat_index", new TextTag(data.Index.ToString()));
                            entry.Queue.SetVariable("repeat_total", new TextTag(data.Total.ToString()));
                            entry.Queue.AddCommandsNow(entry.BlockOwner.Block);
                        }
                    }
                    else
                    {
                        entry.Error("Repeat CALLBACK invalid: not a real callback!");
                    }
                }
                else if (count.ToLower() == "stop")
                {
                    bool hasnext = false;
                    for (int i = 0; i < entry.Queue.CommandList.Length; i++)
                    {
                        if (entry.Queue.GetCommand(i).Command is RepeatCommand &&
                            entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                        {
                            hasnext = true;
                            break;
                        }
                    }
                    if (hasnext)
                    {
                        entry.Good("Stopping repeat loop.");
                        while (entry.Queue.CommandList.Length > 0)
                        {
                            if (entry.Queue.GetCommand(0).Command is RepeatCommand &&
                                entry.Queue.GetCommand(0).Arguments[0].ToString() == "\0CALLBACK")
                            {
                                entry.Queue.RemoveCommand(0);
                                break;
                            }
                            entry.Queue.RemoveCommand(0);
                        }
                    }
                    else
                    {
                        entry.Error("Cannot stop repeat: not in one!");
                    }
                }
                else if (count.ToLower() == "next")
                {
                    bool hasnext = false;
                    for (int i = 0; i < entry.Queue.CommandList.Length; i++)
                    {
                        if (entry.Queue.GetCommand(i).Command is RepeatCommand &&
                            entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                        {
                            hasnext = true;
                            break;
                        }
                    }
                    if (hasnext)
                    {
                        entry.Good("Skipping to next repeat entry...");
                        while (entry.Queue.CommandList.Length > 0)
                        {
                            if (entry.Queue.GetCommand(0).Command is RepeatCommand &&
                                entry.Queue.GetCommand(0).Arguments[0].ToString() == "\0CALLBACK")
                            {
                                break;
                            }
                            entry.Queue.RemoveCommand(0);
                        }
                    }
                    else
                    {
                        entry.Error("Cannot stop repeat: not in one!");
                    }
                }
                else
                {
                    int target = StringToInt(count);
                    if (target <= 0)
                    {
                        entry.Good("Not repeating.");
                        return;
                    }
                    RepeatCommandData data = new RepeatCommandData();
                    data.Total = target;
                    data.Index = 1;
                    entry.Data = data;
                    if (entry.Block != null)
                    {
                        entry.Good("Repeating <{text_color.emphasis}>" + target + "<{text_color.base}> times...");
                        CommandEntry callback = new CommandEntry("repeat \0CALLBACK", null, entry,
                            this, new List<Argument>() { CommandSystem.TagSystem.SplitToArgument("\0CALLBACK", true) }, "repeat", 0, entry.ScriptName, entry.ScriptLine);
                        entry.Block.Add(callback);
                        entry.Queue.SetVariable("repeat_index", new TextTag("1"));
                        entry.Queue.SetVariable("repeat_total", new TextTag(target.ToString()));
                        entry.Queue.AddCommandsNow(entry.Block);
                    }
                    else
                    {
                        entry.Error("Repeat invalid: No block follows!");
                    }
                }
            }
        }
    }
}