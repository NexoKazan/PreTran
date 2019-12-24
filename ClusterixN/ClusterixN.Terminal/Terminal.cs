#region Copyright
/*
 * Copyright 2017 Roman Klassen
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy
 * of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 */
#endregion

﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ClusterixN.Terminal
{
    public class Terminal
    {
        private readonly List<TerminalCommandBase> _commands;
        private bool _listen = true;

        public Terminal()
        {
            _commands = new List<TerminalCommandBase>
            {
                new TerminalCommandBase("exit", "Завершение работы терминала") {ActionEvent = ExitCommandActionEvent},
                new TerminalCommandBase("help", "Вывод доступных команд") {ActionEvent = HelpCommandActionEvent}
            };
        }

        private void HelpCommandActionEvent(object sender, EventArgs e)
        {
            foreach (var command in _commands)
            {
                Console.WriteLine("{0} - {1}", command.Command, command.Description);
                foreach (var argument in command.Arguments)
                    Console.WriteLine("\t{0} - {1}", argument.Argument, argument.Description);
            }
        }

        private void ExitCommandActionEvent(object sender, EventArgs eventArgs)
        {
            _listen = false;
        }

        public void RegisterComand(TerminalCommandBase command)
        {
            _commands.Add(command);
        }

        public void Start()
        {
            while (_listen)
            {
                Console.Write("# ");
                var command = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(command))
                    ProcessCommand(command);
            }
        }

        private void ProcessCommand(string command)
        {
            var terminalCommand = _commands.FirstOrDefault(
                c => command.StartsWith(c.Command, StringComparison.CurrentCultureIgnoreCase));
            if (terminalCommand != null)
            {
                ParseCommandArguments(command, terminalCommand);
                var error = CheckCommandError(terminalCommand);

                if (!error)
                    try
                    {
                        terminalCommand.DoAction();
                    }
                    catch (Exception exception)
                    {
                        OnCommandError(exception);
                    }
            }
            else
            {
                Console.WriteLine("Команда {0} не найдена", command);
            }
        }

        private void ParseCommandArguments(string command, TerminalCommandBase terminalCommand)
        {
            terminalCommand.ClearArguments();
            foreach (var argument in terminalCommand.Arguments)
                if (CheckArgumentCointains(command, argument))
                {
                    var start = FindArgumentStart(command, argument.Argument);
                    if (start > 0)
                    {
                        argument.IsSet = true;
                        var args = terminalCommand.Arguments.Select(a => a.Argument).ToArray();
                        var end = FindArgumentEnd(command, start, args);
                        if (end > 0)
                            argument.Value = command.Substring(start, end - start);
                    }
                }
        }

        private bool CheckCommandError(TerminalCommandBase terminalCommand)
        {
            var error = false;
            foreach (var argument in terminalCommand.Arguments)
            {
                var argError = false;
                if (argument.IsRequered && !argument.IsSet)
                {
                    Console.WriteLine("Не указан аргумент: {0}", argument.Argument);
                    argError = error = true;
                }
                if (!argError && !argument.Validate())
                {
                    Console.WriteLine("Неправильное значение аргумента: {0} = {1}", argument.Argument, argument.Value);
                    Console.WriteLine("Правильный тип: {0}", argument.ValueType);
                    error = true;
                }
            }
            return error;
        }

        private bool CheckArgumentCointains(string command, CommandArgument argument)
        {
            if (command.Contains(argument.Argument)) return true;

            foreach (var argumentSynonim in argument.ArgumentSynonims)
                if (command.Contains(argumentSynonim)) return true;

            return false;
        }

        private int FindArgumentStart(string command, string argument)
        {
            return command.IndexOf(argument, StringComparison.InvariantCultureIgnoreCase) + argument.Length;
        }

        private int FindArgumentEnd(string command, int startIndex, string[] arguments)
        {
            var cmd = command.Substring(startIndex);
            var index = -1;
            foreach (var argument in arguments)
            {
                var ind = cmd.IndexOf(argument, StringComparison.InvariantCultureIgnoreCase);
                if (ind < index || index == -1) index = ind;
            }
            return index > 0 ? startIndex + index : command.Length;
        }

        public event EventHandler<TerminalExceptionEventArg> CommandError;

        protected virtual void OnCommandError(Exception ex)
        {
            CommandError?.Invoke(this, new TerminalExceptionEventArg {Exception = ex});
        }
    }
}