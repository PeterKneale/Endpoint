﻿using System;
using System.Diagnostics;

namespace Endpoint.Core.Services
{
    public class CommandService : ICommandService

    {
        public void Start(string arguments, string workingDirectory = null, bool waitForExit = true)
        {
            try
            {
                workingDirectory ??= Environment.CurrentDirectory;

                Console.WriteLine($"{arguments} in {workingDirectory}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Normal,
                        FileName = "cmd.exe",
                        Arguments = $"/C {arguments}",
                        WorkingDirectory = workingDirectory
                    }
                };

                process.Start();

                if (waitForExit)
                {
                    process.WaitForExit();
                }
            }
            catch (Exception e)
            {
                throw e;

            }

        }
    }
}
