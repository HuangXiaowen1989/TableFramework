using System;
using System.Collections.Generic;
namespace TableFramework
{
    public static class BinaryConverter<T1, T2>
    {
        public static Func<T1, T2> Handler;
    }

    public class BinaryHelper
    {
        public static void Init()
        {
            BinaryConverter<Reader, string>.Handler = value => value.ReadString();
            BinaryConverter<Reader, int>.Handler = value => value.ReadInt();
            BinaryConverter<Reader, bool>.Handler = value => value.ReadBool();
            BinaryConverter<Reader, float>.Handler = value => value.ReadFloat();
            BinaryConverter<Reader, List<string>>.Handler = value => value.ReadListString();
            BinaryConverter<Reader, List<int>>.Handler = value => value.ReadListInt();
            BinaryConverter<Reader, List<float>>.Handler = value => value.ReadListFloat();
            BinaryConverter<Reader, List<bool>>.Handler = value => value.ReadListBool();
            BinaryConverter<Reader, Dictionary<string, string>>.Handler = value => value.ReadDicStringString();
            BinaryConverter<Reader, Dictionary<string, int>>.Handler = value => value.ReadDicStringInt();
            BinaryConverter<Reader, Dictionary<int, int>>.Handler = value => value.ReadDicIntInt();
            BinaryConverter<Reader, Dictionary<int, string>>.Handler = value => value.ReadDicIntString();
            BinaryConverter<Reader, Dictionary<int, float>>.Handler = value => value.ReadDicIntFloat();
        }
    }
}

