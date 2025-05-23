﻿using Tataru.Types.AssembleTime;
using Tataru.Types.RunTime;
using Tataru.Memory;
using Tataru.Mapper;
using Tataru.Headers;
using Tataru.Engine.Types;

using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Linq;
using System.Text.RegularExpressions;

static partial class Program {
    private static int Main(string[] args) {
        (ExitConditions ReturnCode, string? Message) = CommandLineDecode(ref args);
        if (Message != null) Console.Write(Message);
        if (ReturnCode != ExitConditions.CONTINUE) return (int)ReturnCode;

        // null = linear codeflow, non-null = macro execution
        (ExitConditions ResponseStatus, object? _) = Assemble(null, null)!;
        return       (int)ResponseStatus;
    }

    // use Roselyn API to validate terms
    private static async Task<(ExitConditions, T?)> Evaluate<T>(string Expression) {
        try {
            T? Result = await CSharpScript.EvaluateAsync<T>(Expression);
            return (ExitConditions.OK, Result);
        } catch (Exception e) {
            Console.WriteLine($"Could not evalute {Expression} : {e}");
            return (ExitConditions.ERROR, (T?)(object?)null);    
        }
    }

    // rebuild expression using contents from Label DataBase
    private static (bool, string?) ResolveVariables(string[] Line) {
        bool Ready = true;
        string ReconstructedExpression = "";

        for (int i = 0; i < Line.Length; i++) {
            if        (Line[i] == "\\") {
                (AssembleTimeValue? Response, i) = Descope(ref Line, i);
                if (i == -1) {
                    // error in Response 
                } else ReconstructedExpression += Response;
            } else if (Line[i].Length == 1 && Operators.Contains(Line[i][0])) {
                ReconstructedExpression += Line[i];
                continue;
            } else if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue(Line[i], out AssembleTimeValue Context)) {
                if (Context.Value == null) return (false, "");
                ReconstructedExpression += Context;
            } else return (false, null);
        }

        return (Ready, ReconstructedExpression);
    }

    private static void ProcessInstruction(string Mnemonic) {
        switch (Mnemonic) {
            case "adc": // Add with Carry
            case "and": // Logical And | BitMask
            case "asl": // Arithmetic Shift Left | Left Bitshift

            case "bcc": // Branch Carry Clear (traditional)
            case "blt": // Branch Less than (Uncommon)
                        // Branch C clear

            case "bcs": // Branch Carry Set (Tradtional)
            case "bgt": // Branch Greater than (Uncommon)
                        // Branch C Set

            case "bne": // Branch on Not Equal (Traditional)
            case "bzc": // Branch on Zero Clear (Tataru/internal)
                        // Branch Z clear

            case "beq": // Branch on Equal (Traditional)
            case "bzs": // Branch on Zero Set (Tataru/internal)
                        // Branch Z Set

            case "bvc": // Branch on Overflow Clear 
                        // Branch V clear

            case "bvs": // Branch on Overflow Set
                        // Branch V Set

            case "bpl": // Branch on Plus (Traditional)
            case "bnc": // Branch Negative Clear (Tataru/internal)
                        // Branch N clear

            case "bmi": // Branch on Minus (Traditional)
            case "bns": // Branch Negative Set (Tataru/internal)
                        // Branch N set

            case "bit": // Bit Check
            case "brk": // Break

            case "clc": // Clear Carry
            case "cld": // Clear Decimal (nop)
            case "clv": // Clear Overflow

            case "cmp": // Compare (A)
            case "cpx": // Compare X
            case "cpy": // Compare Y

            case "dec": // Decrement
            case "dex": // Decrement X
            case "dey": // Decrement Y

            case "eor": // Exclusive Or (Traditional)
            case "xor": // Exclusive Or (Alternative)

            case "inc": // Increment
            case "inx": // Increment X
            case "iny": // Increment Y

            case "jmp":  // Jump (Traditional)
            case "jump": // Jump (Alternative)

            case "jsr":  // Jump to Subroutine (Traditional)
            case "call": // Call Procedure (Alternative)

            case "lda": // Load with Accumolator
            case "ldx": // Load with X
            case "ldy": // Load with Y

            case "lsr": // Logical Shift Right

            case "nop": // No Operation

            case "ora": // Logical Or 

            case "pha": // Push Accumulator to Stack
            case "php": // Push Processor Status to Stack
            case "pla": // Pull from Stack into Accumulator
            case "plp": // Pull from Stack into Processor Status

            case "rol": // Roll Left
            case "ror": // Roll Right

            case "ret": // Return (Alternative)
            case "rts": // Return from Subroutine (Traditional)

            case "rti": // Return from Interrupt

            case "sbc": // Subtract with Carry

            case "sec": // Set Carry
            case "sed": // Set Decimal
            case "sei": // Set Interrupt

            case "sta": // Store with Accumolator
            case "stx": // Store with X
            case "sty": // Store with Y

            case "tax": // Transfer A to X
            case "tay": // Transfer A to Y
            case "tsx": // Transfer SP to X
            case "txa": // Transfer X to A
            case "txs": // Transfer X to SP
            case "tya": // Transfer Y to A

            // Illegal/Unofficial Instruction Mnemonics : (Taken from NoMoreSecrets with adjustments from ca65)

            case "aso":
            case "slo": // ca65/asm6f

            case "rla": // ca65/asm6f
            case "rln":

            case "lse":
            case "sre": // ca65/asm6f

            case "rra": // ca65/asm6f
            case "rrd":

            case "aax":
            case "sax": // ca65/asm6f

            case "lax": // ca65/asm6f

            case "dcm":
            case "dcp": // ca65/asm6f

            case "isc": // ca65/asm6f
            case "usb":

            case "ana":
            case "anb":
            case "anc": // ca65/asm6f

            case "alr": // ca65/asm6f
            case "asr":

            case "arr": // ca65/asm6f
            case "sbx":
            case "xma":

            case "axs": // ca65/asm6f

            case "ahx":
            case "axa":
            case "sha": // ca65/asm6f
            case "tea":

            case "shx": // ca65/asm6f
            case "sxa":
            case "tex":
            case "xas":

            case "say":
            case "shy": // ca65/asm6f
            case "sya":
            case "tey":

            case "shs":
            case "tas": // ca65/asm6f

            case "lar":
            case "las": // ca65/asm6f

            case "ane":
            case "axm":
            case "xaa": // ca65/asm6f

            case "hlt":
            case "kil":
            case "jam":
            case "stp": // ca65/asm6f

            default: break;
        }
    }

    /// <summary>
    /// Validates all parenthesis are closed for each opened in the correct order.
    /// </summary>
    /// <param name="Context">Array of encapsulating 'parenthesis' operators.</param>
    /// <returns></returns>
    private static bool IsContextComplete(char[] Context) {
        char[] ResolveBuffer = new char[(Context.Length + 1) >> 1];
        int    ResolveBufferIndex = 0;

        for (int element_i = 0; element_i < Context.Length; element_i++) { 
            switch (Context[element_i]) {
                case '{':
                case '[':
                case '(':
                    ResolveBuffer[ResolveBufferIndex] = Context[element_i];
                    ResolveBufferIndex++;
                    continue;

                case '}':
                case ']':

                    if (ResolveBuffer[ResolveBufferIndex] != Context[element_i] - 2) return false;
                    ResolveBufferIndex--;
                    continue;
                
                case ')':
                    if (ResolveBuffer[ResolveBufferIndex] != '(') return false;
                    ResolveBufferIndex--;
                    continue;

                case '\"':
                    if (ResolveBuffer[ResolveBufferIndex] == '\"') ResolveBufferIndex--;
                    else {
                        ResolveBuffer[ResolveBufferIndex] = Context[element_i];
                        ResolveBufferIndex++;
                    }
                    continue;
            }
        }

        return ResolveBufferIndex == 0;
    }

    // returns (decoded, line pos)
    private static (AssembleTimeValue?, int) Descope(ref string[] Line, int Index) {
        Dictionary<string, AssembleTimeValue> ThisScope;
        int Key;

        if (Line[Index] == "\\") {
            Index++;
            Key = 2;
        } else Key = 1;

        ThisScope = (Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^Key]].Value!;
        // check is label is present

        if (!ThisScope.TryGetValue(Line[Index], out AssembleTimeValue Context)) {
            Console.WriteLine((Key == 2 ? "Parent" : "Current") + $"scope contains no definition for : {Line[Index]}");
            return (null, -1);      // error
        } while (Context.Type == AssembleTimeTypes.SCOPE && Line[Index] + 1 == "\\") {
            if (!((Dictionary<string, AssembleTimeValue>)Context.Value!).TryGetValue(Line[Index += 2], out Context)) {
                Console.WriteLine($"Scope {Line[Index]} contains no definition for : {Line[Index]}");
                return (null, -1);
            }
        }
        return (Context, Index);
    }

    private static (ExitConditions, object?) Assemble(string[]? TargetSource, int? TargetIndex) {
        bool LinearExecution = false;

        string[] LineElements = [];
        int      LineElementIndex = 0;
        object?  ReturnValue = null;                                        // void macros/linear codeflow should be null

        if (TargetSource == null) {
            LinearExecution = true;                                         // if linear execution, ensure on macro call we update
            TargetSource = CodeBlocks[^1];
            TargetIndex  = CodeBlockIndexes[^1];
        }

        for (; TargetIndex < TargetSource.Length; TargetIndex++) {
            if (TargetSource[(int)TargetIndex].StartsWith('#')) {
                // directives cannot have code following, so no ; at the end

                SplitCodeLine(ref LineElements);
                LineElementIndex = 1;                                       // start beyond the '#'


                switch (LineElements[LineElementIndex]) {
                    case "include":
                        TargetIndex++;
                        switch (LineElements[LineElementIndex]) {
                            case "\"":
                                LineElementIndex++;
                                string IncPath = LineElements[LineElementIndex];
                                if (LineElements[++LineElementIndex] != "\"") {
                                    Console.WriteLine("Local Include was not completed.");
                                    return (ExitConditions.ERROR, null);
                                }

                                try {
                                    CodeBlocks.Add(File.ReadAllLines(IncPath));
                                    CodeBlockIndexes.Add(0);
                                } catch (Exception e) {
                                    Console.WriteLine($"Could not access {IncPath} : {e}");
                                    return (ExitConditions.ERROR, null);
                                }

                                goto ExecuteNewCodeBlock;

                            case "<":
                                LineElementIndex++;
                                string ModuleName = LineElements[LineElementIndex];
                                if (LineElements[++LineElementIndex] != ">") {
                                    Console.WriteLine("Library Include was not completed.");
                                    return (ExitConditions.ERROR, null);
                                }

                                // attempt to find libpath_i/module_name.s (where libpath_i is iteration over subscribed paths to libpaths)

                                foreach (string libpath in LibPaths) {
                                    try {
                                        CodeBlocks.Add(File.ReadAllLines($"{libpath}/{LineElements[LineElementIndex]}.s"));
                                        CodeBlockIndexes.Add(0);
                                    } catch (Exception e) {
                                        if (libpath == LibPaths[^1]) {
                                            Console.WriteLine($"Could not Locate Library '{ModuleName}', is its parent subscribed to the libpath?");
                                            return (ExitConditions.ERROR, null);
                                        }
                                    }
                                }

                            // leak into ExecuteNewCodeBlock

                            ExecuteNewCodeBlock:
                                CodeBlockIndexes[^1] = 1 + (int)TargetIndex;    // on return, we should not re-enter to execute this line again
                                (ExitConditions ResponseStatus, object? ResponseContext) = Assemble(null, null);

                                // throw the error back, until we exit out
                                if (ResponseStatus == ExitConditions.ERROR) return (ResponseStatus, ResponseContext); 
                                break;

                            default:
                                Console.WriteLine($"Erroneous Symbol '{LineElements[LineElementIndex]}'");
                                return (ExitConditions.ERROR, null);
                        }
                        break;

                    case "define":
                        // symbolic define
                        break;

                    case "undefine":
                        // remove define
                        break;

                    case "assert":
                        // complete an assertation
                        break;

                    case "rom":
                        // pad until we reach desired rom point
                        break;

                    case "cpu":
                        // retarget cpu position
                        break;

                    default:
                        Console.WriteLine($"Unrecognied Directive #{TargetSource[(int)TargetIndex]}");
                        return (ExitConditions.ERROR, null);
                }
                TargetIndex++;
            } else {
                string[] Steps = TargetSource[(int)TargetIndex].Split(';');
                for (int i_step = 0; i_step < Steps.Length - 1; i_step++) {
                    // until no longer possible, solve all defines
                    while (Defines.Keys.Any(k => Regex.IsMatch(Steps[i_step], $@"\b{Regex.Escape(k)}\b"))) {
                        foreach (KeyValuePair<string, string> Define in Defines) {
                            Steps[i_step] = Regex.Replace(Steps[i_step], $@"\b{Regex.Escape(Define.Key)}\b", Define.Value);
                        }

                    }

                    // identify function (keyword, macro call, implicit instruction, explicit instruction)

                    // keywords

                    switch (Steps[i_step]) {
                        case "return":
                            if (i_step == Steps.Length - 1)       return (ExitConditions.OK, null);   // VOID => NULL
                            else if (Steps[i_step + 1] == "null") return (ExitConditions.OK, null);
                            // else attempt to solve, if impossible return ERROR, NULL
                            // otherwise return OK with return value
                            break;

                        case "proc":
                            break;

                        case "if":
                            // wait for condition ()
                            // if true enter and log we entered an if statement
                            // otherwise skip over everything until we hit the if statement close
                            // the skip will require reading things still, logging brace entry and closing
                            break;

                        case "else":
                            // check for else if (), if case is else if then goto case "if"
                            // otherwise act like if but contain no conditional
                            break;

                        case "loop":
                            // indefinitely loop, when we hit the end re-enter loop at top
                            break;

                        case "break":
                            // break from loop, scan for loop end
                            // OR do not leak into next case in switch-case
                            break;

                        case "switch":
                            // switch (num, string, exp) => case 
                            break;

                        case "case":
                            // used in a switch case, target result code
                            break;

                        case "nmi":
                            break;

                        case "reset":
                            break;

                        case "irq":
                            break;

                        case "ref":
                            break;

                        case "void":
                            break;

                        case "int":
                            break;

                        case "string":
                            break;

                        case "exp":
                            break;

                        case "macro":
                            break;

                        case "bank":
                            break;

                        case "register":
                            break;

                        case "flag":
                            break;

                        case "scope":
                            break;

                        case "ux":
                        case "ix":
                        case "lx":
                        case "bx":
                        case "nx":
                        case "ulx":
                        case "ilx":
                        case "ubx":
                        case "ibx":
                        case "unx":
                        case "inx":
                        case "ulnx":
                        case "ilnx":
                        case "num":
                        case "x8":
                        case "x16":
                        case "x24":
                        case "x32":
                        case "x64":
                        case "l16":
                        case "l24":
                        case "l32":
                        case "l64":
                        case "ln16":
                        case "ln24":
                        case "ln32":
                        case "ln64":
                        case "b16":
                        case "b24":
                        case "b32":
                        case "b64":
                        case "bn16":
                        case "bn24":
                        case "bn32":
                        case "bn64":
                            // cannot start with these as they are filter keywords for macros
                            Console.WriteLine($"Cannot reserve type of {Steps[i_step]} as it is not a variable type, but variable type filter rule.");
                            return (ExitConditions.ERROR, null);
                        
                    }




                    // descope and solve labels
                    SplitCodeLine(ref LineElements);
                }

                do {
                    string AutoContextPayload = Steps[^1];
                    try {
                        // until no longer possible, solve all defines
                        // descope and solve labels
                        // attempt evaluate, if more context required fetch from next line and try again
                        // if complete, we can break out
                    } catch (Exception e) {
                        switch (e.HResult) {
                            default:
                                Console.WriteLine($"FATAL ERROR EVALUATING {AutoContextPayload}");
                                return (ExitConditions.ERROR, null);
                        }
                    }
                } while (true);
            }
            // line evaluation
        }

        return (ExitConditions.OK, ReturnValue);
    }

    // responses >3 chars long are an error message when conditon is ERROR otherwise they are variables, null means it was not an implicit => potential macro
    private static (ExitConditions, string?) MakeInstructionExplicit(string Mnemonic) {
        AssembleTimeValue Context;
        switch (Mnemonic.ToLower().Take(2)) {
            case "ld":
                if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[2]}", out Context)) {
                    switch (Context.Type) {
                        case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[2]}");
                        case AssembleTimeTypes.REGISTER:
                            switch ((byte)Context.Value!) {
                                case 0x00: return (ExitConditions.OK, "lda");
                                case 0x01: return (ExitConditions.OK, "ldx");
                                case 0x02: return (ExitConditions.OK, "ldy");
                                default:
                                    return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                            }
                        default:
                            return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[2]} as it is neither an assemble time integer or a register");
                    }
                } else return (ExitConditions.CONTINUE, null);
            case "st":
                if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[2]}", out  Context)) {
                    switch (Context.Type) {
                        case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[2]}");
                        case AssembleTimeTypes.REGISTER:
                            switch ((byte)Context.Value!) {
                                case 0x00: return (ExitConditions.OK, "sta");
                                case 0x01: return (ExitConditions.OK, "stx");
                                case 0x02: return (ExitConditions.OK, "sty");
                                default:
                                    return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                            }
                        default:
                            return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[2]} as it is neither an assemble time integer or a register");
                    }
                } else return (ExitConditions.CONTINUE, null);
            case "ta":
                if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[2]}", out Context)) {
                    switch (Context.Type) {
                        case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[2]}");
                        case AssembleTimeTypes.REGISTER:
                            switch ((byte)Context.Value!) {
                                case 0x00: return (ExitConditions.ERROR, $"Source and Target are identical a, {Mnemonic[2]}");
                                case 0x01: return (ExitConditions.OK, "tax");
                                case 0x02: return (ExitConditions.OK, "tay");
                                default:
                                    return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                            }
                        default:
                            return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[2]} as it is neither an assemble time integer or a register");
                    }
                } else return (ExitConditions.CONTINUE, null);
            case "ty":
                if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[2]}", out Context)) {
                    switch (Context.Type) {
                        case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[2]}");
                        case AssembleTimeTypes.REGISTER:
                            switch ((byte)Context.Value!) {
                                case 0x00: return (ExitConditions.OK, "tya");
                                case 0x01: return (ExitConditions.OK, "tyx");
                                case 0x02: return (ExitConditions.ERROR, $"Source and Target are identical y, {Mnemonic[2]}");
                                default:
                                    return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                            }
                        default:
                            return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[2]} as it is neither an assemble time integer or a register");
                    }
                } else return (ExitConditions.CONTINUE, null);
            case "tx":
                if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[2]}", out Context)) {
                    switch (Context.Type) {
                        case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[2]}");
                        case AssembleTimeTypes.REGISTER:
                            switch ((byte)Context.Value!) {
                                case 0x00: return (ExitConditions.OK, "txa");
                                case 0x01: return (ExitConditions.ERROR, $"Source and Target are identical x, {Mnemonic[2]}");
                                case 0x02: return (ExitConditions.OK, "txy");
                                default:
                                    return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                            }
                        default:
                            return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[2]} as it is neither an assemble time integer or a register");
                    }
                } else return (ExitConditions.CONTINUE, null);
            case "in":
                if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[2]}", out Context)) {
                    switch (Context.Type) {
                        case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[2]}");
                        case AssembleTimeTypes.REGISTER:
                            switch ((byte)Context.Value!) {
                                case 0x00: return (ExitConditions.ERROR, "No instruction exists as ina, implicit mnemonics cannot introduce new opcodes, consider using x/y");
                                case 0x01: return (ExitConditions.OK, "inx");
                                case 0x02: return (ExitConditions.OK, "iny");
                                default:
                                    return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                            }
                        default:
                            return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[2]} as it is neither an assemble time integer or a register");
                    }
                } else return (ExitConditions.CONTINUE, null);
            case "de":
                if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[2]}", out Context)) {
                    switch (Context.Type) {
                        case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[2]}");
                        case AssembleTimeTypes.REGISTER:
                            switch ((byte)Context.Value!) {
                                case 0x00: return (ExitConditions.ERROR, "No instruction exists as dea, implicit mnemonics cannot introduce new opcodes, consider using x/y");
                                case 0x01: return (ExitConditions.OK, "dex");
                                case 0x02: return (ExitConditions.OK, "dey");
                                default:
                                    return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                            }
                        default:
                            return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[2]} as it is neither an assemble time integer or a register");
                    }
                } else return (ExitConditions.CONTINUE, null);
            default:
                switch (Mnemonic[0], Mnemonic[1]) {
                    case ('t', 'a'):    // tra
                        if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[1]}", out Context)) {
                            switch (Context.Type) {
                                case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[1]}");
                                case AssembleTimeTypes.REGISTER:
                                    switch ((byte)Context.Value!) {
                                        case 0x00: return (ExitConditions.ERROR, $"Source and Target are identical {Mnemonic[1]}, a");
                                        case 0x01: return (ExitConditions.OK, "txa");
                                        case 0x02: return (ExitConditions.OK, "tya");
                                        default:
                                            return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                                    }
                                default:
                                    return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[1]} as it is neither an assemble time integer or a register");
                            }
                        } else return (ExitConditions.CONTINUE, null);
                    case ('t', 'x'):    // trx
                        if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[1]}", out Context)) {
                            switch (Context.Type) {
                                case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[1]}");
                                case AssembleTimeTypes.REGISTER:
                                    switch ((byte)Context.Value!) {
                                        case 0x00: return (ExitConditions.OK, "tax");
                                        case 0x01: return (ExitConditions.ERROR, $"Source and Target are identical {Mnemonic[1]}, x");
                                        case 0x02: return (ExitConditions.OK, "tyx");
                                        default:
                                            return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                                    }
                                default:
                                    return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[1]} as it is neither an assemble time integer or a register");
                            } 
                        } else return (ExitConditions.CONTINUE, null);
                    case ('t', 'y'):    // try
                        if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[1]}", out Context)) {
                            switch (Context.Type) {
                                case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[1]}");
                                case AssembleTimeTypes.REGISTER:
                                    switch ((byte)Context.Value!) {
                                        case 0x00: return (ExitConditions.OK, "tay");
                                        case 0x01: return (ExitConditions.OK, "txy");
                                        case 0x02: return (ExitConditions.ERROR, $"Source and Target are identical {Mnemonic[1]}, y");
                                        default:
                                            return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                                    }
                                default:
                                    return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[1]} as it is neither an assemble time integer or a register");
                            }
                        } else return (ExitConditions.CONTINUE, null);

                    case ('b', 's'):    // brs
                        if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[1]}", out Context)) {
                            switch (Context.Type) {
                                case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[1]}");
                                case AssembleTimeTypes.FLAG:
                                    switch ((byte)Context.Value!) {
                                        case 0x00: return (ExitConditions.OK, "bcs");
                                        case 0x01: return (ExitConditions.OK, "bzs");
                                        case 0x02: return (ExitConditions.OK, "bvs");
                                        case 0x03: return (ExitConditions.OK, "bns");
                                        default:
                                            return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                                    }
                                default:
                                    return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[1]} as it is neither an assemble time integer or a flag");
                            }
                        } else return (ExitConditions.CONTINUE, null);
                    case ('b', 'c'):    // brc
                        if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[1]}", out Context)) {
                            switch (Context.Type) {
                                case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[1]}");
                                case AssembleTimeTypes.FLAG:
                                    switch ((byte)Context.Value!) {
                                        case 0x00: return (ExitConditions.OK, "bcc");
                                        case 0x01: return (ExitConditions.OK, "bzc");
                                        case 0x02: return (ExitConditions.OK, "bvc");
                                        case 0x03: return (ExitConditions.OK, "bnc");
                                        default:
                                            return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                                    }
                                default:
                                    return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[1]} as it is neither an assemble time integer or a flag");
                            }
                        } else return (ExitConditions.CONTINUE, null);
                    case ('j', 's'):    // jrs
                        if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[1]}", out Context)) {
                            switch (Context.Type) {
                                case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[1]}");
                                case AssembleTimeTypes.FLAG:
                                    switch ((byte)Context.Value!) {
                                        case 0x00: return (ExitConditions.OK, "jcs");
                                        case 0x01: return (ExitConditions.OK, "jzs");
                                        case 0x02: return (ExitConditions.OK, "jvs");
                                        case 0x03: return (ExitConditions.OK, "jns");
                                        default:
                                            return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                                    }
                                default:
                                    return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[1]} as it is neither an assemble time integer or a flag");
                            }
                        } else return (ExitConditions.CONTINUE, null);
                    case ('j', 'c'):    // jrc
                        if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[1]}", out Context)) {
                            switch (Context.Type) {
                                case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[1]}");
                                case AssembleTimeTypes.FLAG:
                                    switch ((byte)Context.Value!) {
                                        case 0x00: return (ExitConditions.OK, "jcc");
                                        case 0x01: return (ExitConditions.OK, "jzc");
                                        case 0x02: return (ExitConditions.OK, "jvc");
                                        case 0x03: return (ExitConditions.OK, "jnc");
                                        default:
                                            return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                                    }
                                default:
                                    return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[1]} as it is neither an assemble time integer or a flag");
                            }
                        } else return (ExitConditions.CONTINUE, null);
                    case ('c', 's'):    // crs
                        if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[1]}", out Context)) {
                            switch (Context.Type) {
                                case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[1]}");
                                case AssembleTimeTypes.FLAG:
                                    switch ((byte)Context.Value!) {
                                        case 0x00: return (ExitConditions.OK, "ccs");
                                        case 0x01: return (ExitConditions.OK, "czs");
                                        case 0x02: return (ExitConditions.OK, "cvs");
                                        case 0x03: return (ExitConditions.OK, "cns");
                                        default:
                                            return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                                    }
                                default:
                                    return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[1]} as it is neither an assemble time integer or a flag");
                            }
                        } else return (ExitConditions.CONTINUE, null);
                    case ('c', 'c'):    // crc
                        if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[1]}", out Context)) {
                            switch (Context.Type) {
                                case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[1]}");
                                case AssembleTimeTypes.FLAG:
                                    switch ((byte)Context.Value!) {
                                        case 0x00: return (ExitConditions.OK, "ccc");
                                        case 0x01: return (ExitConditions.OK, "czc");
                                        case 0x02: return (ExitConditions.OK, "cvc");
                                        case 0x03: return (ExitConditions.OK, "cnc");
                                        default:
                                            return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                                    }
                                default:
                                    return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[1]} as it is neither an assemble time integer or a flag");
                            }
                        } else return (ExitConditions.CONTINUE, null);
                    case ('r', 's'):    // rrs
                        if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[1]}", out Context)) {
                            switch (Context.Type) {
                                case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[1]}");
                                case AssembleTimeTypes.FLAG:
                                    switch ((byte)Context.Value!) {
                                        case 0x00: return (ExitConditions.OK, "rcs");
                                        case 0x01: return (ExitConditions.OK, "rzs");
                                        case 0x02: return (ExitConditions.OK, "rvs");
                                        case 0x03: return (ExitConditions.OK, "rns");
                                        default:
                                            return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                                    }
                                default:
                                    return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[1]} as it is neither an assemble time integer or a flag");
                            }
                        } else return (ExitConditions.CONTINUE, null);
                    case ('r', 'c'):    // rrc
                        if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[1]}", out Context)) {
                            switch (Context.Type) {
                                case AssembleTimeTypes.INT: return (ExitConditions.OK, $"{Mnemonic[1]}");
                                case AssembleTimeTypes.FLAG:
                                    switch ((byte)Context.Value!) {
                                        case 0x00: return (ExitConditions.OK, "rcc");
                                        case 0x01: return (ExitConditions.OK, "rzc");
                                        case 0x02: return (ExitConditions.OK, "rvc");
                                        case 0x03: return (ExitConditions.OK, "rnc");
                                        default:
                                            return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                                    }
                                default:
                                    return (ExitConditions.ERROR, $"Implicit Mnemonic cannot use component {Mnemonic[1]} as it is neither an assemble time integer or a flag");
                            }
                        } else return (ExitConditions.CONTINUE, null);
                    default:
                        if (Mnemonic[0] == 't') {   // tri
                            // dual implicit
                            if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[1]}", out Context)) {
                                byte Transfer = (byte)Context.Value!;
                                if (Transfer > 3) return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");

                                Transfer <<= 2;
                                if (((Dictionary<string, AssembleTimeValue>)LabelDB[ScopeSequence[^1]].Value!).TryGetValue($"{Mnemonic[2]}", out Context)) {
                                    Transfer |= (byte)Context.Value!;
                                    if ((byte)Context.Value > 3) return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");

                                    switch (Transfer) {
                                        case 0b00_00: case 0b01_01: case 0b10_10: return (ExitConditions.CONTINUE, null);

                                        case 0b00_01: return (ExitConditions.OK, "tax");
                                        case 0b00_10: return (ExitConditions.OK, "tay");
                                        case 0b01_00: return (ExitConditions.OK, "txa");
                                        case 0b01_10: return (ExitConditions.OK, "txy");
                                        case 0b10_00: return (ExitConditions.OK, "tya");
                                        case 0b10_01: return (ExitConditions.OK, "tyx");

                                        default:
                                            return (ExitConditions.ERROR, $"FATAL ERROR : ENCOUNTERED REGISTER VALUE {Context.Value} PLEASE REPORT THIS ON THE GITHUB");
                                    }

                                } else return (ExitConditions.CONTINUE, null);
                            } else return (ExitConditions.CONTINUE, null);
                        } else return (ExitConditions.CONTINUE, null);
                }
            }
    }

    private static void SplitCodeLine(ref string[] Code) {
        // CodeLine here refers to the code between semicolons, this should be a regex operation
        return;
    }

    private static (ExitConditions, string?) CommandLineDecode(ref string[] args) {
        for (int i = 0; i < args.Length; i++) {
            switch (args[i]) {
                case "-h":
                case "--help":
                    Console.Write("""
Tataru v0.0.1 - Brette (2025) GPL V2

-h | --help     | Display a 'help' message, listing argument instructions.
-d | --debug    | Produce a debug file for use with a Debugger
-l | --listing  | Produce a permanent variable sheet
-i | --input    | Specify Assembly Source File Entry Point
-o | --output   | Specfiy the produced ROM/Disk Output.
""");
                    return (ExitConditions.OK, default);

                case "-d":
                case "--debug":
                    Debug = true;
                    break;

                case "-l":
                case "--listing":
                    Listing = true;
                    break;
                    
                case "-i":
                case "--input":
                    try {
                        if (CodeBlocks.Count > 0) return (ExitConditions.ERROR, "Cannot specify more than one debug file");
                        CodeBlocks = [File.ReadAllLines(args[++i])];
                        break;
                    } catch (Exception e) {
                        return (ExitConditions.ERROR, $"Failed to access asssembly source file {args[i]} : {e}");
                    }

                case "-o":
                case "--output":
                    try {
                        OutputPath = args[++i];
                        if (File.Exists(OutputPath)) return (ExitConditions.ERROR, $"File at {args[i]} already exists!");
                        File.Create(OutputPath);
                        break;
                    } catch (Exception e) {
                        return (ExitConditions.ERROR, $"Failed to create file at {OutputPath} : {e}");
                    }

                default:
                    return (ExitConditions.ERROR, $"Unknown request {args[i]} during Argument Parsing");
            }
        }
        
        if (CodeBlocks.Count == 1) return (ExitConditions.CONTINUE, default);
                                   return (ExitConditions.ERROR,    "Unknown Error during Argument Parsing");
    }

    static List<string[]>   CodeBlocks       = [];
    static List<int>        CodeBlockIndexes = [0];

    static List<byte>       Output           = [];
    static string           OutputPath       = "";

    static Memory SimulatedMemory = new(Mappers.NROM, new());

    static List<string>     LibPaths         = [];

    static Dictionary<string, AssembleTimeValue> LabelDB = new();
    static string[]         ScopeSequence    = [];

    static Dictionary<string, string> Defines= new();

    static bool Debug, Listing, Verbose = false;

    const string            Operators        = "+-/*()[]{}|^&;#~$%£?:=<>!\"',.\\";
    const string            Registers        = "axy";
    const string            Flags            = "nczv";
    const string            Reserved         = "axynczv";
}