using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;

namespace NativeDll
{
    public unsafe class Library
    {
        private static readonly Person Person = new()
        {
            Age = 100,
            Gender = Gender.Female
        };

        [UnmanagedCallersOnly(EntryPoint = "GetSerializedPerson")]
        public static nint SerializedPerson()
        {
            var serializedObject = JsonSerializer.Serialize(Person, JsonContext.Default.Person);
            var length = Encoding.UTF8.GetByteCount(serializedObject);

            // Allocate unmanaged memory block
            var bufferPtr = Marshal.AllocHGlobal(length + 1);

            // Write serialized object directly to unmanaged memory
            var buffer = new Span<byte>(bufferPtr.ToPointer(), length);
            Encoding.UTF8.GetBytes(serializedObject, buffer);

            // Add null terminator
            Marshal.WriteByte(bufferPtr, length, 0);

            return bufferPtr;
        }

        [UnmanagedCallersOnly(EntryPoint = "GetDeserializedPerson")]
        public static nint DeserializedPerson(nint stringPtr, int length)
        {
            var serializedData = new Span<byte>((void*)stringPtr, length);

            var person = JsonSerializer.Deserialize(serializedData, JsonContext.Default.Person);

            // Allocate unmanaged memory block
            var bufferPtr = Marshal.AllocHGlobal(Marshal.SizeOf<Person>());

            Marshal.StructureToPtr(person, bufferPtr, false);

            return bufferPtr;
        }

        [UnmanagedCallersOnly(EntryPoint = "GetStringLength")]
        public static int GetStringLength(nint stringPtr)
        {
            var ptr = (byte*)stringPtr;

            // Find the null terminator
            var length = 0;
            while (*(ptr + length) != 0)
            {
                length++;
            }

            return length;
        }
    }

    [JsonSerializable(typeof(Person), GenerationMode = JsonSourceGenerationMode.Metadata)]
    internal partial class JsonContext : JsonSerializerContext { }

    [StructLayout(LayoutKind.Sequential)]
    public class Person
    {
        public int Age { get; set; }
        public Gender Gender { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }
}