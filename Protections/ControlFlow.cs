using de4dot.blocks;
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;

namespace ConfuserEx_Dynamic_Unpacker.Protections
{
    class ControlFlow : BlockDeobfuscator
    {
        private Block switchBlock;
        private Local localSwitch;

        protected override bool Deobfuscate(Block block)
        {
            bool modified = false;
            if (block.LastInstr.OpCode == OpCodes.Switch)
            {
                allVars = blocks.Method.Body.Variables;
                isSwitchBlock(block);
                if (switchBlock != null && localSwitch != null)
                {
                    ins.Initialize(blocks.Method);
                    modified |= Cleaner();
                }
                isExpressionBlock(block);
                if (switchBlock != null || localSwitch != null)
                {
                    ins.Initialize(blocks.Method); modified |= Cleaner();
                    while (Cleaner())
                    {
                        modified |= Cleaner();
                    }
                }
            }
            return modified;
        }
        public InstructionEmulator ins = new InstructionEmulator();
        bool Cleaner()
        {
            bool modified = false;
            List<Block> allblocks = new List<Block>();
            foreach (var block in allBlocks)
            {
                if (block.FallThrough == switchBlock)
                {
                    allblocks.Add(block);
                }
            }
            List<Block> targetBlocks = new List<Block>();
            targetBlocks = switchBlock.Targets;
            foreach (Block block in allblocks)
            {
                if (block.LastInstr.IsLdcI4())
                {
                    int val1 = block.LastInstr.GetLdcI4Value();
                    ins.Push(new Int32Value(val1));
                    int nextCase = emulateCase(out int localValue);
                    block.ReplaceLastNonBranchWithBranch(0, targetBlocks[nextCase]);
                    replace(targetBlocks[nextCase], localValue);
                    block.Instructions.Add(new Instr(new Instruction(OpCodes.Pop)));
                    modified = true;
                }
                else if (isXor(block))
                {
                    ins.Emulate(block.Instructions, block.Instructions.Count - 5, block.Instructions.Count);
                    Int32Value val1 = (Int32Value)ins.Pop();
                    ins.Push(val1);
                    int nextCase = emulateCase(out int localValue);
                    block.ReplaceLastNonBranchWithBranch(0, targetBlocks[nextCase]);
                    replace(targetBlocks[nextCase], localValue);
                    block.Instructions.Add(new Instr(new Instruction(OpCodes.Pop)));
                    modified = true;
                }
                else if (block.Sources.Count == 2 && block.Instructions.Count == 1)
                {
                    var sources = new List<Block>(block.Sources);
                    foreach (Block source in sources)
                    {
                        if (source.FirstInstr.IsLdcI4())
                        {
                            int val1 = source.FirstInstr.GetLdcI4Value();
                            ins.Push(new Int32Value(val1));
                            int nextCase = emulateCase(out int localValue);
                            source.ReplaceLastNonBranchWithBranch(0, targetBlocks[nextCase]);
                            replace(targetBlocks[nextCase], localValue);
                            source.Instructions[1] = (new Instr(new Instruction(OpCodes.Pop)));
                            modified = true;
                        }
                    }
                }
                else if (block.LastInstr.OpCode == OpCodes.Xor)
                {
                    if (block.Instructions[block.Instructions.Count - 2].OpCode == OpCodes.Mul)
                    {
                        var instr = block.Instructions;
                        int l = instr.Count;
                        if (!(instr[l - 4].IsLdcI4())) continue;
                        var sources = new List<Block>(block.Sources);
                        foreach (Block source in sources)
                        {
                            if (source.FirstInstr.IsLdcI4())
                            {
                                int val1 = source.FirstInstr.GetLdcI4Value();
                                try
                                {
                                    instr[l - 5] = new Instr(new Instruction(OpCodes.Ldc_I4, val1));
                                }
                                catch
                                {
                                    instr.Insert(l - 4, new Instr(new Instruction(OpCodes.Ldc_I4, val1)));
                                    l++;
                                }
                                ins.Emulate(instr, l - 5, l);
                                int nextCase = emulateCase(out int localValue);
                                source.ReplaceLastNonBranchWithBranch(0, targetBlocks[nextCase]);
                                replace(targetBlocks[nextCase], localValue);
                                try
                                {
                                    source.Instructions[1] = (new Instr(new Instruction(OpCodes.Pop)));
                                }
                                catch
                                {
                                    source.Instructions.Add((new Instr(new Instruction(OpCodes.Pop))));
                                }
                                modified = true;
                            }
                        }
                    }
                }
            }
            return modified;
        }
        public bool replace(Block test, int locVal)
        {
            if (test.IsConditionalBranch())
            {
                if (test.FallThrough.FallThrough == switchBlock)
                {
                    test = test.FallThrough;
                }
                else
                {
                    test = test.FallThrough.FallThrough;
                }
            }
            if (test == switchBlock) return false;
            for (int i = 0; i < test.Instructions.Count; i++)
            {
                if (test.Instructions[i].Instruction.GetLocal(blocks.Method.Body.Variables) == localSwitch)
                {
                    test.Instructions[i] = new Instr(Instruction.CreateLdcI4(locVal));
                    return true;
                }
            }
            return false;
        }
        public int emulateCase(out int localValueasInt)
        {
            ins.Emulate(switchBlock.Instructions, 0, switchBlock.Instructions.Count - 1);
            var localValue = ins.GetLocal(localSwitch) as Int32Value;
            localValueasInt = localValue.Value;
            return ((Int32Value)ins.Pop()).Value;
        }
        bool isXor(Block block)
        {
            int l = block.Instructions.Count - 1;
            var instr = block.Instructions;
            if (l < 4) return false;
            if (instr[l].OpCode != OpCodes.Xor) return false;
            if (!instr[l - 1].IsLdcI4()) return false;
            if (instr[l - 2].OpCode != OpCodes.Mul) return false;
            if (!instr[l - 3].IsLdcI4()) return false;
            if (!instr[l - 4].IsLdcI4()) return false;
            return true;
        }
        void isExpressionBlock(Block block)
        {
            if (block.Instructions.Count < 7) return;
            if (!block.FirstInstr.IsStloc()) return;
            switchBlock = block;
            localSwitch = Instr.GetLocalVar(blocks.Method.Body.Variables.Locals, block.Instructions[block.Instructions.Count - 4]);
        }
        private IList<Local> allVars;
        void isSwitchBlock(Block block)
        {
            if (block.Instructions.Count <= 6) return;
            if (!block.FirstInstr.IsLdcI4()) return;
            switchBlock = block;
            localSwitch = Instr.GetLocalVar(allVars, block.Instructions[block.Instructions.Count - 4]);
        }
    }
}