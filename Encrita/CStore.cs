using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

#pragma warning disable CS8601
// VERSION 3.0.0

namespace cstore
{
    public class Compiler {
        public FileStream file;
        public List<object> cache = new List<object>();

        public Compiler(string filename) {
            this.file = new FileStream(filename, FileMode.Create);
        }

        public Compiler value(object? value) {
            if (value is Array) {
                this.array((Array) value);
                return this;
            }

            byte type = 0;
            bool isCustom = false;
            byte[] segments = value switch {
                int => ValueSerial.integer((int) value, out type),
                long => ValueSerial.longValue((long) value, out type),
                short => ValueSerial.shortValue((short) value, out type),
                uint => ValueSerial.uInt((uint) value, out type),
                ushort => ValueSerial.uShort((ushort) value, out type),
                ulong => ValueSerial.uLong((ulong) value, out type),
                string => ValueSerial.stringValue((string) value, out type),
                float => ValueSerial.floatValue((float) value, out type),
                double => ValueSerial.doubleValue((double) value, out type),
                sbyte => ValueSerial.signbyte((sbyte) value, out type),
                byte => ValueSerial.byteValue((byte) value, out type),
                char => ValueSerial.character((char) value, out type),
                bool => ValueSerial.boolean((bool) value, out type),
                null => ValueSerial.nill(out type),
                _ => _valueObjWrapper(value, out isCustom),
                // _ => throw new CompilerError($"Cannot compile raw value type '{value.GetType().FullName}'"),
            }; if (isCustom) return this;
            this.file.WriteByte(type);
            this.file.WriteByte((byte) segments.Length);
            this.file.Write(segments, 0, segments.Length);
            return this;
        }

        private byte[] _valueObjWrapper(object obj, out bool isCustom) {
            isCustom = true;
            this.obj(obj);
            return new byte[0];
        }

        public Compiler obj(object obj) {
            if (this.cache.Contains(obj)) {
                this.file.WriteByte(21);
                this.file.WriteByte((byte) this.cache.IndexOf(obj));
                return this;
            } if (this.cache.Count == 255) throw new CompilerError($"Cannot have more than 255 objects in compile cache (will fix if needed)");

            this.cache.Add(obj);
            this.file.WriteByte(20);

            object[] fields = Man.serial(obj);
            if (fields.Length > 255) throw new CompilerError("Cannot compile object with more than 255 fields");
            byte length = (byte) fields.Length;
            this.file.WriteByte(length);
            for (byte i = 0; i < length; i++) this.value(fields[i]);
            return this;
        }

        public Compiler array(Array raw) {
            // if (array.Length > 255) throw new CompilerError("Cannot compile array over 255 length limit");
            if (raw.Length > 255) return this.largeArray(raw);
            this.file.WriteByte(18);
            object[] array = raw.Cast<object>().ToArray();
            byte length = (byte) array.Length;
            this.file.WriteByte(length);
            for (byte i = 0; i < length; i++) this.value(array[i]);
            return this;
        }

        public Compiler largeArray(Array raw) {
            this.file.WriteByte(22);
            object[] array = raw.Cast<object>().ToArray();
            this.value(array.Length);
            for (int i = 0; i < array.Length; i++) this.value(array[i]);
            return this;
        }

        public void close() => this.file.Close();
    }

    public static class ValueSerial {
        public static byte[] val(object raw) => Man.numEncode(raw);

        public static (byte, byte[]) posint(int value) => (1, val(value));
        public static (byte, byte[]) negint(int value) => (2, val(-value));
        public static (byte, byte[]) posShort(short value) => (3, val(value));
        public static (byte, byte[]) negShort(short value) => (4, val(-value));
        public static (byte, byte[]) posLong(long value) => (5, val(value));
        public static (byte, byte[]) negLong(long value) => (6, val(-value));
        public static byte[] uInt(uint value, out byte type) {
            type = 7;
            return val(value);
        } public static byte[] uShort(ushort value, out byte type) {
            type = 8;
            return val(value);
        } public static byte[] uLong(ulong value, out byte type) {
            type = 10;
            return val(value);
        } public static byte[] posByte(sbyte value, out byte type) {
            type = 9;
            return val(value);
        } public static byte[] negByte(sbyte value, out byte type) {
            type = 14;
            return val(value);
        } public static byte[] fpoint(float value, out byte type, byte w) {
            type = w;
            byte man = 0;
            while ((int) value != value) {
                value *= 10;
                man++;
            } return new byte[] {man}.Concat(val(value)).ToArray();
        } public static byte[] dpoint(double value, out byte type, byte w) {
            type = w;
            byte man = 0;
            while ((long) value != value) {
                value *= 10;
                man++;
            } return new byte[] {man}.Concat(val(value)).ToArray();
        } public static byte[] posFloat(float value, out byte type) =>
            fpoint(value, out type, 11);
        public static byte[] negFloat(float value, out byte type) =>
            fpoint(-value, out type, 15);
        public static byte[] posDouble(double value, out byte type) =>
            dpoint(value, out type, 12);
        public static byte[] negDouble(double value, out byte type) =>
            dpoint(value, out type, 16);

        public static byte[] integer(int value, out byte type) {
            (type, byte[] data) = value < 0 ? negint(value): posint(value);
            return data;
        }

        public static byte[] longValue(long value, out byte type) {
            (type, byte[] data) = value < 0 ? negLong(value): posLong(value);
            return data;
        }

        public static byte[] shortValue(short value, out byte type) {
            (type, byte[] data) = value < 0 ? negShort(value): posShort(value);
            return data;
        }

        public static byte[] stringValue(string value, out byte type) {
            type = 13;
            return Encoding.UTF8.GetBytes(value);
        }

        public static byte[] floatValue(float value, out byte type) =>
            value < 0 ? negFloat(value, out type): posFloat(value, out type);

        public static byte[] doubleValue(double value, out byte type) =>
            value < 0 ? negDouble(value, out type): posDouble(value, out type);
        
        public static byte[] signbyte(sbyte value, out byte type) =>
            value < 0 ? negByte(value, out type): posByte(value, out type);

        public static byte[] byteValue(byte value, out byte type) {
            type = 0;
            return new byte[] {value};
        }

        public static byte[] character(char value, out byte type) {
            type = 17;
            return Encoding.UTF8.GetBytes(value.ToString());
        }

        public static byte[] nill(out byte type) {
            type = 19;
            return new byte[0];
        }

        public static byte[] boolean(bool value, out byte type) {
            if (value) type = 23;
            else type = 24;
            return new byte[0];
        }

        public static byte[] bool0(out byte type) {
            type = 24;
            return new byte[0];
        }
    }

    public class CSStream {
        private Compiler compiler;
        private Parser parser;
        private string filename;
        private string tmp;
        public const byte id = 37;
        public long Length {get => this.parser.file.Length;}
        public long pos {get => this.parser.file.Position;}
        public List<object?> cache = new List<object?>();

        public static CSStream init(string filename) {
            File.WriteAllBytes(filename, new byte[] {id});
            return new CSStream(filename);
        }

        public void close() {
            // Write all
            this.writeAllCache();
            while (pos < Length) this.compiler.file.WriteByte((byte) this.parser.file.ReadByte());

            // Cleaning up
            this.parser.close();
            this.compiler.close();
            File.Delete(this.tmp);
        }

        ~CSStream() => this.close();

        public CSStream(string filename) {
            if (!File.Exists(filename)) throw new CStoreError($"File '{filename}' does not exist");
            this.filename = filename;
            this.tmp = filename + ".tmp";
            File.Move(filename, this.tmp);

            this.parser = new Parser(this.tmp);
            if (this.parser.file.ReadByte() != id) throw new CStoreError("Cannot read from foreign file format");

            this.compiler = new Compiler(filename);
            this.compiler.file.WriteByte(id);
        }

        public void writeCache() {
            this.compiler.value(this.cache[0]);
            this.compiler.cache = new List<object>();
            this.cache.RemoveAt(0);
        }

        public void writeAllCache() {
            while (this.cache.Count > 0) this.writeCache();
        }

        public bool read(out object? res, Type? type=null) {
            if (this.pos == this.Length) {
                res = null;
                return false;
            } res = this.parser.value(type);
            this.parser.cache = new List<object>();
            this.cache.Add(res);
            return true;
        }

        public bool readObj<T>(out T? res) {
            if (this.pos == this.Length) {
                res = default(T);
                return false;
            } res = this.parser.obj<T>();
            this.parser.cache = new List<object>();
            this.cache.Add(res);
            return true;
        }

        public bool readArray<T>(out T[] res) {
            if (this.pos == this.Length) {
                res = new T[0];
                return false;
            } res = this.parser.array<T>();
            this.cache.Add(res);
            return true;
        }

        public void skip(Type? type=null) => this.compiler.value(this.parser.value(type));

        public void write(object? value) {
            this.compiler.value(value);
            if (this.cache.Count > 0) this.cache.RemoveAt(0);
            this.compiler.cache = new List<object>();
        }

        public void append(object? value) => this.cache.Add(value);
    }

    public static class CStore {
        public static void store(string filename, object obj) {
            if (obj is null) throw new CStoreError("Cannot store null object (pointless)");
            new Compiler(filename).value(obj).close();
        }

        public static object? loadPrimative(string filename) {
            Parser parser = new Parser(filename);
            object? res = parser.value();
            parser.close();
            return res;
        }

        public static T loadObj<T>(string filename) {
            Parser parser = new Parser(filename);
            T res = parser.obj<T>();
            parser.close();
            return res;
        }

        public static T[] loadArray<T>(string filename) {
            Parser parser = new Parser(filename);
            T[] res = parser.array<T>();
            parser.close();
            return res;
        }
    }

    public class CompilerError: Exception {
        public CompilerError(string msg): base(msg) {}
        public override string ToString() => $"[CStore Compiler] Error: {Message}\n{StackTrace}";
    }

    public class ParserError: Exception {
        public ParserError(string msg): base(msg) {}
        public override string ToString() => $"[CStore Parser] Error: {Message}\n{StackTrace}";
    }

    public class CStoreError: Exception {
        public CStoreError(string msg): base(msg) {}
        public override string ToString() => $"[CStore] Error: {Message}\n{StackTrace}";
    }

    public static class Man {
        public static object[] serial(object source) {
            Type type = source.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            object[] res = new object[fields.Length];
            for (int i = 0; i < fields.Length; i++) res[i] = fields[i].GetValue(source);
            return res;
        }

        public static object deserial(Type type, object result, object[] data) {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            for (int i = 0; i < fields.Length; i++) fields[i].SetValue(result, data[i]);
            return result;
        }

        public static object unInit(Type type) => FormatterServices.GetUninitializedObject(type);

        public static Type getFieldType(int idx, Type type) =>
            type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)[idx].FieldType;

        public static byte[] numEncode(object raw) {
            long value = Convert.ToInt64(raw);
            byte[] bytes = new byte[10]; // assume long is 64 bits
            int i = 0;
            do
            {
                byte b = (byte)(value & 0x7F);
                value >>= 7;
                if (value != 0)
                    b |= 0x80;
                bytes[i++] = b;
            } while (value != 0 && i < 10); // stop after 10 bytes

            Array.Resize(ref bytes, i); // trim unused bytes
            return bytes;
        }

        public static long numDecode(byte[] bytes) {
            long value = 0;
            int shift = 0;
            foreach (byte b in bytes)
            {
                value |= (long)(b & 0x7F) << shift;
                shift += 7;
                if ((b & 0x80) == 0)
                    break;
            } return value;
        }
    }

    public class Parser {
        public FileStream file;
        public List<object> cache = new List<object>();

        public Parser(string filename) {
            this.file = new FileStream(filename, FileMode.Open);
        }

        public void close() => this.file.Close();

        public object? value(Type? obj=null) {
            byte type = (byte) this.file.ReadByte();
            byte length = (byte) this.file.ReadByte();
            ValueDeserial value = new ValueDeserial(length, this.file);
            return type switch {
                0 => value.byteValue(),
                1 => value.posInt(),
                2 => value.negInt(),
                3 => value.posShort(),
                4 => value.negShort(),
                5 => value.posLong(),
                6 => value.negLong(),
                7 => value.uintValue(),
                8 => value.ushortValue(),
                9 => value.posByte(),
                10 => value.ulongValue(),
                11 => value.posFloat(),
                12 => value.posDouble(),
                13 => value.stringValue(),
                14 => value.negByte(),
                15 => value.negFloat(),
                16 => value.negDouble(),
                17 => value.character(),
                18 => this._baseArray(length, obj),
                19 => null,
                20 => this._baseObj(length, obj),
                21 => this.cache[length],
                22 => this.largeArray(obj),
                23 => true,
                24 => false,
                _ => throw new ParserError($"Cannot parse value identifier '{type}'"),
            };
        }

        private Array _baseArray(byte length, Type? type) {
            if (type is null) type = typeof(object[]);
            type = type.GetElementType();
            if (type is null) type = typeof(object);
            Array array = Array.CreateInstance(type, length);
            for (byte i = 0; i < length; i++) {
                array.SetValue(this.value(type), i);
            } return array;
        }

        private Array largeArray(Type? type) {
            if (type is null) type = typeof(object[]);
            type = type.GetElementType();
            if (type is null) type = typeof(object);
            int length = new ValueDeserial((byte) this.file.ReadByte(), this.file).posInt();
            Array array = Array.CreateInstance(type, length);
            for (int i = 0; i < length; i++) {
                array.SetValue(this.value(type), i);
            } return array;
        }

        public T[] array<T>() {
            bool large = this.file.ReadByte() == 22;
            byte length = (byte) this.file.ReadByte();
            if (large) return (T[]) this.largeArray(typeof(T[]));
            return (T[]) this._baseArray(length, typeof(T[]));
        }

        private object _baseObj(byte length, Type? type) {
            if (type is null) throw new ParserError("Cannot parse non-primative object without explicit type given");
            object result = Man.unInit(type);
            this.cache.Add(result);
            object[] fields = new object[length];
            for (byte i = 0; i < length; i++) fields[i] = this.value(Man.getFieldType(i, type));
            return Man.deserial(type, result, fields);
        }

        public T obj<T>() {
            this.file.ReadByte();
            byte length = (byte) this.file.ReadByte();
            return (T) this._baseObj(length, typeof(T));
        }
    }

    public class ValueDeserial {
        private byte length;
        private byte pos = 0;
        private FileStream file;

        public ValueDeserial(byte length, FileStream file) {
            this.length = length;
            this.file = file;
        }

        public byte advance() {
            this.pos++;
            return (byte) this.file.ReadByte();
        }

        public byte[] advance(int amount) {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < amount && this.pos < this.length; i++) bytes.Add(this.advance());
            return bytes.ToArray();
        }

        public long val() => Man.numDecode(this.advance(this.length));

        public byte byteValue() => this.advance();

        public int posInt() => (int) val();
        public int negInt() => -posInt();
        
        public short posShort() => (short) val();
        public short negShort() => (short) -val();

        public long posLong() => val();
        public long negLong() => (long) -val();

        public uint uintValue() => (uint) val();
        public ushort ushortValue() => (ushort) val();
        public ulong ulongValue() => (ulong) val();

        public sbyte posByte() => (sbyte) val();
        public sbyte negByte() => (sbyte) -val();

        public float posFloat() {
            byte man = this.advance();
            return val() / MathF.Pow(10, man);
        } public float negFloat() => -posFloat();
        
        public double posDouble() {
            byte man = this.advance();
            return val() / Math.Pow(10, man);
        } public double negDouble() => -posDouble();

        public string stringValue() {
            byte[] bytes = this.advance(this.length);
            return Encoding.UTF8.GetString(bytes);
        }

        public char character() {
            byte[] bytes = this.advance(this.length);
            return Encoding.UTF8.GetString(bytes)[0];
        }
    }
}