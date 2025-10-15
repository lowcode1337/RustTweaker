using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SharpCompress.Archives;
using SharpCompress.Common;

class Program
{
    static async Task Main()
    {
        const string inputFile = "RustTweaker.exe";
        const string outputFile = "RustTweaker_Patched.exe";
        const string downloadUrl = "https://rusttweaker.com/RustTweaker_v1.0.0.rar";
        const string rarFile = "RustTweaker_v1.0.0.rar";

        if (!File.Exists(inputFile))
        {
            Console.WriteLine("RustTweaker.exe not found. Downloading...");
            await DownloadFile(downloadUrl, rarFile);
            Console.WriteLine("Download complete. Extracting...");
            ExtractRar(rarFile);
            Console.WriteLine("Extraction complete.");

            if (!File.Exists(inputFile))
            {
                Console.WriteLine("Error: RustTweaker.exe not found after extraction.");
                Console.ReadKey();
                return;
            }
        }

        Console.WriteLine("Starting patch process...");

        var module = ModuleDefinition.ReadModule(inputFile);

        var loginForm = module.Types.First(t => t.Name == "LoginForm");
        var checkerMethod = loginForm.Methods.First(m => m.Name == "Сhecker");

        checkerMethod.Body.Instructions.Clear();
        checkerMethod.Body.Variables.Clear();

        var il = checkerMethod.Body.GetILProcessor();

        var formType = module.AssemblyReferences
            .SelectMany(a => module.AssemblyResolver.Resolve(a).MainModule.Types)
            .First(t => t.FullName == "System.Windows.Forms.Form");

        var dialogResultSetter = module.ImportReference(
            formType.Properties.First(p => p.Name == "DialogResult").SetMethod);

        var controlType = module.AssemblyReferences
            .SelectMany(a => module.AssemblyResolver.Resolve(a).MainModule.Types)
            .First(t => t.FullName == "System.Windows.Forms.Control");

        var hideMethod = module.ImportReference(
            controlType.Methods.First(m => m.Name == "Hide" && m.Parameters.Count == 0));

        var mscorlibRef = module.AssemblyReferences.First(a => a.Name == "mscorlib");
        var mscorlib = module.AssemblyResolver.Resolve(mscorlibRef);
        var taskType = mscorlib.MainModule.Types.First(t => t.FullName == "System.Threading.Tasks.Task");
        var taskCompletedTask = module.ImportReference(
            taskType.Properties.First(p => p.Name == "CompletedTask").GetMethod);

        var notificationService = module.Types.First(t => t.Name == "NotificationService");
        var showNotification = module.ImportReference(
            notificationService.Methods.First(m => m.Name == "ShowNotification" && m.Parameters.Count == 2));

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Callvirt, dialogResultSetter);

        il.Emit(OpCodes.Ldstr, "Welcome! RustTweaker - patched by lowcode1337 (https://github.com/lowcode1337)");
        il.Emit(OpCodes.Ldc_I4, 2000);
        il.Emit(OpCodes.Call, showNotification);
        il.Emit(OpCodes.Pop);

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, hideMethod);

        il.Emit(OpCodes.Call, taskCompletedTask);
        il.Emit(OpCodes.Ret);

        var mainForm = module.Types.First(t => t.Name == "MainForm");
        var initMethod = mainForm.Methods.First(m => m.Name == "InitializeComponent");

        foreach (var instruction in initMethod.Body.Instructions)
        {
            if (instruction.OpCode == OpCodes.Ldstr)
            {
                string str = instruction.Operand as string;
                if (str != null && (str.Contains("RustTweaker") || str.Contains("Rust Tweaker")))
                {
                    instruction.Operand = "RustTweaker - patched by lowcode1337 (https://github.com/lowcode1337)";
                    Console.WriteLine($"Changed string: (0x{instruction.Offset:x8}){str} -> {instruction.Operand}");
                }
            }
        }

        module.Write(outputFile);
        Console.WriteLine("Patched successfully!");
        Console.WriteLine($"Run {outputFile} to use the patched version.");
        Console.ReadKey();
    }

    static async Task DownloadFile(string url, string outputPath)
    {
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromMinutes(10);

            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1;

            using (var contentStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                var buffer = new byte[8192];
                var totalRead = 0L;
                int read;

                while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read);
                    totalRead += read;

                    if (canReportProgress)
                    {
                        var progress = (int)((totalRead * 100) / totalBytes);
                        Console.Write($"\rProgress: {progress}%");
                    }
                }
                Console.WriteLine();
            }
        }
    }

    static void ExtractRar(string rarPath)
    {
        using (var archive = ArchiveFactory.Open(rarPath))
        {
            foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
            {
                Console.WriteLine($"Extracting: {entry.Key}");
                entry.WriteToDirectory(".", new ExtractionOptions()
                {
                    ExtractFullPath = false,
                    Overwrite = true
                });
            }
        }
    }
}