using Cosmos.IL2CPU.ILOpCodes;
using IL2CPU.Reflection;

using XSharp;
using static XSharp.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldarga)]
  public class Ldarga : ILOp
  {
    public Ldarga(XSharp.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpVar = (OpVar)aOpCode;
      DoExecute(Assembler, aMethod, xOpVar.Value);
    }

    public static void DoExecute(XSharp.Assembler.Assembler Assembler, _MethodInfo aMethod, ushort aParam)
    {
      var xDisplacement = Ldarg.GetArgumentDisplacement(aMethod, aParam);

      /*
       * The function GetArgumentDisplacement() does not give the correct displacement for the Ldarga opcode
       * we need to "fix" it subtracting the argSize and 4
       */
      TypeInfo xArgType;
      if (aMethod.MethodInfo.IsStatic)
      {
        xArgType = aMethod.MethodInfo.ParameterTypes[aParam];
      }
      else
      {
        if (aParam == 0u)
        {
          xArgType = aMethod.MethodInfo.DeclaringType;
          if (xArgType.IsValueType)
          {
            xArgType = xArgType.MakeByReferenceType();
          }
        }
        else
        {
          xArgType = aMethod.MethodInfo.ParameterTypes[aParam - 1];
        }
      }

      uint xArgRealSize = SizeOfType(xArgType);
      uint xArgSize = Align(xArgRealSize, 4);
      XS.Comment("Arg idx = " + aParam);
      XS.Comment("Arg type = " + xArgType);
      XS.Comment("Arg real size = " + xArgRealSize + " aligned size = " + xArgSize);

      xDisplacement -= (int)(xArgSize - 4);
      XS.Comment("Real displacement " + xDisplacement);

      XS.Set(EAX, EBP);
      XS.Set(EBX, (uint)(xDisplacement));
      XS.Add(EAX, EBX);
      XS.Push(EAX);
    }
  }
}
