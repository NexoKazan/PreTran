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
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ClusterixN.Common.Utils
{
    public static class AssemblyHelper
    {
        public static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyPath = Path.Combine(
                Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName).FullName,
                new AssemblyName(args.Name).Name + ".dll");

            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }

            var dbDir = Path.Combine(
                Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName,
                "Database");

            assemblyPath = Path.Combine(dbDir, new AssemblyName(args.Name).Name + ".dll");

            if (File.Exists(assemblyPath))
                return Assembly.LoadFrom(assemblyPath);


            foreach (var directory in Directory.EnumerateDirectories(dbDir))
            {
                assemblyPath = Path.Combine(directory, new AssemblyName(args.Name).Name + ".dll");

                if (File.Exists(assemblyPath))
                    return Assembly.LoadFrom(assemblyPath);
            }

            return null;
        }

        public static string GetAssemblyVersion(Assembly assembly)
        {
            string version = "0.0.0.0";
            if (assembly != null)
            {
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                version = fvi.FileVersion;
            }
            return version;
        }
    }
}
