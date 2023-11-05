using System.Diagnostics;
using Quartz;

namespace SmartCityBackend.Infrastructure.Jobs;

public class TrainModelJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // Initialize the Conda environment
        Process envProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "conda",
                Arguments = "init /bin/bash",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        envProcess.Start();
        envProcess.WaitForExit();
        envProcess.Close();

        // Create and activate the Conda environment
        envProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "conda",
                Arguments = "env create --name codebooq --file Scripts/env.txt",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        envProcess.Start();
        envProcess.WaitForExit();
        envProcess.Close();

        // Activate the Conda environment and run your Python script
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "conda",
                Arguments = "run -n codebooq python3 Scripts/linear_model.py",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();
        
        int exitCode = process.ExitCode;
        if (exitCode == 0)
        {
            // The Python script executed successfully.
            Console.WriteLine("Python script executed successfully.");
        }
        else
        {
            // The Python script encountered an error.
            Console.WriteLine($"Python script encountered an error. Exit code: {exitCode}");
        }
    }
}