using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Reflection;

namespace ConfuserEx_Dynamic_Unpacker.Protections
{
    class Constants
    {
        public static int constants(ModuleDefMD module, Assembly asm)
        {
            int amount = 0;
            var manifestModule = asm.ManifestModule;
            foreach (TypeDef types in module.GetTypes())
            {
                foreach (MethodDef methods in types.Methods)
                {
                    if (!methods.HasBody) continue;
                    for (int i = 0; i < methods.Body.Instructions.Count; i++)
                    {
                        if (methods.Body.Instructions[i].OpCode == OpCodes.Call && methods.Body.Instructions[i].Operand.ToString().Contains("tring>") && methods.Body.Instructions[i].Operand is MethodSpec)
                        {
                            if (methods.Body.Instructions[i - 1].IsLdcI4())
                            {
                                MethodSpec methodSpec = methods.Body.Instructions[i].Operand as MethodSpec;

                                uint param1 = (uint)methods.Body.Instructions[i - 1].GetLdcI4Value();
                                var value = (string)manifestModule.ResolveMethod(methodSpec.MDToken.ToInt32()).Invoke(null, new object[] { (uint)param1 });
                                methods.Body.Instructions[i].OpCode = OpCodes.Nop;
                                methods.Body.Instructions[i - 1].OpCode = OpCodes.Ldstr;
                                methods.Body.Instructions[i - 1].Operand = value;
                                amount++;
                            }
                        }
                    }
                }
            }
            return amount;
        }
    }
}