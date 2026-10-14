using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SalesTrading.UiTests
{
    internal static class OperationIntervalCheck
    {
        private static void Require(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        public static int Main(string[] args)
        {
            try
            {
                if (args.Length == 3 && args[0] == "--control")
                {
                    var control = new PlaybackControl(args[1]);
                    if (args[2] == "pause") control.SetPaused(true);
                    else if (args[2] == "resume") control.SetPaused(false);
                    else if (args[2] == "next") control.NextAction();
                    else throw new ArgumentException("Unknown playback test command");
                    return 0;
                }
                if (args.Length == 3 && args[0] == "--playback-worker")
                {
                    string path = args[1];
                    File.WriteAllText(Path.Combine(path, "ready-" + args[2]), "ready");
                    new OperationInterval(path).WaitAsync(() =>
                    {
                        if (File.Exists(Path.Combine(path, "cancel"))) throw new OperationCanceledException();
                    }, (waiting, milliseconds) => { }).GetAwaiter().GetResult();
                    File.WriteAllText(Path.Combine(path, "action-" + args[2]), "executed");
                    return 0;
                }
                if (args.Length == 2 && args[0] == "--worker")
                {
                    new OperationInterval(args[1]).WaitAsync(() =>
                    {
                        if (File.Exists(Path.Combine(args[1], "cancel"))) throw new OperationCanceledException();
                    }, (waiting, milliseconds) =>
                    {
                        File.WriteAllText(Path.Combine(args[1], waiting ? "waiting" : "done"), milliseconds.ToString());
                    }).GetAwaiter().GetResult();
                    return 0;
                }
                RunAsync().GetAwaiter().GetResult();
                Console.WriteLine("PASS: zero interval, live decrease/increase across processes, cancellation, invalid input, watchdog accounting, two-process pause, exclusive single step, resume and paused cancellation");
                return 0;
            }
            catch (OperationCanceledException)
            {
                return 3;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error);
                return 1;
            }
        }

        private static Process StartPlaybackWorker(string directory, string id)
        {
            return Process.Start(new ProcessStartInfo(typeof(OperationIntervalCheck).Assembly.Location,
                "--playback-worker \"" + directory + "\" " + id)
            {
                UseShellExecute = false, CreateNoWindow = true
            });
        }

        private static async Task WaitUntil(Func<bool> condition)
        {
            var elapsed = Stopwatch.StartNew();
            while (!condition())
            {
                Require(elapsed.ElapsedMilliseconds < 3000, "Playback check timed out");
                await Task.Delay(20);
            }
        }

        private static async Task RunAsync()
        {
            string directory = Path.Combine(Path.GetTempPath(), "stc-ui-interval-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                OperationInterval interval = new OperationInterval(directory);
                interval.Save(0);
                bool reported = false;
                await interval.WaitAsync(() => { }, (waiting, milliseconds) => reported = true);
                Require(!reported, "Zero must not add a wait");
                bool invalid = false;
                try { interval.Save(-1); } catch (ArgumentOutOfRangeException) { invalid = true; }
                Require(invalid, "Reject invalid interval");
                foreach (string scenario in new[] { "decrease", "increase", "cancel" })
                {
                    File.Delete(Path.Combine(directory, "waiting"));
                    File.Delete(Path.Combine(directory, "done"));
                    interval.Save(scenario == "increase" ? 1000 : 60000);
                    ProcessStartInfo start = new ProcessStartInfo(typeof(OperationIntervalCheck).Assembly.Location,
                        "--worker \"" + directory + "\"")
                    {
                        UseShellExecute = false, CreateNoWindow = true
                    };
                    using (Process process = Process.Start(start))
                    {
                        try
                        {
                            Stopwatch limit = Stopwatch.StartNew();
                            while (!File.Exists(Path.Combine(directory, "waiting")))
                            {
                                Require(!process.HasExited && limit.ElapsedMilliseconds < 3000, "Worker failed to enter wait");
                                await Task.Delay(20);
                            }
                            if (scenario == "cancel") File.WriteAllText(Path.Combine(directory, "cancel"), "stop");
                            else interval.Save(scenario == "increase" ? 1500 : 0);
                            if (scenario == "increase")
                            {
                                await Task.Delay(1050);
                                Require(!process.HasExited, "Live increase did not extend current wait");
                                File.WriteAllText(Path.Combine(directory, "operation-interval-ms.txt"), "invalid");
                            }
                            Require(process.WaitForExit(3000), "Worker did not finish bounded wait");
                            Require(process.ExitCode == (scenario == "cancel" ? 3 : 0), "Worker exit mismatch");
                            long waited = long.Parse(File.ReadAllText(Path.Combine(directory, "done")));
                            Require(scenario == "increase" ? waited >= 1450 : waited < 1000, "Live wait timing mismatch");
                        }
                        finally
                        {
                            if (!process.HasExited) process.Kill();
                        }
                    }
                }
                File.Delete(Path.Combine(directory, "cancel"));
                interval.Save(0);
                var playback = new PlaybackControl(directory);
                foreach (string completion in new[] { "resume", "cancel" })
                {
                    playback.SetPaused(true);
                    using (var first = StartPlaybackWorker(directory, completion + "-first"))
                    using (var second = StartPlaybackWorker(directory, completion + "-second"))
                    {
                        try
                        {
                            await WaitUntil(() => File.Exists(Path.Combine(directory, "ready-" + completion + "-first"))
                                && File.Exists(Path.Combine(directory, "ready-" + completion + "-second")));
                            await Task.Delay(150);
                            Require(Directory.GetFiles(directory, "action-" + completion + "-*").Length == 0,
                                "Paused workers must not mutate even at maximum speed");
                            playback.NextAction();
                            await WaitUntil(() => Directory.GetFiles(directory, "action-" + completion + "-*").Length == 1);
                            await Task.Delay(150);
                            Require(Directory.GetFiles(directory, "action-" + completion + "-*").Length == 1 && playback.IsPaused,
                                "One shared permit must release only one account and remain paused");
                            Require(playback.PausedMilliseconds >= 250, "Pause must be accounted separately from business time");
                            if (completion == "resume") playback.SetPaused(false);
                            else File.WriteAllText(Path.Combine(directory, "cancel"), "stop");
                            await WaitUntil(() => first.HasExited && second.HasExited);
                            Require(completion == "resume" ? first.ExitCode == 0 && second.ExitCode == 0
                                : first.ExitCode + second.ExitCode == 3, "Resume/cancel did not unblock the second account correctly");
                        }
                        finally
                        {
                            if (!first.HasExited) first.Kill();
                            if (!second.HasExited) second.Kill();
                        }
                    }
                }
                OperationWaitBudget budget = new OperationWaitBudget();
                budget.Begin();
                budget.Complete(250);
                Require(budget.ExcludedMilliseconds == 250, "Completed waits must be excluded");
                budget.Complete(250);
                Require(budget.ExcludedMilliseconds == 250, "Duplicate completion must not extend watchdog");
                budget.Begin();
                budget.Complete(999999);
                Require(budget.ExcludedMilliseconds == 60350, "Watchdog wait allowance must be bounded");
            }
            finally
            {
                // 仅清理本测试随机创建的临时目录。
                Directory.Delete(directory, true);
            }
        }
    }
}
