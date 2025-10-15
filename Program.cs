using ConfuserEx_Dynamic_Unpacker.Protections;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ConfuserEx_Dynamic_Unpacker
{
    class Program
    {
        public static ModuleDefMD module;
        public static Assembly asm;
        public static bool veryVerbose = false;

        static void Main(string[] args)
        {
            string inputFile = "RustTweaker.exe";
            string decodedFile = "RustTweaker_Decompiled.exe";
            string outputFile = "RustTweaker_Patched.exe";

            if (!File.Exists(inputFile))
            {
                Console.WriteLine("[!] RustTweaker.exe not found!");
                Console.ReadKey();
                return;
            }

            try
            {
                Console.WriteLine("[*] Loading assembly...");
                module = ModuleDefMD.Load(inputFile);

                Console.WriteLine("\n[*] Removing ConfuserEx protections...");
                DeobfuscateConfuserEx();

                SaveModule(decodedFile);

                Console.WriteLine("\n[*] Patching authentication...");
                PatchAuthentication(decodedFile, outputFile);

                Console.WriteLine("\n╔══════════════════════════════════════════════════╗");
                Console.WriteLine("║ [✓] SUCCESS! RustTweaker fully bypassed          ║");
                Console.WriteLine("║ [✓] Run: RustTweaker_Patched.exe                 ║");
                Console.WriteLine("║ [✓] Patched by: https://github.com/lowcode1337   ║");
                Console.WriteLine("╚══════════════════════════════════════════════════╝\n");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[!] Fatal Error: {ex.Message}");
                Console.ForegroundColor = ConsoleColor.Green;
            }

            Console.ReadKey();
        }

        static void DeobfuscateConfuserEx()
        {
            RemoveAntiTamper();
            RemovePacker();
            CleanControlFlow();
            CleanProxyCalls();
            DecryptStrings();
        }

        static void RemoveAntiTamper()
        {
            try
            {
                var isTampered = AntiTamper.IsTampered(module);
                if (isTampered == true)
                {
                    Console.WriteLine("  [+] Anti-Tamper detected");

                    var stream = module.Metadata.PEImage.CreateReader();
                    byte[] rawBytes = stream.ReadBytes((int)stream.Length);

                    module = AntiTamper.UnAntiTamper(module, rawBytes);
                    Console.WriteLine("  [✓] Anti-Tamper removed successfully");
                }
                else
                {
                    Console.WriteLine("  [i] No Anti-Tamper detected");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [!] Anti-Tamper removal error: {ex.Message}");
            }
        }

        static void RemovePacker()
        {
            try
            {
                if (Packer.IsPacked(module))
                {
                    Console.WriteLine("  [+] Compressor/Packer detected");

                    if (StaticPacker.Run(module))
                    {
                        RemoveAntiTamper();
                        module.EntryPoint = module.ResolveToken(StaticPacker.epToken) as MethodDef;
                        Console.WriteLine("  [✓] Compressor removed successfully");
                    }
                    else
                    {
                        Console.WriteLine("  [!] Static unpacking failed");
                    }
                }
                else
                {
                    Console.WriteLine("  [i] No packer detected");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [!] Packer removal error: {ex.Message}");
            }
        }

        static void CleanControlFlow()
        {
            try
            {
                Console.WriteLine("  [+] Cleaning control flow obfuscation...");
                ControlFlowRun.cleaner(module);
                Console.WriteLine("  [✓] Control flow cleaned");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [!] Control flow cleaning error: {ex.Message}");
            }
        }

        static void CleanProxyCalls()
        {
            try
            {
                Console.WriteLine("  [+] Removing proxy calls...");
                int removed = ReferenceProxy.ProxyFixer(module);
                Console.WriteLine($"  [✓] Removed {removed} proxy calls");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [!] Proxy removal error: {ex.Message}");
            }
        }

        static void DecryptStrings()
        {
            try
            {
                Console.WriteLine("  [+] Decrypting strings...");
                int decrypted = StaticStrings.Run(module);
                Console.WriteLine($"  [✓] Decrypted {decrypted} strings");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [!] String decryption error: {ex.Message}");
            }
        }

        static void PatchAuthentication(string inputFile, string outputFile)
        {
            try
            {
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

                il.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
                il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4_1);
                il.Emit(Mono.Cecil.Cil.OpCodes.Callvirt, dialogResultSetter);

                il.Emit(Mono.Cecil.Cil.OpCodes.Ldstr, "Welcome! RustTweaker - patched by lowcode1337 (https://github.com/lowcode1337)");
                il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, 2000);
                il.Emit(Mono.Cecil.Cil.OpCodes.Call, showNotification);
                il.Emit(Mono.Cecil.Cil.OpCodes.Pop);

                il.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
                il.Emit(Mono.Cecil.Cil.OpCodes.Callvirt, hideMethod);

                il.Emit(Mono.Cecil.Cil.OpCodes.Call, taskCompletedTask);
                il.Emit(Mono.Cecil.Cil.OpCodes.Ret);

                var mainForm = module.Types.First(t => t.Name == "MainForm");
                var initMethod = mainForm.Methods.First(m => m.Name == "InitializeComponent");

                foreach (var instruction in initMethod.Body.Instructions)
                {
                    if (instruction.OpCode == Mono.Cecil.Cil.OpCodes.Ldstr)
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

            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [!] Authentication patch error: {ex.Message}");
            }
        }

        static void SaveModule(string outputPath)
        {
            try
            {
                var writerOptions = new ModuleWriterOptions(module);
                writerOptions.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
                writerOptions.Logger = DummyLogger.NoThrowInstance;

                module.Write(outputPath, writerOptions);
                Console.WriteLine("  [✓] File saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [!] Save error: {ex.Message}");
                throw;
            }
        }
    }
}