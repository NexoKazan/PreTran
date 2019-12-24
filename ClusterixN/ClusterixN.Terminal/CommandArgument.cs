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

﻿namespace ClusterixN.Terminal
{
    public class CommandArgument
    {
        public CommandArgument(string argument, string[] argumentSynonims, string description, bool isRequered,
            CommandArgumentType valueType)
        {
            Argument = argument;
            ArgumentSynonims = argumentSynonims;
            Description = description;
            IsRequered = isRequered;
            ValueType = valueType;
        }

        public CommandArgument(string argument, string description, bool isRequered, CommandArgumentType valueType)
            : this(argument, new string[0], description, isRequered, valueType)
        {
        }

        public string Argument { get; protected set; }

        public string[] ArgumentSynonims { get; protected set; }

        public bool IsRequered { get; protected set; }

        public bool IsSet { get; set; }

        public string Description { get; protected set; }

        public string Value { get; set; }

        public CommandArgumentType ValueType { get; protected set; }

        public bool Validate()
        {
            switch (ValueType)
            {
                case CommandArgumentType.Empty:
                case CommandArgumentType.String:
                    return true;
                case CommandArgumentType.Int:
                    if (!IsRequered && !IsSet) return true;
                    int val;
                    if (int.TryParse(Value, out val)) return true;
                    return false;
                case CommandArgumentType.Float:
                    float fval;
                    if (float.TryParse(Value, out fval)) return true;
                    return false;
                default:
                    return false;
            }
        }
    }
}