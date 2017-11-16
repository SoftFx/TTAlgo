namespace SoftFX.Net.BotAgent
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using SoftFX.Net.Core;
    
    #pragma warning disable 164
    
    struct BoolArray
    {
        public BoolArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public bool this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetBool(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetBool(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BoolNullArray
    {
        public BoolNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public bool? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetBoolNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetBoolNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct CharArray
    {
        public CharArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public char this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetChar(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetChar(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct CharNullArray
    {
        public CharNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public char? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetCharNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetCharNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UCharArray
    {
        public UCharArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public char this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetUChar(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetUChar(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UCharNullArray
    {
        public UCharNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public char? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetUCharNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetUCharNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ByteArray
    {
        public ByteArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public byte this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetByte(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetByte(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ByteNullArray
    {
        public ByteNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public byte? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetByteNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetByteNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct SByteArray
    {
        public SByteArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public sbyte this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetSByte(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetSByte(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct SByteNullArray
    {
        public SByteNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public sbyte? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetSByteNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetSByteNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ShortArray
    {
        public ShortArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 2);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public short this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                data_.SetShort(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                return data_.GetShort(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ShortNullArray
    {
        public ShortNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 2);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public short? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                data_.SetShortNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                return data_.GetShortNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UShortArray
    {
        public UShortArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 2);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ushort this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                data_.SetUShort(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                return data_.GetUShort(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UShortNullArray
    {
        public UShortNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 2);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ushort? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                data_.SetUShortNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                return data_.GetUShortNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct IntArray
    {
        public IntArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public int this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetInt(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return data_.GetInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct IntNullArray
    {
        public IntNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public int? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetIntNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return data_.GetIntNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UIntArray
    {
        public UIntArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public uint this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UIntNullArray
    {
        public UIntNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public uint? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUIntNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return data_.GetUIntNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LongArray
    {
        public LongArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public long this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetLong(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetLong(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LongNullArray
    {
        public LongNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public long? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetLongNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetLongNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ULongArray
    {
        public ULongArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ulong this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetULong(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetULong(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ULongNullArray
    {
        public ULongNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ulong? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetULongNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetULongNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct DoubleArray
    {
        public DoubleArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public double this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetDouble(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetDouble(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct DoubleNullArray
    {
        public DoubleNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public double? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetDoubleNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetDoubleNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct DateTimeArray
    {
        public DateTimeArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public DateTime this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetDateTime(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetDateTime(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct DateTimeNullArray
    {
        public DateTimeNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public DateTime? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetDateTimeNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetDateTimeNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct StringArray
    {
        public StringArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public string this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetString(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetString(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct StringNullArray
    {
        public StringNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public string this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetStringNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetStringNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UStringArray
    {
        public UStringArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public string this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetUString(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetUString(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UStringNullArray
    {
        public UStringNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public string this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetUStringNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetUStringNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BytesArray
    {
        public BytesArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public byte[] this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetBytes(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetBytes(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BytesNullArray
    {
        public BytesNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public byte[] this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetBytesNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetBytesNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginKey
    {
        public PluginKey(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string PackageName
        {
            set { data_.SetString(offset_ + 0, value); }
            
            get { return data_.GetString(offset_ + 0); }
        }
        
        public int GetPackageNameLength()
        {
            return data_.GetStringLength(offset_ + 0);
        }
        
        public void SetPackageName(char[] value, int offset, int count)
        {
            data_.SetString(offset_ + 0, value, offset, count);
        }
        
        public void GetPackageName(char[] value, int offset)
        {
            data_.GetString(offset_ + 0, value, offset);
        }
        
        public void ReadPackageName(Stream stream, int size)
        {
            data_.ReadString(offset_ + 0, stream, size);
        }
        
        public void WritePackageName(Stream stream)
        {
            data_.WriteString(offset_ + 0, stream);
        }
        
        public string DescriptorId
        {
            set { data_.SetString(offset_ + 8, value); }
            
            get { return data_.GetString(offset_ + 8); }
        }
        
        public int GetDescriptorIdLength()
        {
            return data_.GetStringLength(offset_ + 8);
        }
        
        public void SetDescriptorId(char[] value, int offset, int count)
        {
            data_.SetString(offset_ + 8, value, offset, count);
        }
        
        public void GetDescriptorId(char[] value, int offset)
        {
            data_.GetString(offset_ + 8, value, offset);
        }
        
        public void ReadDescriptorId(Stream stream, int size)
        {
            data_.ReadString(offset_ + 8, stream, size);
        }
        
        public void WriteDescriptorId(Stream stream)
        {
            data_.WriteString(offset_ + 8, stream);
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginKeyArray
    {
        public PluginKeyArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 16);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginKey this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 16);
                
                return new PluginKey(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountKey
    {
        public AccountKey(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string Login
        {
            set { data_.SetString(offset_ + 0, value); }
            
            get { return data_.GetString(offset_ + 0); }
        }
        
        public int GetLoginLength()
        {
            return data_.GetStringLength(offset_ + 0);
        }
        
        public void SetLogin(char[] value, int offset, int count)
        {
            data_.SetString(offset_ + 0, value, offset, count);
        }
        
        public void GetLogin(char[] value, int offset)
        {
            data_.GetString(offset_ + 0, value, offset);
        }
        
        public void ReadLogin(Stream stream, int size)
        {
            data_.ReadString(offset_ + 0, stream, size);
        }
        
        public void WriteLogin(Stream stream)
        {
            data_.WriteString(offset_ + 0, stream);
        }
        
        public string Server
        {
            set { data_.SetString(offset_ + 8, value); }
            
            get { return data_.GetString(offset_ + 8); }
        }
        
        public int GetServerLength()
        {
            return data_.GetStringLength(offset_ + 8);
        }
        
        public void SetServer(char[] value, int offset, int count)
        {
            data_.SetString(offset_ + 8, value, offset, count);
        }
        
        public void GetServer(char[] value, int offset)
        {
            data_.GetString(offset_ + 8, value, offset);
        }
        
        public void ReadServer(Stream stream, int size)
        {
            data_.ReadString(offset_ + 8, stream, size);
        }
        
        public void WriteServer(Stream stream)
        {
            data_.WriteString(offset_ + 8, stream);
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountKeyArray
    {
        public AccountKeyArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 16);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public AccountKey this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 16);
                
                return new AccountKey(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BotModel
    {
        public BotModel(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string InstanceId
        {
            set { data_.SetString(offset_ + 0, value); }
            
            get { return data_.GetString(offset_ + 0); }
        }
        
        public int GetInstanceIdLength()
        {
            return data_.GetStringLength(offset_ + 0);
        }
        
        public void SetInstanceId(char[] value, int offset, int count)
        {
            data_.SetString(offset_ + 0, value, offset, count);
        }
        
        public void GetInstanceId(char[] value, int offset)
        {
            data_.GetString(offset_ + 0, value, offset);
        }
        
        public void ReadInstanceId(Stream stream, int size)
        {
            data_.ReadString(offset_ + 0, stream, size);
        }
        
        public void WriteInstanceId(Stream stream)
        {
            data_.WriteString(offset_ + 0, stream);
        }
        
        public bool Isolated
        {
            set { data_.SetBool(offset_ + 8, value); }
            
            get { return data_.GetBool(offset_ + 8); }
        }
        
        public string Permissions
        {
            set { data_.SetString(offset_ + 9, value); }
            
            get { return data_.GetString(offset_ + 9); }
        }
        
        public int GetPermissionsLength()
        {
            return data_.GetStringLength(offset_ + 9);
        }
        
        public void SetPermissions(char[] value, int offset, int count)
        {
            data_.SetString(offset_ + 9, value, offset, count);
        }
        
        public void GetPermissions(char[] value, int offset)
        {
            data_.GetString(offset_ + 9, value, offset);
        }
        
        public void ReadPermissions(Stream stream, int size)
        {
            data_.ReadString(offset_ + 9, stream, size);
        }
        
        public void WritePermissions(Stream stream)
        {
            data_.WriteString(offset_ + 9, stream);
        }
        
        public uint State
        {
            set { data_.SetUInt(offset_ + 17, value); }
            
            get { return data_.GetUInt(offset_ + 17); }
        }
        
        public string Config
        {
            set { data_.SetString(offset_ + 21, value); }
            
            get { return data_.GetString(offset_ + 21); }
        }
        
        public int GetConfigLength()
        {
            return data_.GetStringLength(offset_ + 21);
        }
        
        public void SetConfig(char[] value, int offset, int count)
        {
            data_.SetString(offset_ + 21, value, offset, count);
        }
        
        public void GetConfig(char[] value, int offset)
        {
            data_.GetString(offset_ + 21, value, offset);
        }
        
        public void ReadConfig(Stream stream, int size)
        {
            data_.ReadString(offset_ + 21, stream, size);
        }
        
        public void WriteConfig(Stream stream)
        {
            data_.WriteString(offset_ + 21, stream);
        }
        
        public AccountKey Account
        {
            get { return new AccountKey(data_, offset_ + 29); }
        }
        
        public PluginKey Plugin
        {
            get { return new PluginKey(data_, offset_ + 45); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BotModelArray
    {
        public BotModelArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 61);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public BotModel this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 61);
                
                return new BotModel(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountModel
    {
        public AccountModel(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public AccountKey Key
        {
            get { return new AccountKey(data_, offset_ + 0); }
        }
        
        public BotModelArray Bots
        {
            get { return new BotModelArray(data_, offset_ + 16); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountModelArray
    {
        public AccountModelArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 24);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public AccountModel this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 24);
                
                return new AccountModel(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct Request
    {
        public static implicit operator Message(Request message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public Request(int i)
        {
            info_ = BotAgent.Info.Request;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 0);
        }
        
        public Request(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.Request))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public Request Clone()
        {
            MessageData data = data_.Clone();
            
            return new Request(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.minSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct Report
    {
        public static implicit operator Message(Report message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public Report(int i)
        {
            info_ = BotAgent.Info.Report;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 1);
        }
        
        public Report(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.Report))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string RequestId
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public Report Clone()
        {
            MessageData data = data_.Clone();
            
            return new Report(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.minSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct AccountListRequest
    {
        public static implicit operator Request(AccountListRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static implicit operator Message(AccountListRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public AccountListRequest(int i)
        {
            info_ = BotAgent.Info.AccountListRequest;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 2);
        }
        
        public AccountListRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.AccountListRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public AccountListRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new AccountListRequest(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.minSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct AccountListReport
    {
        public static implicit operator Report(AccountListReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static implicit operator Message(AccountListReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public AccountListReport(int i)
        {
            info_ = BotAgent.Info.AccountListReport;
            data_ = new MessageData(24);
            
            data_.SetInt(4, 3);
        }
        
        public AccountListReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.AccountListReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string RequestId
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public AccountModelArray Accounts
        {
            get { return new AccountModelArray(data_, 16); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public AccountListReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new AccountListReport(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.minSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    
    
    class Is
    {
        public static bool Request(Message message)
        {
            return message.Info.Is(Info.Request);
        }
        
        public static bool Report(Message message)
        {
            return message.Info.Is(Info.Report);
        }
        
        public static bool AccountListRequest(Request message)
        {
            return message.Info.Is(Info.AccountListRequest);
        }
        
        public static bool AccountListRequest(Message message)
        {
            return message.Info.Is(Info.AccountListRequest);
        }
        
        public static bool AccountListReport(Report message)
        {
            return message.Info.Is(Info.AccountListReport);
        }
        
        public static bool AccountListReport(Message message)
        {
            return message.Info.Is(Info.AccountListReport);
        }
    }
    
    class Cast
    {
        public static Message Message(Request message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static Request Request(Message message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static Message Message(Report message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static Report Report(Message message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static Request Request(AccountListRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static AccountListRequest AccountListRequest(Request message)
        {
            return new AccountListRequest(message.Info, message.Data);
        }
        
        public static Message Message(AccountListRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static AccountListRequest AccountListRequest(Message message)
        {
            return new AccountListRequest(message.Info, message.Data);
        }
        
        public static Report Report(AccountListReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static AccountListReport AccountListReport(Report message)
        {
            return new AccountListReport(message.Info, message.Data);
        }
        
        public static Message Message(AccountListReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static AccountListReport AccountListReport(Message message)
        {
            return new AccountListReport(message.Info, message.Data);
        }
    }
    
    class Info
    {
        static Info()
        {
            ConstructPluginKey();
            ConstructAccountKey();
            ConstructBotModel();
            ConstructAccountModel();
            ConstructRequest();
            ConstructReport();
            ConstructAccountListRequest();
            ConstructAccountListReport();
            ConstructBotAgent();
        }
        
        static void ConstructPluginKey()
        {
            FieldInfo PackageName = new FieldInfo();
            PackageName.name = "PackageName";
            PackageName.offset = 0;
            PackageName.type = FieldType.String;
            PackageName.optional = false;
            PackageName.repeatable = false;
            
            FieldInfo DescriptorId = new FieldInfo();
            DescriptorId.name = "DescriptorId";
            DescriptorId.offset = 8;
            DescriptorId.type = FieldType.String;
            DescriptorId.optional = false;
            DescriptorId.repeatable = false;
            
            PluginKey = new GroupInfo();
            PluginKey.name = "PluginKey";
            PluginKey.minSize = 16;
            PluginKey.fields = new FieldInfo[2];
            PluginKey.fields[0] = PackageName;
            PluginKey.fields[1] = DescriptorId;
        }
        
        static void ConstructAccountKey()
        {
            FieldInfo Login = new FieldInfo();
            Login.name = "Login";
            Login.offset = 0;
            Login.type = FieldType.String;
            Login.optional = false;
            Login.repeatable = false;
            
            FieldInfo Server = new FieldInfo();
            Server.name = "Server";
            Server.offset = 8;
            Server.type = FieldType.String;
            Server.optional = false;
            Server.repeatable = false;
            
            AccountKey = new GroupInfo();
            AccountKey.name = "AccountKey";
            AccountKey.minSize = 16;
            AccountKey.fields = new FieldInfo[2];
            AccountKey.fields[0] = Login;
            AccountKey.fields[1] = Server;
        }
        
        static void ConstructBotModel()
        {
            FieldInfo InstanceId = new FieldInfo();
            InstanceId.name = "InstanceId";
            InstanceId.offset = 0;
            InstanceId.type = FieldType.String;
            InstanceId.optional = false;
            InstanceId.repeatable = false;
            
            FieldInfo Isolated = new FieldInfo();
            Isolated.name = "Isolated";
            Isolated.offset = 8;
            Isolated.type = FieldType.Bool;
            Isolated.optional = false;
            Isolated.repeatable = false;
            
            FieldInfo Permissions = new FieldInfo();
            Permissions.name = "Permissions";
            Permissions.offset = 9;
            Permissions.type = FieldType.String;
            Permissions.optional = false;
            Permissions.repeatable = false;
            
            FieldInfo State = new FieldInfo();
            State.name = "State";
            State.offset = 17;
            State.type = FieldType.UShort;
            State.optional = false;
            State.repeatable = false;
            
            FieldInfo Config = new FieldInfo();
            Config.name = "Config";
            Config.offset = 21;
            Config.type = FieldType.String;
            Config.optional = false;
            Config.repeatable = false;
            
            FieldInfo Account = new FieldInfo();
            Account.name = "Account";
            Account.offset = 29;
            Account.type = FieldType.Group;
            Account.groupInfo = Info.AccountKey;
            Account.optional = false;
            Account.repeatable = false;
            
            FieldInfo Plugin = new FieldInfo();
            Plugin.name = "Plugin";
            Plugin.offset = 45;
            Plugin.type = FieldType.Group;
            Plugin.groupInfo = Info.PluginKey;
            Plugin.optional = false;
            Plugin.repeatable = false;
            
            BotModel = new GroupInfo();
            BotModel.name = "BotModel";
            BotModel.minSize = 61;
            BotModel.fields = new FieldInfo[7];
            BotModel.fields[0] = InstanceId;
            BotModel.fields[1] = Isolated;
            BotModel.fields[2] = Permissions;
            BotModel.fields[3] = State;
            BotModel.fields[4] = Config;
            BotModel.fields[5] = Account;
            BotModel.fields[6] = Plugin;
        }
        
        static void ConstructAccountModel()
        {
            FieldInfo Key = new FieldInfo();
            Key.name = "Key";
            Key.offset = 0;
            Key.type = FieldType.Group;
            Key.groupInfo = Info.AccountKey;
            Key.optional = false;
            Key.repeatable = false;
            
            FieldInfo Bots = new FieldInfo();
            Bots.name = "Bots";
            Bots.offset = 16;
            Bots.type = FieldType.Group;
            Bots.groupInfo = Info.BotModel;
            Bots.optional = false;
            Bots.repeatable = true;
            
            AccountModel = new GroupInfo();
            AccountModel.name = "AccountModel";
            AccountModel.minSize = 24;
            AccountModel.fields = new FieldInfo[2];
            AccountModel.fields[0] = Key;
            AccountModel.fields[1] = Bots;
        }
        
        static void ConstructRequest()
        {
            FieldInfo Id = new FieldInfo();
            Id.name = "Id";
            Id.offset = 8;
            Id.type = FieldType.String;
            Id.optional = false;
            Id.repeatable = false;
            
            Request = new MessageInfo();
            Request.name = "Request";
            Request.id = 0;
            Request.minSize = 16;
            Request.fields = new FieldInfo[1];
            Request.fields[0] = Id;
        }
        
        static void ConstructReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.String;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            Report = new MessageInfo();
            Report.name = "Report";
            Report.id = 1;
            Report.minSize = 16;
            Report.fields = new FieldInfo[1];
            Report.fields[0] = RequestId;
        }
        
        static void ConstructAccountListRequest()
        {
            FieldInfo Id = new FieldInfo();
            Id.name = "Id";
            Id.offset = 8;
            Id.type = FieldType.String;
            Id.optional = false;
            Id.repeatable = false;
            
            AccountListRequest = new MessageInfo();
            AccountListRequest.parentInfo = Request;
            AccountListRequest.name = "AccountListRequest";
            AccountListRequest.id = 2;
            AccountListRequest.minSize = 16;
            AccountListRequest.fields = new FieldInfo[1];
            AccountListRequest.fields[0] = Id;
        }
        
        static void ConstructAccountListReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.String;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            FieldInfo Accounts = new FieldInfo();
            Accounts.name = "Accounts";
            Accounts.offset = 16;
            Accounts.type = FieldType.Group;
            Accounts.groupInfo = Info.AccountModel;
            Accounts.optional = false;
            Accounts.repeatable = true;
            
            AccountListReport = new MessageInfo();
            AccountListReport.parentInfo = Report;
            AccountListReport.name = "AccountListReport";
            AccountListReport.id = 3;
            AccountListReport.minSize = 24;
            AccountListReport.fields = new FieldInfo[2];
            AccountListReport.fields[0] = RequestId;
            AccountListReport.fields[1] = Accounts;
        }
        
        
        
        static void ConstructBotAgent()
        {
            BotAgent = new ProtocolInfo();
            BotAgent.name = "BotAgent";
            BotAgent.majorVersion = 1;
            BotAgent.minorVersion = 0;
            BotAgent.AddMessageInfo(Request);
            BotAgent.AddMessageInfo(Report);
            BotAgent.AddMessageInfo(AccountListRequest);
            BotAgent.AddMessageInfo(AccountListReport);
        }
        
        public static GroupInfo PluginKey;
        public static GroupInfo AccountKey;
        public static GroupInfo BotModel;
        public static GroupInfo AccountModel;
        public static MessageInfo Request;
        public static MessageInfo Report;
        public static MessageInfo AccountListRequest;
        public static MessageInfo AccountListReport;
        public static ProtocolInfo BotAgent;
    }
    
    class ClientContext : Context
    {
        public ClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class ConnectClientContext : ClientContext
    {
        public ConnectClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class DisconnectClientContext : ClientContext
    {
        public DisconnectClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class AccountListRequestClientContext : ClientContext
    {
        public AccountListRequestClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    struct ClientSessionLogOptions
    {
        public ClientSessionLogOptions(string directory)
        {
            Directory = directory;
#if DEBUG
            Events = true;
            States = false;
            Messages = true;
#else
            Events = false;
            States = false;
            Messages = false;
#endif
        }
        
        public string Directory;
        
        public bool Events;
        
        public bool States;
        
        public bool Messages;
    }
    
    struct ClientSessionOptions
    {
        public ClientSessionOptions(int connectPort)
        {
            ConnectPort = connectPort;
            ConnectionType = ConnectionType.Socket;
            ServerCertificateName = null;
            Certificates = null;
            ConnectMaxCount = -1;
            ReconnectMaxCount = -1;
            ConnectInterval = 10000;
            HeartbeatInterval = 10000;
            SendBufferSize = 1024 * 1024;
            OptimizationType = OptimizationType.Latency;
            Log = new ClientSessionLogOptions("Logs");
        }
        
        public int ConnectPort;
        
        public ConnectionType ConnectionType;
        
        public string ServerCertificateName;
        
        public X509Certificate2Collection Certificates;
        
        public int ConnectMaxCount;
        
        public int ReconnectMaxCount;
        
        public int ConnectInterval;
        
        public int HeartbeatInterval;
        
        public int SendBufferSize;
        
        public OptimizationType OptimizationType;
        
        public ClientSessionLogOptions Log;
    }
    
    class ClientSession
    {
        public ClientSession(string name, ClientSessionOptions options)
        {
            Core.ClientSessionOptions coreOptions = new Core.ClientSessionOptions(options.ConnectPort);
            coreOptions.ConnectionType = options.ConnectionType;
            coreOptions.ServerCertificateName = options.ServerCertificateName;
            coreOptions.Certificates = options.Certificates;
            coreOptions.WaitEventMask = WaitEventMask.Send;
            coreOptions.ConnectMaxCount = options.ConnectMaxCount;
            coreOptions.ReconnectMaxCount = options.ReconnectMaxCount;
            coreOptions.ConnectInterval = options.ConnectInterval;
            coreOptions.HeartbeatInterval = options.HeartbeatInterval;
            coreOptions.SendBufferSize = options.SendBufferSize;
            coreOptions.OptimizationType = options.OptimizationType;
            coreOptions.Log.Directory = options.Log.Directory;
            coreOptions.Log.Events = options.Log.Events;
            coreOptions.Log.States = options.Log.States;
            coreOptions.Log.Messages = options.Log.Messages;
            
            coreSession_ = new Core.ClientSession(name, Info.BotAgent, coreOptions);
            coreSession_.OnConnect = new Core.ClientSession.OnConnectDelegate(this.OnCoreConnect);
            coreSession_.OnConnectError = new Core.ClientSession.OnConnectErrorDelegate(this.OnCoreConnectError);
            coreSession_.OnDisconnect = new Core.ClientSession.OnDisconnectDelegate(this.OnCoreDisconnect);
            coreSession_.OnReceive = new Core.ClientSession.OnReceiveDelegate(this.OnCoreReceive);
            coreSession_.OnSend = new Core.ClientSession.OnSendDelegate(this.OnCoreSend);
            
            stateMutex_ = new object();
            connected_ = false;
            
            Event_AccountListReport_53_10_AccountListReport_ = new Event_AccountListReport_53_10_AccountListReport(this);
            
            event_ = null;
        }
        
        public string Name
        {
            get { return coreSession_.Name; }
        }
        
        public Guid Guid
        {
            get { return coreSession_.Guid; }
        }
        
        public ClientSessionListener Listener
        {
            set
            {
                lock (stateMutex_)
                {
                    if (connected_)
                        throw new Exception(string.Format("Session is connected : {0}({1})", coreSession_.Name, coreSession_.Guid));
                    
                    listener_ = value;
                }
            }
            
            get
            {
                lock (stateMutex_)
                {
                    return listener_;
                }
            }
        }
        
        public object Data
        {
            set { coreSession_.Data = value; }
            
            get { return coreSession_.Data; }
        }
        
        public int ServerMajorVersion
        {
            get { return coreSession_.ServerMajorVersion; }
        }
        
        public int ServerMinorVersion
        {
            get { return coreSession_.ServerMinorVersion; }
        }
        
        public Guid ServerGuid
        {
            get { return coreSession_.ServerGuid; }
        }
        
        public void Connect(ConnectClientContext context, string address)
        {
            lock (stateMutex_)
            {
                connectContext_ = context;
                coreSession_.Connect(address);
                connected_ = true;
            }
        }
        
        public void Disconnect(DisconnectClientContext context, string text)
        {
            lock (stateMutex_)
            {
                if (connected_)
                {
                    connected_ = false;
                    disconnectContext_ = context;
                    coreSession_.Disconnect(text);
                }
            }
        }
        
        public void Join()
        {
            coreSession_.Join();
        }
        
        public bool Reconnect
        {
            set
            {
                coreSession_.Reconnect = value;
            }
            
            get
            {
                return coreSession_.Reconnect;
            }
        }
        
        public void SendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("SendAccountListRequest({0})", message.ToString());
                
                ClientProcessor_.PreprocessSendAccountListRequest(context, message);
                
                if (context != null)
                    context.Reset();
                
                coreSession_.Send(message);
                
                Client:
                
                ClientProcessor_.PostprocessSendAccountListRequest(context, message);
                
                if (ClientProcessor_.Completed)
                    coreSession_.Disconnect("Client disconnect");
            }
        }
        
        public void Send(Message message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("Send({0})", message.ToString());
                
                ClientProcessor_.PreprocessSend(message);
                
                coreSession_.Send(message);
                
                Client:
                
                ClientProcessor_.PostprocessSend(message);
                
                if (ClientProcessor_.Completed)
                    coreSession_.Disconnect("Client disconnect");
            }
        }
        
        public bool WaitSend(Message message, int timeout)
        {
            return coreSession_.WaitSend(message, timeout);
        }
        
        public bool LogEvents
        {
            set { coreSession_.LogEvents = value; }
            
            get { return coreSession_.LogEvents; }
        }
        
        public bool LogStatets
        {
            set { coreSession_.LogStates = value; }
            
            get { return coreSession_.LogStates; }
        }
        
        public bool LogMessages
        {
            set { coreSession_.LogMessages = value; }
            
            get { return coreSession_.LogMessages; }
        }
        
        public void LogError(string format, params object[] args)
        {
            coreSession_.LogError(format, args);
        }
        
        public void LogWarning(string format, params object[] args)
        {
            coreSession_.LogWarning(format, args);
        }
        
        public void LogInfo(string format, params object[] args)
        {
            coreSession_.LogWarning(format, args);
        }
        
        public void LogEvent(string format, params object[] args)
        {
            coreSession_.LogError(format, args);
        }
        
        public void LogState(string format, params object[] args)
        {
            coreSession_.LogState(format, args);
        }
        
        class ClientProcessor
        {
            public ClientProcessor(ClientSession session)
            {
                session_ = session;
                
                State_51_9_ = new State_51_9(this);
                State_53_10_ = new State_53_10(this);
                State_0_ = new State_0(this);
                
                state_ = State_51_9_;
                
                if (session_.coreSession_.LogStates)
                    session_.coreSession_.LogState("Client : 51_9");
            }
            
            public bool Completed
            {
                get { return state_ == State_0_; }
            }
            
            public void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
            {
                state_.PreprocessSendAccountListRequest(context, message);
            }
            
            public void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
            {
                state_.PostprocessSendAccountListRequest(context, message);
            }
            
            public void PreprocessSend(Message message)
            {
                state_.PreprocessSend(message);
            }
            
            public void PostprocessSend(Message message)
            {
                state_.PostprocessSend(message);
            }
            
            public void ProcessReceive(Message message)
            {
                state_.ProcessReceive(message);
            }
            
            public void ProcessDisconnect(List<ClientContext> contextList)
            {
                state_.ProcessDisconnect(contextList);
            }
            
            abstract class State
            {
                public State(ClientProcessor processor)
                {
                    processor_ = processor;
                }
                
                public abstract void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message);
                
                public abstract void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message);
                
                public abstract void PreprocessSend(Message message);
                
                public abstract void PostprocessSend(Message message);
                
                public abstract void ProcessReceive(Message message);
                
                public abstract void ProcessDisconnect(List<ClientContext> contextList);
                
                protected ClientProcessor processor_;
            }
            
            class State_51_9 : State
            {
                public State_51_9(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    processor_.State_53_10_.AccountListRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_53_10_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("Client : 53_10");
                }
                
                public override void PreprocessSend(Message message)
                {
                    if (Is.AccountListRequest(message))
                        return;
                    
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 51_9 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                    if (Is.AccountListRequest(message))
                    {
                        processor_.state_ = processor_.State_53_10_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 53_10");
                        
                        return;
                    }
                }
                
                public override void ProcessReceive(Message message)
                {
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 51_9 : {0}", message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            class State_53_10 : State
            {
                public State_53_10(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 53_10 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 53_10 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.AccountListReport(message))
                    {
                        AccountListReport AccountListReport = Cast.AccountListReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_AccountListReport_53_10_AccountListReport_.AccountListRequestClientContext_ = AccountListRequestClientContext_;
                            processor_.session_.Event_AccountListReport_53_10_AccountListReport_.AccountListReport_ = AccountListReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_AccountListReport_53_10_AccountListReport_;
                        }
                        
                        AccountListRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_51_9_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 51_9");
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 53_10 : {0}", message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (AccountListRequestClientContext_ != null)
                        contextList.Add(AccountListRequestClientContext_);
                }
                
                public AccountListRequestClientContext AccountListRequestClientContext_;
            }
            
            class State_0 : State
            {
                public State_0(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            ClientSession session_;
            
            State_51_9 State_51_9_;
            State_53_10 State_53_10_;
            State_0 State_0_;
            
            State state_;
        }
        
        abstract class Event
        {
            public Event(ClientSession session)
            {
                session_ = session;
            }
            
            public abstract void Dispatch();
            
            protected ClientSession session_;
        }
        
        class Event_AccountListReport_53_10_AccountListReport : Event
        {
            public Event_AccountListReport_53_10_AccountListReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnAccountListReport({0})", AccountListReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnAccountListReport(session_, AccountListRequestClientContext_, AccountListReport_);
                    }
                    catch
                    {
                    }
                }
                
                if (AccountListRequestClientContext_ != null)
                    AccountListRequestClientContext_.SetCompleted();
                
                AccountListReport_ = new AccountListReport();
                
                AccountListRequestClientContext_ = null;
            }
            
            public AccountListRequestClientContext AccountListRequestClientContext_;
            
            public AccountListReport AccountListReport_;
        }
        
        void OnCoreConnect(Core.ClientSession clientSession)
        {
            ConnectClientContext connectContext;
            
            lock (stateMutex_)
            {
                ClientProcessor_ = new ClientProcessor(this);
                
                if (ClientProcessor_.Completed)
                    coreSession_.Disconnect("Client disconnect");
                
                connectContext = connectContext_;
                connectContext_ = null;
            }
            
            if (listener_ != null)
            {
                try
                {
                    listener_.OnConnect(this, connectContext);
                }
                catch
                {
                }
            }
            
            if (connectContext != null)
                connectContext.SetCompleted();
        }
        
        void OnCoreConnectError(Core.ClientSession clientSession)
        {
            ConnectClientContext connectContext;
            
            lock (stateMutex_)
            {
                connectContext = connectContext_;
                connectContext_ = null;
            }
            
            if (listener_ != null)
            {
                try
                {
                    listener_.OnConnectError(this, connectContext);
                }
                catch
                {
                }
            }
            
            if (connectContext != null)
                connectContext.SetCompleted();
        }
        
        void OnCoreDisconnect(Core.ClientSession clientSession, string text)
        {
            DisconnectClientContext disconnectContext;
            List<ClientContext> contexList = new List<ClientContext>();
            
            lock (stateMutex_)
            {
                disconnectContext = disconnectContext_;
                disconnectContext_ = null;
                
                ClientProcessor_.ProcessDisconnect(contexList);
                ClientProcessor_ = null;
            }
            
            if (listener_ != null)
            {
                try
                {
                    listener_.OnDisconnect(this, disconnectContext, contexList.ToArray(), text);
                }
                catch
                {
                }
            }
            
            foreach (ClientContext context in contexList)
                context.SetDisconnected(text);
            
            if (disconnectContext != null)
                disconnectContext.SetCompleted();
        }
        
        void OnCoreReceive(Core.ClientSession clientSession, Message message)
        {
            lock (stateMutex_)
            {
                ClientProcessor_.ProcessReceive(message);
                
                Client:
                
                if (ClientProcessor_.Completed)
                    coreSession_.Disconnect("Client disconnect");
            }
            
            if (event_ != null)
            {
                event_.Dispatch();
                event_ = null;
            }
            else
            {
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("OnReceive({0})", message.ToString());
                
                if (listener_ != null)
                {
                    try
                    {
                        listener_.OnReceive(this, message);
                    }
                    catch
                    {
                    }
                }
            }
        }
        
        void OnCoreSend(Core.ClientSession clientSession)
        {
            if (listener_ != null)
            {
                try
                {
                    listener_.OnSend(this);
                }
                catch
                {
                }
            }
        }
        
        Core.ClientSession coreSession_;
        
        ClientSessionListener listener_;
        
        object stateMutex_;
        ConnectClientContext connectContext_;
        DisconnectClientContext disconnectContext_;
        bool connected_;
        
        ClientProcessor ClientProcessor_;
        
        Event_AccountListReport_53_10_AccountListReport Event_AccountListReport_53_10_AccountListReport_;
        
        Event event_;
    }
    
    class ClientSessionListener
    {
        public virtual void OnConnect(ClientSession clientSession, ConnectClientContext connectContext)
        {
        }
        
        public virtual void OnConnectError(ClientSession clientSession, ConnectClientContext connectContext)
        {
        }
        
        public virtual void OnDisconnect(ClientSession clientSession, DisconnectClientContext disconnectContext, ClientContext[] contexts, string text)
        {
        }
        
        public virtual void OnAccountListReport(ClientSession session, AccountListRequestClientContext AccountListRequestClientContext, AccountListReport message)
        {
        }
        
        public virtual void OnReceive(ClientSession clientSession, Message message)
        {
        }
        
        public virtual void OnSend(ClientSession clientSession)
        {
        }
    }
    
    class ServerContext : Context
    {
        public ServerContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class AccountListReportServerContext : ServerContext
    {
        public AccountListReportServerContext(bool waitable) : base(waitable)
        {
        }
    }
    
    struct ServerLogOptions
    {
        public ServerLogOptions(string directory)
        {
            Directory = directory;
#if DEBUG
            Events = true;
            States = false;
            Messages = true;
#else
            Events = false;
            States = false;
            Messages = false;
#endif
        }
        
        public string Directory;
        
        public bool Events;
        
        public bool States;
        
        public bool Messages;
    }
    
    struct ServerOptions
    {
        public ServerOptions(int listeningPort)
        {
            ListeningPort = listeningPort;
            ConnectionType = ConnectionType.Socket;
            Certificate = null;
            RequireClientCertificate = false;
            SessionMaxCount = 1000;
            SessionThreadCount = 1;
            HeartbeatInterval = 10000;
            SendBufferSize = 128 * 1024;
            OptimizationType = OptimizationType.Latency;
            Log = new ServerLogOptions("Logs");
        }
        
        public int ListeningPort;
        
        public ConnectionType ConnectionType;
        
        public X509Certificate2 Certificate;
        
        public bool RequireClientCertificate;
        
        public int SessionMaxCount;
        
        public int SessionThreadCount;
        
        public int HeartbeatInterval;
        
        public int SendBufferSize;
        
        public OptimizationType OptimizationType;
        
        public ServerLogOptions Log;
    }
    
    class Server
    {
        public Server(string name, ServerOptions options)
        {
            Core.ServerOptions coreOptions = new Core.ServerOptions(options.ListeningPort);
            coreOptions.ConnectionType = options.ConnectionType;
            coreOptions.Certificate = options.Certificate;
            coreOptions.RequireClientCertificate = options.RequireClientCertificate;
            coreOptions.SessionMaxCount = options.SessionMaxCount;
            coreOptions.SessionThreadCount = options.SessionThreadCount;
            coreOptions.HeartbeatInterval = options.HeartbeatInterval;
            coreOptions.SendBufferSize = options.SendBufferSize;
            coreOptions.OptimizationType = options.OptimizationType;
            coreOptions.Log.Directory = options.Log.Directory;
            coreOptions.Log.Events = options.Log.Events;
            coreOptions.Log.States = options.Log.States;
            coreOptions.Log.Messages = options.Log.Messages;
            
            coreServer_ = new Core.Server(name, Info.BotAgent, coreOptions);
            coreServer_.OnConnect = new Core.Server.OnConnectDelegate(this.OnCoreConnect);
            coreServer_.OnDisconnect = new Core.Server.OnDisconnectDelegate(this.OnCoreDisconnect);
            coreServer_.OnReceive = new Core.Server.OnReceiveDelegate(this.OnCoreReceive);
            coreServer_.OnSend = new Core.Server.OnSendDelegate(this.OnCoreSend);
            
            stateMutex_ = new object();
            started_ = false;
            
            sessionDictionary_ = new Dictionary<int, Session>();
        }
        
        public string Name
        {
            get { return coreServer_.Name; }
        }
        
        public ServerListener Listener
        {
            set
            {
                lock (stateMutex_)
                {
                    if (started_)
                        throw new Exception(string.Format("Server is started : {0}", coreServer_.Name));
                    
                    listener_ = value;
                }
            }
            
            get
            {
                lock (stateMutex_)
                {
                    return listener_;
                }
            }
        }
        
        public void Start()
        {
            lock (stateMutex_)
            {
                coreServer_.Start();
                started_ = true;
            }
        }
        
        public void Stop(string text)
        {
            lock (stateMutex_)
            {
                if (started_)
                {
                    started_ = false;
                    coreServer_.Stop(text);
                }
            }
        }
        
        public void Join()
        {
            coreServer_.Join();
        }
        
        public int SessionCount
        {
            get { return coreServer_.SessionCount; }
        }
        
        public void BroadcastAccountListReport(AccountListReportServerContext context, AccountListReport message)
        {
            lock (stateMutex_)
            {
                if (! started_)
                    throw new Exception(string.Format("Server is not active : {0}", coreServer_.Name));
                
                foreach (var item in sessionDictionary_)
                {
                    try
                    {
                        item.Value.SendAccountListReport(context, message);
                    }
                    catch (Exception exception)
                    {
                        coreServer_.LogError(exception.Message);
                    }
                }
            }
        }
        
        public void Broadcast(Message message)
        {
            lock (stateMutex_)
            {
                if (! started_)
                    throw new Exception(string.Format("Server is not active : {0}", coreServer_.Name));
                
                foreach (var item in sessionDictionary_)
                {
                    try
                    {
                        item.Value.Send(message);
                    }
                    catch (Exception exception)
                    {
                        coreServer_.LogError(exception.Message);
                    }
                }
            }
        }
        
        public void SendAccountListReport(int sessionId, AccountListReportServerContext context, AccountListReport message)
        {
            lock (stateMutex_)
            {
                if (! started_)
                    throw new Exception(string.Format("Server is not active : {0}", coreServer_.Name));
                
                Session session;
                if (! sessionDictionary_.TryGetValue(sessionId, out session))
                    throw new Exception(string.Format("Session does not exist : {0}({1})", coreServer_.Name, sessionId));
                
                session.SendAccountListReport(context, message);
            }
        }
        
        public void Send(int sessionId, Message message)
        {
            lock (stateMutex_)
            {
                if (! started_)
                    throw new Exception(string.Format("Server is not active : {0}", coreServer_.Name));
                
                Session session;
                if (! sessionDictionary_.TryGetValue(sessionId, out session))
                    throw new Exception(string.Format("Session does not exist : {0}({1})", coreServer_.Name, sessionId));
                
                session.Send(message);
            }
        }
        
        public void Disconnect(string text)
        {
            coreServer_.Disconnect(text);
        }
        
        public void Disconnect(int sessionId, string text)
        {
            coreServer_.Disconnect(sessionId, text);
        }
        
        public bool LogEvents
        {
            set { coreServer_.LogEvents = value; }
            
            get { return coreServer_.LogEvents; }
        }
        
        public bool LogStatets
        {
            set { coreServer_.LogStates = value; }
            
            get { return coreServer_.LogStates; }
        }
        
        public bool LogMessages
        {
            set { coreServer_.LogMessages = value; }
            
            get { return coreServer_.LogMessages; }
        }
        
        public void LogError(string format, params object[] args)
        {
            coreServer_.LogError(format, args);
        }
        
        public void LogWarning(string format, params object[] args)
        {
            coreServer_.LogWarning(format, args);
        }
        
        public void LogInfo(string format, params object[] args)
        {
            coreServer_.LogWarning(format, args);
        }
        
        public void LogEvent(string format, params object[] args)
        {
            coreServer_.LogError(format, args);
        }
        
        public void LogState(string format, params object[] args)
        {
            coreServer_.LogState(format, args);
        }
        
        public class Session
        {
            internal Session(Server server, Core.Server.Session coreSession)
            {
                server_ = server;
                coreSession_ = coreSession;
                
                data_ = null;
                
                stateMutex_ = new object();
                
                Event_AccountListRequest_63_9_AccountListRequest_ = new Event_AccountListRequest_63_9_AccountListRequest(this);
                
                event_ = null;
            }
            
            public int Id
            {
                get { return coreSession_.Id; }
            }
            
            public Guid Guid
            {
                get { return coreSession_.Guid; }
            }
            
            public object Data
            {
                set { data_ = value; }
                
                get { return data_; }
            }
            
            public int ClientMajorVersion
            {
                get { return coreSession_.ClientMajorVersion; }
            }
            
            public int ClientMinorVersion
            {
                get { return coreSession_.ClientMinorVersion; }
            }
            
            public Guid ClientGuid
            {
                get { return coreSession_.ClientGuid; }
            }
            
            public void Disconnect(string text)
            {
                lock (stateMutex_)
                {
                    coreSession_.Disconnect(text);
                }
            }
            
            public void SendAccountListReport(AccountListReportServerContext context, AccountListReport message)
            {
                lock (stateMutex_)
                {
                    if (ServerProcessor_ == null)
                        throw new Exception(string.Format("Session is not active : {0}({1})", server_.coreServer_.Name, coreSession_.Id));
                    
                    if (coreSession_.LogEvents)
                        coreSession_.LogEvent("SendAccountListReport({0})", message.ToString());
                    
                    ServerProcessor_.PreprocessSendAccountListReport(context, message);
                    
                    if (context != null)
                        context.Reset();
                    
                    coreSession_.Send(message);
                    
                    Server:
                    
                    ServerProcessor_.PostprocessSendAccountListReport(context, message);
                    
                    if (ServerProcessor_.Completed)
                        coreSession_.Disconnect("Server disconnect");
                }
            }
            
            public void Send(Message message)
            {
                lock (stateMutex_)
                {
                    if (ServerProcessor_ == null)
                        throw new Exception(string.Format("Session is not active : {0}({1})", server_.coreServer_.Name, coreSession_.Guid));
                    
                    if (coreSession_.LogEvents)
                        coreSession_.LogEvent("Send({0})", message.ToString());
                    
                    ServerProcessor_.PreprocessSend(message);
                    
                    coreSession_.Send(message);
                    
                    Server:
                    
                    ServerProcessor_.PostprocessSend(message);
                    
                    if (ServerProcessor_.Completed)
                        coreSession_.Disconnect("Server disconnect");
                }
            }
            
            public void LogError(string format, params object[] args)
            {
                coreSession_.LogError(format, args);
            }
            
            public void LogWarning(string format, params object[] args)
            {
                coreSession_.LogWarning(format, args);
            }
            
            public void LogInfo(string format, params object[] args)
            {
                coreSession_.LogWarning(format, args);
            }
            
            public void LogEvent(string format, params object[] args)
            {
                coreSession_.LogError(format, args);
            }
            
            public void LogState(string format, params object[] args)
            {
                coreSession_.LogState(format, args);
            }
            
            class ServerProcessor
            {
                public ServerProcessor(Session session)
                {
                    session_ = session;
                    
                    State_63_9_ = new State_63_9(this);
                    State_65_10_ = new State_65_10(this);
                    State_0_ = new State_0(this);
                    
                    state_ = State_63_9_;
                    
                    if (session_.coreSession_.LogStates)
                        session_.coreSession_.LogState("Server : 63_9");
                }
                
                public bool Completed
                {
                    get { return state_ == State_0_; }
                }
                
                public void PreprocessSendAccountListReport(AccountListReportServerContext context, AccountListReport message)
                {
                    state_.PreprocessSendAccountListReport(context, message);
                }
                
                public void PostprocessSendAccountListReport(AccountListReportServerContext context, AccountListReport message)
                {
                    state_.PostprocessSendAccountListReport(context, message);
                }
                
                public void PreprocessSend(Message message)
                {
                    state_.PreprocessSend(message);
                }
                
                public void PostprocessSend(Message message)
                {
                    state_.PostprocessSend(message);
                }
                
                public void ProcessReceive(Message message)
                {
                    state_.ProcessReceive(message);
                }
                
                public void ProcessDisconnect(List<ServerContext> contextList)
                {
                    state_.ProcessDisconnect(contextList);
                }
                
                abstract class State
                {
                    public State(ServerProcessor processor)
                    {
                        processor_ = processor;
                    }
                    
                    public abstract void PreprocessSendAccountListReport(AccountListReportServerContext context, AccountListReport message);
                    
                    public abstract void PostprocessSendAccountListReport(AccountListReportServerContext context, AccountListReport message);
                    
                    public abstract void PreprocessSend(Message message);
                    
                    public abstract void PostprocessSend(Message message);
                    
                    public abstract void ProcessReceive(Message message);
                    
                    public abstract void ProcessDisconnect(List<ServerContext> contextList);
                    
                    protected ServerProcessor processor_;
                }
                
                class State_63_9 : State
                {
                    public State_63_9(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSendAccountListReport(AccountListReportServerContext context, AccountListReport message)
                    {
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 63_9 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                    }
                    
                    public override void PostprocessSendAccountListReport(AccountListReportServerContext context, AccountListReport message)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 63_9 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        if (Is.AccountListRequest(message))
                        {
                            AccountListRequest AccountListRequest = Cast.AccountListRequest(message);
                            
                            if (processor_.session_.event_ == null)
                            {
                                processor_.session_.Event_AccountListRequest_63_9_AccountListRequest_.AccountListRequest_ = AccountListRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_AccountListRequest_63_9_AccountListRequest_;
                            }
                            
                            processor_.state_ = processor_.State_65_10_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 65_10");
                            
                            return;
                        }
                        
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 63_9 : {0}", message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_65_10 : State
                {
                    public State_65_10(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSendAccountListReport(AccountListReportServerContext context, AccountListReport message)
                    {
                    }
                    
                    public override void PostprocessSendAccountListReport(AccountListReportServerContext context, AccountListReport message)
                    {
                        processor_.state_ = processor_.State_63_9_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Server : 63_9");
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.AccountListReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 65_10 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.AccountListReport(message))
                        {
                            processor_.state_ = processor_.State_63_9_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 63_9");
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 65_10 : {0}", message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_0 : State
                {
                    public State_0(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSendAccountListReport(AccountListReportServerContext context, AccountListReport message)
                    {
                    }
                    
                    public override void PostprocessSendAccountListReport(AccountListReportServerContext context, AccountListReport message)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                Session session_;
                
                State_63_9 State_63_9_;
                State_65_10 State_65_10_;
                State_0 State_0_;
                
                State state_;
            }
            
            abstract class Event
            {
                public Event(Session session)
                {
                    session_ = session;
                }
                
                public abstract void Dispatch();
                
                protected Session session_;
            }
            
            class Event_AccountListRequest_63_9_AccountListRequest : Event
            {
                public Event_AccountListRequest_63_9_AccountListRequest(Session session) : base(session)
                {
                }
                
                public override void Dispatch()
                {
                    if (session_.coreSession_.LogEvents)
                        session_.coreSession_.LogEvent("OnAccountListRequest({0})", AccountListRequest_.ToString());
                    
                    if (session_.server_.listener_ != null)
                    {
                        try
                        {
                            session_.server_.listener_.OnAccountListRequest(session_.server_, session_, AccountListRequest_);
                        }
                        catch
                        {
                        }
                    }
                    
                    AccountListRequest_ = new AccountListRequest();
                }
                
                public AccountListRequest AccountListRequest_;
            }
            
            internal void OnCoreConnect()
            {
                lock (stateMutex_)
                {
                    ServerProcessor_ = new ServerProcessor(this);
                    
                    if (ServerProcessor_.Completed)
                        coreSession_.Disconnect("Server disconnect");
                }
                
                if (server_.listener_ != null)
                {
                    try
                    {
                        server_.listener_.OnConnect(server_, this);
                    }
                    catch
                    {
                    }
                }
            }
            
            internal void OnCoreDisconnect(string text)
            {
                List<ServerContext> contexList = new List<ServerContext>();
                
                lock (stateMutex_)
                {
                    ServerProcessor_.ProcessDisconnect(contexList);
                    ServerProcessor_ = null;
                }
                
                if (server_.listener_ != null)
                {
                    try
                    {
                        server_.listener_.OnDisconnect(server_, this, contexList.ToArray(), text);
                    }
                    catch
                    {
                    }
                }
                
                foreach (ServerContext context in contexList)
                    context.SetDisconnected(text);
            }
            
            internal void OnCoreReceive(Message message)
            {
                lock (stateMutex_)
                {
                    ServerProcessor_.ProcessReceive(message);
                    
                    Server:
                    
                    if (ServerProcessor_.Completed)
                        coreSession_.Disconnect("Server disconnect");
                }
                
                if (event_ != null)
                {
                    event_.Dispatch();
                    event_ = null;
                }
                else
                {
                    if (coreSession_.LogEvents)
                        coreSession_.LogEvent("OnReceive({0})", message.ToString());
                    
                    if (server_.listener_ != null)
                    {
                        try
                        {
                            server_.listener_.OnReceive(server_, this, message);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            
            internal void OnCoreSend()
            {
                if (server_.listener_ != null)
                {
                    try
                    {
                        server_.listener_.OnSend(server_, this);
                    }
                    catch
                    {
                    }
                }
            }
            
            Server server_;
            Core.Server.Session coreSession_;
            
            object data_;
            
            object stateMutex_;
            
            ServerProcessor ServerProcessor_;
            
            Event_AccountListRequest_63_9_AccountListRequest Event_AccountListRequest_63_9_AccountListRequest_;
            
            Event event_;
        }
        
        void OnCoreConnect(Core.Server coreServer, Core.Server.Session coreSession)
        {
            Session session = new Session(this, coreSession);
            
            lock (stateMutex_)
            {
                sessionDictionary_.Add(session.Id, session);
            }
            
            coreSession.Data = session;
            
            session.OnCoreConnect();
        }
        
        void OnCoreDisconnect(Core.Server coreServer, Core.Server.Session coreSession, string text)
        {
            Session session = ((Session) coreSession.Data);
            
            session.OnCoreDisconnect(text);
            
            coreSession.Data = null;
            
            lock (stateMutex_)
            {
                sessionDictionary_.Remove(session.Id);
            }
        }
        
        void OnCoreReceive(Core.Server coreServer, Core.Server.Session coreSession, Message message)
        {
            ((Session) coreSession.Data).OnCoreReceive(message);
        }
        
        void OnCoreSend(Core.Server coreServer, Core.Server.Session coreSession)
        {
            ((Session) coreSession.Data).OnCoreSend();
        }
        
        Core.Server coreServer_;
        
        ServerListener listener_;
        
        object stateMutex_;
        bool started_;
        
        Dictionary<int, Session> sessionDictionary_;
    }
    
    class ServerListener
    {
        public virtual void OnConnect(Server server, Server.Session session)
        {
        }
        
        public virtual void OnDisconnect(Server server, Server.Session session, ServerContext[] contexts, string text)
        {
        }
        
        public virtual void OnAccountListRequest(Server server, Server.Session session, AccountListRequest message)
        {
        }
        
        public virtual void OnReceive(Server server, Server.Session session, Message message)
        {
        }
        
        public virtual void OnSend(Server server, Server.Session session)
        {
        }
    }
    
    #pragma warning restore 164
}
