using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Sulakore.Habbo.Web;

using Flazzy.ABC;
using Flazzy.ABC.AVM2;
using Flazzy.ABC.AVM2.Instructions;

namespace Sulakore.Habbo.Messages
{
    [DebuggerDisplay("{Id,nq}")]
    public class HMessage : IEquatable<HMessage>
    {
        public ushort Id { get; set; }
        public string Hash { get; set; }

        public bool IsOutgoing { get; set; }

        public string Name { get; set; }
        public string Structure { get; set; }

        public ASClass Class { get; set; }
        public string ClassName { get; }

        public ASClass Parser { get; set; }
        public string ParserName { get; }

        public List<HReference> References { get; }

        public static implicit operator ushort(HMessage message) => message?.Id ?? ushort.MaxValue;

        public HMessage()
            : this(ushort.MaxValue, false, null, null, null)
        { }
        public HMessage(ushort id, bool isOutgoing, ASClass messageClass)
        {
            Id = id;
            IsOutgoing = isOutgoing;
            References = new List<HReference>();

            Class = messageClass;
            ClassName = messageClass.QName.Name;

            if (!IsOutgoing)
            {
                Parser = GetMessageParser();
                if (Parser != null)
                {
                    ParserName = Parser.QName.Name;
                    Structure = GetIncomingStructure(Parser);
                }
            }
            else
            {
                Structure = GetOutgoingStructure(Class);
            }
        }
        public HMessage(ushort id, bool isOutgoing, string hash, string name, string structure)
        {
            Id = id;
            Hash = hash;
            Name = name;
            Structure = structure;
            IsOutgoing = isOutgoing;
        }

        public string GenerateHash()
        {
            if (!string.IsNullOrWhiteSpace(Hash))
            {
                return Hash;
            }
            using (var output = new HashWriter(false))
            {
                output.Write(IsOutgoing);
                if (!HGame.IsValidIdentifier(Class.QName.Name, true))
                {
                    output.Write(Class.Instance, true);
                    output.Write(Class.Instance.Constructor);

                    output.Write(References.Count);
                    foreach (HReference reference in References)
                    {
                        output.Write(reference.IsStatic);
                        output.Write(reference.IsAnonymous);

                        output.Write(reference.MethodRank);
                        output.Write(reference.InstructionRank);

                        output.Write(reference.FromMethod);

                        output.Write(reference.FromClass.Constructor);
                        output.Write(reference.FromClass.Instance.Constructor);
                    }
                    if (!IsOutgoing && Parser != null)
                    {
                        output.Write(Parser.Instance, true);
                    }
                }
                else output.Write(Class.QName.Name);
                return Hash = output.GenerateHash();
            }
        }

        public override string ToString() => Id.ToString();
        public bool Equals(HMessage other) => Id == other.Id;

        #region Structure Extraction
        private ASClass GetMessageParser()
        {
            ABCFile abc = Class.GetABC();
            ASInstance instance = Class.Instance;

            ASInstance superInstance = abc.GetInstance(instance.Super);
            if (superInstance == null) superInstance = instance;

            ASMethod parserGetterMethod = superInstance.GetGetter("parser")?.Method;
            if (parserGetterMethod == null) return null;

            IEnumerable<ASMethod> methods = instance.GetMethods();
            foreach (ASMethod method in methods.Concat(new[] { instance.Constructor }))
            {
                ASCode code = method.Body.ParseCode();
                foreach (ASInstruction instruction in code)
                {
                    ASMultiname multiname = null;
                    if (instruction.OP == OPCode.FindPropStrict)
                    {
                        var findPropStrictIns = (FindPropStrictIns)instruction;
                        multiname = findPropStrictIns.PropertyName;
                    }
                    else if (instruction.OP == OPCode.GetLex)
                    {
                        var getLexIns = (GetLexIns)instruction;
                        multiname = getLexIns.TypeName;
                    }
                    else continue;

                    foreach (ASClass refClass in abc.GetClasses(multiname))
                    {
                        ASInstance refInstance = refClass.Instance;
                        if (refInstance.ContainsInterface(parserGetterMethod.ReturnType.Name))
                        {
                            return refClass;
                        }
                    }
                }
            }
            return null;
        }

        private string GetIncomingStructure(ASClass @class)
        {
            ASMethod parseMethod = @class.Instance.GetMethod("parse", "Boolean", 1);
            return GetIncomingStructure(@class.Instance, parseMethod);
        }
        private string GetIncomingStructure(ASInstance instance, ASMethod method)
        {
            if (method.Body.Exceptions.Count > 0) return null;

            ASCode code = method.Body.ParseCode();
            if (code.JumpExits.Count > 0 || code.SwitchExits.Count > 0) return null;

            string structure = null;
            ABCFile abc = method.GetABC();
            for (int i = 0; i < code.Count; i++)
            {
                ASInstruction instruction = code[i];
                if (instruction.OP != OPCode.GetLocal_1) continue;

                ASInstruction next = code[++i];
                switch (next.OP)
                {
                    case OPCode.CallProperty:
                    {
                        var callProperty = (CallPropertyIns)next;
                        if (callProperty.ArgCount > 0)
                        {
                            ASMultiname propertyName = null;
                            ASInstruction previous = code[i - 2];

                            switch (previous.OP)
                            {
                                case OPCode.GetLex:
                                {
                                    var getLex = (GetLexIns)previous;
                                    propertyName = getLex.TypeName;
                                    break;
                                }

                                case OPCode.ConstructProp:
                                {
                                    var constructProp = (ConstructPropIns)previous;
                                    propertyName = constructProp.PropertyName;
                                    break;
                                }

                                case OPCode.GetLocal_0:
                                {
                                    propertyName = instance.QName;
                                    break;
                                }
                            }

                            ASInstance innerInstance = abc.GetInstance(propertyName);
                            ASMethod innerMethod = innerInstance.GetMethod(callProperty.PropertyName.Name, null, callProperty.ArgCount);
                            if (innerMethod == null)
                            {
                                ASClass innerClass = abc.GetClass(propertyName);
                                innerMethod = innerClass.GetMethod(callProperty.PropertyName.Name, null, callProperty.ArgCount);
                            }

                            string innerStructure = GetIncomingStructure(innerInstance, innerMethod);
                            if (string.IsNullOrWhiteSpace(innerStructure)) return null;
                            structure += innerStructure;
                        }
                        else
                        {
                            if (!TryGetStructurePiece(callProperty.PropertyName, null, out char piece)) return null;
                            structure += piece;
                        }
                        break;
                    }

                    case OPCode.ConstructProp:
                    {
                        var constructProp = (ConstructPropIns)next;
                        ASInstance innerInstance = abc.GetInstance(constructProp.PropertyName);

                        string innerStructure = GetIncomingStructure(innerInstance, innerInstance.Constructor);
                        if (string.IsNullOrWhiteSpace(innerStructure)) return null;
                        structure += innerStructure;
                        break;
                    }

                    case OPCode.ConstructSuper:
                    {
                        ASInstance superInstance = abc.GetInstance(instance.Super);

                        string innerStructure = GetIncomingStructure(superInstance, superInstance.Constructor);
                        if (string.IsNullOrWhiteSpace(innerStructure)) return null;
                        structure += innerStructure;
                        break;
                    }

                    case OPCode.CallSuper:
                    {
                        var callSuper = (CallSuperIns)next;
                        ASInstance superInstance = abc.GetInstance(instance.Super);
                        ASMethod superMethod = superInstance.GetMethod(callSuper.MethodName.Name, null, callSuper.ArgCount);

                        string innerStructure = GetIncomingStructure(superInstance, superMethod);
                        if (string.IsNullOrWhiteSpace(innerStructure)) return null;
                        structure += innerStructure;
                        break;
                    }

                    case OPCode.CallPropVoid:
                    {
                        var callPropVoid = (CallPropVoidIns)next;
                        if (callPropVoid.ArgCount != 0) return null;

                        if (!TryGetStructurePiece(callPropVoid.PropertyName, null, out char piece)) return null;
                        structure += piece;
                        break;
                    }

                    default: return null;
                }
            }
            return structure;
        }

        private string GetOutgoingStructure(ASClass @class)
        {
            ASMethod getArrayMethod = @class.Instance.GetMethod(null, "Array", 0);
            if (getArrayMethod == null)
            {
                ASClass superClass = @class.GetABC().GetClass(@class.Instance.Super);
                return GetOutgoingStructure(superClass);
            }
            if (getArrayMethod.Body.Exceptions.Count > 0) return null;
            ASCode getArrayCode = getArrayMethod.Body.ParseCode();

            if (getArrayCode.JumpExits.Count > 0 || getArrayCode.SwitchExits.Count > 0)
            {
                // Unable to parse data structure that relies on user input that is not present,
                // since the structure may change based on the provided input.
                return null;
            }

            ASInstruction resultPusher = null;
            for (int i = getArrayCode.Count - 1; i >= 0; i--)
            {
                ASInstruction instruction = getArrayCode[i];
                if (instruction.OP == OPCode.ReturnValue)
                {
                    resultPusher = getArrayCode[i - 1];
                    break;
                }
            }

            int argCount = -1;
            if (resultPusher.OP == OPCode.ConstructProp)
            {
                argCount = ((ConstructPropIns)resultPusher).ArgCount;
            }
            else if (resultPusher.OP == OPCode.NewArray)
            {
                argCount = ((NewArrayIns)resultPusher).ArgCount;
            }

            if (argCount > 0)
            {
                return GetOutgoingStructure(getArrayCode, resultPusher, argCount);
            }
            else if (argCount == 0 || resultPusher.OP == OPCode.PushNull)
            {
                return null;
            }

            if (resultPusher.OP == OPCode.GetProperty)
            {
                var getProperty = (GetPropertyIns)resultPusher;
                return GetOutgoingStructure(Class, getProperty.PropertyName);
            }
            else if (Local.IsGetLocal(resultPusher.OP))
            {
                return GetOutgoingStructure(getArrayCode, (Local)resultPusher);
            }
            return null;
        }
        private string GetOutgoingStructure(ASCode code, Local getLocal)
        {
            string structure = null;
            for (int i = 0; i < code.Count; i++)
            {
                ASInstruction instruction = code[i];
                if (instruction == getLocal) break;
                if (!Local.IsGetLocal(instruction.OP)) continue;

                var local = (Local)instruction;
                if (local.Register != getLocal.Register) continue;

                for (i += 1; i < code.Count; i++)
                {
                    ASInstruction next = code[i];
                    if (next.OP != OPCode.CallPropVoid) continue;

                    var callPropVoid = (CallPropVoidIns)next;
                    if (callPropVoid.PropertyName.Name != "push") continue;

                    ASInstruction previous = code[i - 1];
                    if (previous.OP == OPCode.GetProperty)
                    {
                        ASClass classToCheck = Class;
                        var getProperty = (GetPropertyIns)previous;
                        ASMultiname propertyName = getProperty.PropertyName;

                        ASInstruction beforeGetProp = code[i - 2];
                        if (beforeGetProp.OP == OPCode.GetLex)
                        {
                            var getLex = (GetLexIns)beforeGetProp;
                            classToCheck = classToCheck.GetABC().GetClass(getLex.TypeName);
                        }

                        if (!TryGetStructurePiece(propertyName, classToCheck, out char piece)) return null;
                        structure += piece;
                    }
                }
            }
            return structure;
        }
        private string GetOutgoingStructure(ASClass @class, ASMultiname propertyName)
        {
            ASMethod constructor = @class.Instance.Constructor;
            if (constructor.Body.Exceptions.Count > 0) return null;

            ASCode code = constructor.Body.ParseCode();
            if (code.JumpExits.Count > 0 || code.SwitchExits.Count > 0) return null;

            string structure = null;
            for (int i = 0; i < code.Count; i++)
            {
                ASInstruction instruction = code[i];
                if (instruction.OP == OPCode.NewArray)
                {
                    var newArray = (NewArrayIns)instruction;
                    if (newArray.ArgCount > 0)
                    {
                        var structurePieces = new char[newArray.ArgCount];
                        for (int j = i - 1, length = newArray.ArgCount; j >= 0; j--)
                        {
                            ASInstruction previous = code[j];
                            if (Local.IsGetLocal(previous.OP) && previous.OP != OPCode.GetLocal_0)
                            {
                                var local = (Local)previous;
                                ASParameter parameter = constructor.Parameters[local.Register - 1];

                                if (!TryGetStructurePiece(parameter.Type, null, out char piece)) return null;
                                structurePieces[--length] = piece;
                            }
                            if (length == 0)
                            {
                                structure += new string(structurePieces);
                                break;
                            }
                        }
                    }
                }
                else if (instruction.OP == OPCode.ConstructSuper)
                {
                    var constructSuper = (ConstructSuperIns)instruction;
                    if (constructSuper.ArgCount > 0)
                    {
                        ASClass superClass = @class.GetABC().GetClass(@class.Instance.Super);
                        structure += GetOutgoingStructure(superClass, propertyName);
                    }
                }
                if (instruction.OP != OPCode.GetProperty) continue;

                var getProperty = (GetPropertyIns)instruction;
                if (getProperty.PropertyName != propertyName) continue;

                ASInstruction next = code[++i];
                ASClass classToCheck = @class;
                if (Local.IsGetLocal(next.OP))
                {
                    if (next.OP == OPCode.GetLocal_0) continue;

                    var local = (Local)next;
                    ASParameter parameter = constructor.Parameters[local.Register - 1];

                    if (!TryGetStructurePiece(parameter.Type, null, out char piece)) return null;
                    structure += piece;
                }
                else
                {
                    if (next.OP == OPCode.FindPropStrict)
                    {
                        classToCheck = null;
                    }
                    else if (next.OP == OPCode.GetLex)
                    {
                        var getLex = (GetLexIns)next;
                        classToCheck = classToCheck.GetABC().GetClass(getLex.TypeName);
                    }
                    do
                    {
                        next = code[++i];
                        propertyName = null;
                        if (next.OP == OPCode.GetProperty)
                        {
                            getProperty = (GetPropertyIns)next;
                            propertyName = getProperty.PropertyName;
                        }
                        else if (next.OP == OPCode.CallProperty)
                        {
                            var callProperty = (CallPropertyIns)next;
                            propertyName = callProperty.PropertyName;
                        }
                    }
                    while (next.OP != OPCode.GetProperty && next.OP != OPCode.CallProperty);

                    if (!TryGetStructurePiece(propertyName, classToCheck, out char piece)) return null;
                    structure += piece;
                }
            }
            return structure;
        }
        private string GetOutgoingStructure(ASCode code, ASInstruction beforeReturn, int length)
        {
            var getLocalEndIndex = -1;
            int pushingEndIndex = code.IndexOf(beforeReturn);

            var structure = new char[length];
            var pushedLocals = new Dictionary<int, int>();
            for (int i = pushingEndIndex - 1; i >= 0; i--)
            {
                ASInstruction instruction = code[i];
                if (instruction.OP == OPCode.GetProperty)
                {
                    ASClass classToCheck = Class;
                    var getProperty = (GetPropertyIns)instruction;
                    ASMultiname propertyName = getProperty.PropertyName;

                    ASInstruction previous = code[i - 1];
                    if (previous.OP == OPCode.GetLex)
                    {
                        var getLex = (GetLexIns)previous;
                        classToCheck = classToCheck.GetABC().GetClass(getLex.TypeName);
                    }

                    if (!TryGetStructurePiece(propertyName, classToCheck, out char piece)) return null;
                    structure[--length] = piece;
                }
                else if (Local.IsGetLocal(instruction.OP) &&
                    instruction.OP != OPCode.GetLocal_0)
                {
                    var local = (Local)instruction;
                    pushedLocals.Add(local.Register, --length);
                    if (getLocalEndIndex == -1)
                    {
                        getLocalEndIndex = i;
                    }
                }
                if (length == 0) break;
            }
            for (int i = getLocalEndIndex - 1; i >= 0; i--)
            {
                ASInstruction instruction = code[i];
                if (!Local.IsSetLocal(instruction.OP)) continue;

                var local = (Local)instruction;
                if (pushedLocals.TryGetValue(local.Register, out int structIndex))
                {
                    ASInstruction beforeSet = code[i - 1];
                    pushedLocals.Remove(local.Register);
                    switch (beforeSet.OP)
                    {
                        case OPCode.PushInt:
                        case OPCode.PushByte:
                        case OPCode.Convert_i:
                        structure[structIndex] = 'i';
                        break;

                        case OPCode.Coerce_s:
                        case OPCode.PushString:
                        structure[structIndex] = 's';
                        break;

                        case OPCode.PushTrue:
                        case OPCode.PushFalse:
                        structure[structIndex] = 'B';
                        break;

                        default:
                        throw new Exception($"Don't know what this value type is, tell someone about this please.\r\nOP: {beforeSet.OP}");
                    }
                }
                if (pushedLocals.Count == 0) break;
            }
            return new string(structure);
        }

        private ASMultiname GetTraitType(ASContainer container, ASMultiname traitName)
        {
            if (container == null) return traitName;

            return container.GetTraits(TraitKind.Slot, TraitKind.Constant, TraitKind.Getter)
                .Where(t => t.QName == traitName)
                .FirstOrDefault()?.Type;
        }
        private bool TryGetStructurePiece(ASMultiname multiname, ASClass @class, out char piece)
        {
            ASMultiname returnValueType = multiname;
            if (@class != null)
            {
                returnValueType = GetTraitType(@class, multiname) ?? GetTraitType(@class.Instance, multiname);
            }

            switch (returnValueType.Name.ToLower())
            {
                case "int":
                case "readint":
                case "gettimer": piece = 'i'; break;

                case "byte":
                case "readbyte": piece = 'b'; break;

                case "double":
                case "readdouble": piece = 'd'; break;

                case "string":
                case "readstring": piece = 's'; break;

                case "boolean":
                case "readboolean": piece = 'B'; break;

                case "array": piece = char.MinValue; break;
                default:
                {
                    if (!IsOutgoing && !HGame.IsValidIdentifier(returnValueType.Name, true))
                    {
                        piece = 'i'; // This reference call is most likely towards 'readInt'
                    }
                    else piece = char.MinValue;
                    break;
                }
            }
            return piece != char.MinValue;
        }
        #endregion

        public class HReference
        {
            public bool IsStatic { get; set; }
            public bool IsAnonymous { get; set; }

            public int ClassRank { get; set; }
            public int MethodRank { get; set; }
            public int InstructionRank { get; set; }

            public int GroupCount { get; set; }

            public ASClass FromClass { get; set; }
            public ASMethod FromMethod { get; set; }
        }
    }
}