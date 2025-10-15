using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ConfuserEx_Dynamic_Unpacker.Protections
{
    class Packer
    {
        public static bool IsPacked(ModuleDefMD module)
        {
            for (uint rid = 1; rid <= module.Metadata.TablesStream.FileTable.Rows; rid++)
            {
                RawFileRow row;
                module.TablesStream.TryReadFileRow(rid, out row);
                string name = module.StringsStream.ReadNoNull(row.Name);
                if (name == "koi")
                {
                    return true;
                }
            }
            return false;
        }

        private static byte[] initialValue;
        public static int epToken;

        private static void arrayFinder(MethodDef entryPoint, Local loc)
        {
            for (int i = 0; i < entryPoint.Body.Instructions.Count; i++)
            {
                if (entryPoint.Body.Instructions[i].IsStloc())
                {
                    if (entryPoint.Body.Instructions[i].GetLocal(entryPoint.Body.Variables) == loc)
                    {
                        if (entryPoint.Body.Instructions[i - 1].OpCode == OpCodes.Call && entryPoint.Body.Instructions[i - 2].OpCode == OpCodes.Ldtoken)
                        {
                            var tester = entryPoint.Body.Instructions[i - 2].Operand as FieldDef;
                            initialValue = tester.InitialValue;
                            break;
                        }
                    }
                }
            }
        }

        public static void findLocal(ModuleDefMD module, Assembly asm)
        {
            var manifestModule = asm.ManifestModule;
            MethodDef entryPoint = module.EntryPoint;
            var tester = entryPoint.Body.Variables.Where(i => i.Type.Namespace == "System.Runtime.InteropServices" && i.Type.TypeName == "GCHandle").ToArray();

            if (tester.Length != 0)
            {
                Local loc = tester[0];
                for (int i = 0; i < entryPoint.Body.Instructions.Count; i++)
                {
                    if (entryPoint.Body.Instructions[i].IsStloc())
                    {
                        if (entryPoint.Body.Instructions[i].GetLocal(entryPoint.Body.Variables) == loc)
                        {
                            if (entryPoint.Body.Instructions[i - 1].OpCode == OpCodes.Call)
                            {
                                if (entryPoint.Body.Instructions[i - 2].IsLdcI4())
                                {
                                    if (entryPoint.Body.Instructions[i - 3].IsLdloc())
                                    {
                                        MethodDef decryptMethod = entryPoint.Body.Instructions[i - 1].Operand as MethodDef;
                                        var dec = manifestModule.ResolveMethod(decryptMethod.MDToken.ToInt32());
                                        object[] param = new object[2];
                                        param[1] = (uint)entryPoint.Body.Instructions[i - 2].GetLdcI4Value();
                                        Local loc2 = entryPoint.Body.Instructions[i - 3].GetLocal(entryPoint.Body.Variables);
                                        arrayFinder(entryPoint, loc2);
                                        uint[] decoded = new uint[initialValue.Length / 4];
                                        Buffer.BlockCopy(initialValue, 0, decoded, 0, initialValue.Length);
                                        param[0] = decoded;

                                        GCHandle aaaaa = (GCHandle)dec.Invoke(null, param);

                                        Program.module = ModuleDefMD.Load((byte[])aaaaa.Target);
                                        var key = manifestModule.ResolveSignature(0x11000001);

                                        epToken = ((int)key[0] | (int)key[1] << 8 | (int)key[2] << 16 | (int)key[3] << 24);
                                        Program.module.EntryPoint = Program.module.ResolveToken(epToken) as MethodDef;
                                        Program.asm = Assembly.Load((byte[])aaaaa.Target);

                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}