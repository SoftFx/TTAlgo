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
    
    struct LoginRequest
    {
        public static implicit operator Message(LoginRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public LoginRequest(int i)
        {
            info_ = BotAgent.Info.LoginRequest;
            data_ = new MessageData(24);
            
            data_.SetInt(4, 0);
        }
        
        public LoginRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.LoginRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Username
        {
            set { data_.SetUString(8, value); }
            
            get { return data_.GetUString(8); }
        }
        
        public int GetUsernameLength()
        {
            return data_.GetUStringLength(8);
        }
        
        public void SetUsername(char[] value, int offset, int count)
        {
            data_.SetUString(8, value, offset, count);
        }
        
        public void GetUsername(char[] value, int offset)
        {
            data_.GetUString(8, value, offset);
        }
        
        public void ReadUsername(Stream stream, int size)
        {
            data_.ReadUString(8, stream, size);
        }
        
        public void WriteUsername(Stream stream)
        {
            data_.WriteUString(8, stream);
        }
        
        public string Password
        {
            set { data_.SetUString(16, value); }
            
            get { return data_.GetUString(16); }
        }
        
        public int GetPasswordLength()
        {
            return data_.GetUStringLength(16);
        }
        
        public void SetPassword(char[] value, int offset, int count)
        {
            data_.SetUString(16, value, offset, count);
        }
        
        public void GetPassword(char[] value, int offset)
        {
            data_.GetUString(16, value, offset);
        }
        
        public void ReadPassword(Stream stream, int size)
        {
            data_.ReadUString(16, stream, size);
        }
        
        public void WritePassword(Stream stream)
        {
            data_.WriteUString(16, stream);
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
        
        public LoginRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new LoginRequest(info_, data);
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
    
    struct LoginReport
    {
        public static implicit operator Message(LoginReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public LoginReport(int i)
        {
            info_ = BotAgent.Info.LoginReport;
            data_ = new MessageData(8);
            
            data_.SetInt(4, 1);
        }
        
        public LoginReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.LoginReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
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
        
        public LoginReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new LoginReport(info_, data);
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
    
    enum LoginRejectReason
    {
        InvalidCredentials = 0,
        InternalServerError = 1,
    }
    
    struct LoginRejectReasonArray
    {
        public LoginRejectReasonArray(MessageData data, int offset)
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
        
        public LoginRejectReason this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (LoginRejectReason) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LoginRejectReasonNullArray
    {
        public LoginRejectReasonNullArray(MessageData data, int offset)
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
        
        public LoginRejectReason? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (LoginRejectReason) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LoginReject
    {
        public static implicit operator Message(LoginReject message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public LoginReject(int i)
        {
            info_ = BotAgent.Info.LoginReject;
            data_ = new MessageData(20);
            
            data_.SetInt(4, 2);
        }
        
        public LoginReject(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.LoginReject))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public LoginRejectReason Reason
        {
            set { data_.SetUInt(8, (uint) value); }
            
            get { return (LoginRejectReason) data_.GetUInt(8); }
        }
        
        public string Text
        {
            set { data_.SetUStringNull(12, value); }
            
            get { return data_.GetUStringNull(12); }
        }
        
        public int? GetTextLength()
        {
            return data_.GetUStringNullLength(12);
        }
        
        public void SetText(char[] value, int offset, int count)
        {
            data_.SetUString(12, value, offset, count);
        }
        
        public void GetText(char[] value, int offset)
        {
            data_.GetUString(12, value, offset);
        }
        
        public void ReadText(Stream stream, int size)
        {
            data_.ReadUString(12, stream, size);
        }
        
        public void WriteText(Stream stream)
        {
            data_.WriteUString(12, stream);
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
        
        public LoginReject Clone()
        {
            MessageData data = data_.Clone();
            
            return new LoginReject(info_, data);
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
    
    struct LogoutRequest
    {
        public static implicit operator Message(LogoutRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public LogoutRequest(int i)
        {
            info_ = BotAgent.Info.LogoutRequest;
            data_ = new MessageData(8);
            
            data_.SetInt(4, 3);
        }
        
        public LogoutRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.LogoutRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
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
        
        public LogoutRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new LogoutRequest(info_, data);
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
    
    enum LogoutReason
    {
        ClientRequest = 0,
        ServerLogout = 1,
        InternalServerError = 2,
    }
    
    struct LogoutReasonArray
    {
        public LogoutReasonArray(MessageData data, int offset)
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
        
        public LogoutReason this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (LogoutReason) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LogoutReasonNullArray
    {
        public LogoutReasonNullArray(MessageData data, int offset)
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
        
        public LogoutReason? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (LogoutReason) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LogoutReport
    {
        public static implicit operator Message(LogoutReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public LogoutReport(int i)
        {
            info_ = BotAgent.Info.LogoutReport;
            data_ = new MessageData(20);
            
            data_.SetInt(4, 4);
        }
        
        public LogoutReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.LogoutReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public LogoutReason Reason
        {
            set { data_.SetUInt(8, (uint) value); }
            
            get { return (LogoutReason) data_.GetUInt(8); }
        }
        
        public string Text
        {
            set { data_.SetUStringNull(12, value); }
            
            get { return data_.GetUStringNull(12); }
        }
        
        public int? GetTextLength()
        {
            return data_.GetUStringNullLength(12);
        }
        
        public void SetText(char[] value, int offset, int count)
        {
            data_.SetUString(12, value, offset, count);
        }
        
        public void GetText(char[] value, int offset)
        {
            data_.GetUString(12, value, offset);
        }
        
        public void ReadText(Stream stream, int size)
        {
            data_.ReadUString(12, stream, size);
        }
        
        public void WriteText(Stream stream)
        {
            data_.WriteUString(12, stream);
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
        
        public LogoutReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new LogoutReport(info_, data);
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
            
            data_.SetInt(4, 5);
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
            
            data_.SetInt(4, 6);
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
    
    enum UpdateType
    {
        Added = 0,
        Updated = 1,
        Removed = 2,
    }
    
    struct UpdateTypeArray
    {
        public UpdateTypeArray(MessageData data, int offset)
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
        
        public UpdateType this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (UpdateType) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UpdateTypeNullArray
    {
        public UpdateTypeNullArray(MessageData data, int offset)
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
        
        public UpdateType? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (UpdateType) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct Update
    {
        public static implicit operator Message(Update message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public Update(int i)
        {
            info_ = BotAgent.Info.Update;
            data_ = new MessageData(20);
            
            data_.SetInt(4, 7);
        }
        
        public Update(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.Update))
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
        
        public UpdateType Type
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (UpdateType) data_.GetUInt(16); }
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
        
        public Update Clone()
        {
            MessageData data = data_.Clone();
            
            return new Update(info_, data);
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
    
    struct PluginKey
    {
        public PluginKey(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string PackageName
        {
            set { data_.SetUString(offset_ + 0, value); }
            
            get { return data_.GetUString(offset_ + 0); }
        }
        
        public int GetPackageNameLength()
        {
            return data_.GetUStringLength(offset_ + 0);
        }
        
        public void SetPackageName(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 0, value, offset, count);
        }
        
        public void GetPackageName(char[] value, int offset)
        {
            data_.GetUString(offset_ + 0, value, offset);
        }
        
        public void ReadPackageName(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 0, stream, size);
        }
        
        public void WritePackageName(Stream stream)
        {
            data_.WriteUString(offset_ + 0, stream);
        }
        
        public string DescriptorId
        {
            set { data_.SetUString(offset_ + 8, value); }
            
            get { return data_.GetUString(offset_ + 8); }
        }
        
        public int GetDescriptorIdLength()
        {
            return data_.GetUStringLength(offset_ + 8);
        }
        
        public void SetDescriptorId(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 8, value, offset, count);
        }
        
        public void GetDescriptorId(char[] value, int offset)
        {
            data_.GetUString(offset_ + 8, value, offset);
        }
        
        public void ReadDescriptorId(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 8, stream, size);
        }
        
        public void WriteDescriptorId(Stream stream)
        {
            data_.WriteUString(offset_ + 8, stream);
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginKeyNull
    {
        public PluginKeyNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 16);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public PluginKey Value
        {
            get { return new PluginKey(data_, GetDataOffset()); }
        }
        
        public string PackageName
        {
            set { data_.SetUString(GetDataOffset() + 0, value); }
            
            get { return data_.GetUString(GetDataOffset() + 0); }
        }
        
        public int GetPackageNameLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 0);
        }
        
        public void SetPackageName(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 0, value, offset, count);
        }
        
        public void GetPackageName(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 0, value, offset);
        }
        
        public void ReadPackageName(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 0, stream, size);
        }
        
        public void WritePackageName(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 0, stream);
        }
        
        public string DescriptorId
        {
            set { data_.SetUString(GetDataOffset() + 8, value); }
            
            get { return data_.GetUString(GetDataOffset() + 8); }
        }
        
        public int GetDescriptorIdLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 8);
        }
        
        public void SetDescriptorId(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 8, value, offset, count);
        }
        
        public void GetDescriptorId(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 8, value, offset);
        }
        
        public void ReadDescriptorId(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 8, stream, size);
        }
        
        public void WriteDescriptorId(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 8, stream);
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
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
    
    struct PluginKeyNullArray
    {
        public PluginKeyNullArray(MessageData data, int offset)
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
        
        public PluginKeyNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new PluginKeyNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    enum PluginType
    {
        Indicator = 0,
        Robot = 1,
        Unknown = 2,
    }
    
    struct PluginTypeArray
    {
        public PluginTypeArray(MessageData data, int offset)
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
        
        public PluginType this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (PluginType) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginTypeNullArray
    {
        public PluginTypeNullArray(MessageData data, int offset)
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
        
        public PluginType? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (PluginType) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginDescriptor
    {
        public PluginDescriptor(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string ApiVersion
        {
            set { data_.SetUString(offset_ + 0, value); }
            
            get { return data_.GetUString(offset_ + 0); }
        }
        
        public int GetApiVersionLength()
        {
            return data_.GetUStringLength(offset_ + 0);
        }
        
        public void SetApiVersion(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 0, value, offset, count);
        }
        
        public void GetApiVersion(char[] value, int offset)
        {
            data_.GetUString(offset_ + 0, value, offset);
        }
        
        public void ReadApiVersion(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 0, stream, size);
        }
        
        public void WriteApiVersion(Stream stream)
        {
            data_.WriteUString(offset_ + 0, stream);
        }
        
        public string Id
        {
            set { data_.SetUString(offset_ + 8, value); }
            
            get { return data_.GetUString(offset_ + 8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetUStringLength(offset_ + 8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetUString(offset_ + 8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteUString(offset_ + 8, stream);
        }
        
        public string DisplayName
        {
            set { data_.SetUString(offset_ + 16, value); }
            
            get { return data_.GetUString(offset_ + 16); }
        }
        
        public int GetDisplayNameLength()
        {
            return data_.GetUStringLength(offset_ + 16);
        }
        
        public void SetDisplayName(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 16, value, offset, count);
        }
        
        public void GetDisplayName(char[] value, int offset)
        {
            data_.GetUString(offset_ + 16, value, offset);
        }
        
        public void ReadDisplayName(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 16, stream, size);
        }
        
        public void WriteDisplayName(Stream stream)
        {
            data_.WriteUString(offset_ + 16, stream);
        }
        
        public string UserDisplayName
        {
            set { data_.SetUString(offset_ + 24, value); }
            
            get { return data_.GetUString(offset_ + 24); }
        }
        
        public int GetUserDisplayNameLength()
        {
            return data_.GetUStringLength(offset_ + 24);
        }
        
        public void SetUserDisplayName(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 24, value, offset, count);
        }
        
        public void GetUserDisplayName(char[] value, int offset)
        {
            data_.GetUString(offset_ + 24, value, offset);
        }
        
        public void ReadUserDisplayName(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 24, stream, size);
        }
        
        public void WriteUserDisplayName(Stream stream)
        {
            data_.WriteUString(offset_ + 24, stream);
        }
        
        public string Category
        {
            set { data_.SetUString(offset_ + 32, value); }
            
            get { return data_.GetUString(offset_ + 32); }
        }
        
        public int GetCategoryLength()
        {
            return data_.GetUStringLength(offset_ + 32);
        }
        
        public void SetCategory(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 32, value, offset, count);
        }
        
        public void GetCategory(char[] value, int offset)
        {
            data_.GetUString(offset_ + 32, value, offset);
        }
        
        public void ReadCategory(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 32, stream, size);
        }
        
        public void WriteCategory(Stream stream)
        {
            data_.WriteUString(offset_ + 32, stream);
        }
        
        public string Version
        {
            set { data_.SetUString(offset_ + 40, value); }
            
            get { return data_.GetUString(offset_ + 40); }
        }
        
        public int GetVersionLength()
        {
            return data_.GetUStringLength(offset_ + 40);
        }
        
        public void SetVersion(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 40, value, offset, count);
        }
        
        public void GetVersion(char[] value, int offset)
        {
            data_.GetUString(offset_ + 40, value, offset);
        }
        
        public void ReadVersion(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 40, stream, size);
        }
        
        public void WriteVersion(Stream stream)
        {
            data_.WriteUString(offset_ + 40, stream);
        }
        
        public string Description
        {
            set { data_.SetUString(offset_ + 48, value); }
            
            get { return data_.GetUString(offset_ + 48); }
        }
        
        public int GetDescriptionLength()
        {
            return data_.GetUStringLength(offset_ + 48);
        }
        
        public void SetDescription(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 48, value, offset, count);
        }
        
        public void GetDescription(char[] value, int offset)
        {
            data_.GetUString(offset_ + 48, value, offset);
        }
        
        public void ReadDescription(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 48, stream, size);
        }
        
        public void WriteDescription(Stream stream)
        {
            data_.WriteUString(offset_ + 48, stream);
        }
        
        public string Copyright
        {
            set { data_.SetUString(offset_ + 56, value); }
            
            get { return data_.GetUString(offset_ + 56); }
        }
        
        public int GetCopyrightLength()
        {
            return data_.GetUStringLength(offset_ + 56);
        }
        
        public void SetCopyright(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 56, value, offset, count);
        }
        
        public void GetCopyright(char[] value, int offset)
        {
            data_.GetUString(offset_ + 56, value, offset);
        }
        
        public void ReadCopyright(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 56, stream, size);
        }
        
        public void WriteCopyright(Stream stream)
        {
            data_.WriteUString(offset_ + 56, stream);
        }
        
        public PluginType Type
        {
            set { data_.SetUInt(offset_ + 64, (uint) value); }
            
            get { return (PluginType) data_.GetUInt(offset_ + 64); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginDescriptorNull
    {
        public PluginDescriptorNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 68);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public PluginDescriptor Value
        {
            get { return new PluginDescriptor(data_, GetDataOffset()); }
        }
        
        public string ApiVersion
        {
            set { data_.SetUString(GetDataOffset() + 0, value); }
            
            get { return data_.GetUString(GetDataOffset() + 0); }
        }
        
        public int GetApiVersionLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 0);
        }
        
        public void SetApiVersion(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 0, value, offset, count);
        }
        
        public void GetApiVersion(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 0, value, offset);
        }
        
        public void ReadApiVersion(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 0, stream, size);
        }
        
        public void WriteApiVersion(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 0, stream);
        }
        
        public string Id
        {
            set { data_.SetUString(GetDataOffset() + 8, value); }
            
            get { return data_.GetUString(GetDataOffset() + 8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 8, stream);
        }
        
        public string DisplayName
        {
            set { data_.SetUString(GetDataOffset() + 16, value); }
            
            get { return data_.GetUString(GetDataOffset() + 16); }
        }
        
        public int GetDisplayNameLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 16);
        }
        
        public void SetDisplayName(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 16, value, offset, count);
        }
        
        public void GetDisplayName(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 16, value, offset);
        }
        
        public void ReadDisplayName(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 16, stream, size);
        }
        
        public void WriteDisplayName(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 16, stream);
        }
        
        public string UserDisplayName
        {
            set { data_.SetUString(GetDataOffset() + 24, value); }
            
            get { return data_.GetUString(GetDataOffset() + 24); }
        }
        
        public int GetUserDisplayNameLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 24);
        }
        
        public void SetUserDisplayName(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 24, value, offset, count);
        }
        
        public void GetUserDisplayName(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 24, value, offset);
        }
        
        public void ReadUserDisplayName(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 24, stream, size);
        }
        
        public void WriteUserDisplayName(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 24, stream);
        }
        
        public string Category
        {
            set { data_.SetUString(GetDataOffset() + 32, value); }
            
            get { return data_.GetUString(GetDataOffset() + 32); }
        }
        
        public int GetCategoryLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 32);
        }
        
        public void SetCategory(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 32, value, offset, count);
        }
        
        public void GetCategory(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 32, value, offset);
        }
        
        public void ReadCategory(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 32, stream, size);
        }
        
        public void WriteCategory(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 32, stream);
        }
        
        public string Version
        {
            set { data_.SetUString(GetDataOffset() + 40, value); }
            
            get { return data_.GetUString(GetDataOffset() + 40); }
        }
        
        public int GetVersionLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 40);
        }
        
        public void SetVersion(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 40, value, offset, count);
        }
        
        public void GetVersion(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 40, value, offset);
        }
        
        public void ReadVersion(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 40, stream, size);
        }
        
        public void WriteVersion(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 40, stream);
        }
        
        public string Description
        {
            set { data_.SetUString(GetDataOffset() + 48, value); }
            
            get { return data_.GetUString(GetDataOffset() + 48); }
        }
        
        public int GetDescriptionLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 48);
        }
        
        public void SetDescription(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 48, value, offset, count);
        }
        
        public void GetDescription(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 48, value, offset);
        }
        
        public void ReadDescription(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 48, stream, size);
        }
        
        public void WriteDescription(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 48, stream);
        }
        
        public string Copyright
        {
            set { data_.SetUString(GetDataOffset() + 56, value); }
            
            get { return data_.GetUString(GetDataOffset() + 56); }
        }
        
        public int GetCopyrightLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 56);
        }
        
        public void SetCopyright(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 56, value, offset, count);
        }
        
        public void GetCopyright(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 56, value, offset);
        }
        
        public void ReadCopyright(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 56, stream, size);
        }
        
        public void WriteCopyright(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 56, stream);
        }
        
        public PluginType Type
        {
            set { data_.SetUInt(GetDataOffset() + 64, (uint) value); }
            
            get { return (PluginType) data_.GetUInt(GetDataOffset() + 64); }
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginDescriptorArray
    {
        public PluginDescriptorArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 68);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginDescriptor this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 68);
                
                return new PluginDescriptor(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginDescriptorNullArray
    {
        public PluginDescriptorNullArray(MessageData data, int offset)
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
        
        public PluginDescriptorNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new PluginDescriptorNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginInfo
    {
        public PluginInfo(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public PluginKey Key
        {
            get { return new PluginKey(data_, offset_ + 0); }
        }
        
        public PluginDescriptor Descriptor
        {
            get { return new PluginDescriptor(data_, offset_ + 16); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginInfoNull
    {
        public PluginInfoNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 84);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public PluginInfo Value
        {
            get { return new PluginInfo(data_, GetDataOffset()); }
        }
        
        public PluginKey Key
        {
            get { return new PluginKey(data_, GetDataOffset() + 0); }
        }
        
        public PluginDescriptor Descriptor
        {
            get { return new PluginDescriptor(data_, GetDataOffset() + 16); }
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginInfoArray
    {
        public PluginInfoArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 84);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginInfo this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 84);
                
                return new PluginInfo(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginInfoNullArray
    {
        public PluginInfoNullArray(MessageData data, int offset)
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
        
        public PluginInfoNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new PluginInfoNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PackageModel
    {
        public PackageModel(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string Name
        {
            set { data_.SetUString(offset_ + 0, value); }
            
            get { return data_.GetUString(offset_ + 0); }
        }
        
        public int GetNameLength()
        {
            return data_.GetUStringLength(offset_ + 0);
        }
        
        public void SetName(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 0, value, offset, count);
        }
        
        public void GetName(char[] value, int offset)
        {
            data_.GetUString(offset_ + 0, value, offset);
        }
        
        public void ReadName(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 0, stream, size);
        }
        
        public void WriteName(Stream stream)
        {
            data_.WriteUString(offset_ + 0, stream);
        }
        
        public DateTime Created
        {
            set { data_.SetDateTime(offset_ + 8, value); }
            
            get { return data_.GetDateTime(offset_ + 8); }
        }
        
        public PluginInfoArray Plugins
        {
            get { return new PluginInfoArray(data_, offset_ + 16); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PackageModelNull
    {
        public PackageModelNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 24);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public PackageModel Value
        {
            get { return new PackageModel(data_, GetDataOffset()); }
        }
        
        public string Name
        {
            set { data_.SetUString(GetDataOffset() + 0, value); }
            
            get { return data_.GetUString(GetDataOffset() + 0); }
        }
        
        public int GetNameLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 0);
        }
        
        public void SetName(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 0, value, offset, count);
        }
        
        public void GetName(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 0, value, offset);
        }
        
        public void ReadName(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 0, stream, size);
        }
        
        public void WriteName(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 0, stream);
        }
        
        public DateTime Created
        {
            set { data_.SetDateTime(GetDataOffset() + 8, value); }
            
            get { return data_.GetDateTime(GetDataOffset() + 8); }
        }
        
        public PluginInfoArray Plugins
        {
            get { return new PluginInfoArray(data_, GetDataOffset() + 16); }
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PackageModelArray
    {
        public PackageModelArray(MessageData data, int offset)
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
        
        public PackageModel this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 24);
                
                return new PackageModel(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PackageModelNullArray
    {
        public PackageModelNullArray(MessageData data, int offset)
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
        
        public PackageModelNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new PackageModelNull(data_, itemOffset);
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
            set { data_.SetUString(offset_ + 0, value); }
            
            get { return data_.GetUString(offset_ + 0); }
        }
        
        public int GetLoginLength()
        {
            return data_.GetUStringLength(offset_ + 0);
        }
        
        public void SetLogin(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 0, value, offset, count);
        }
        
        public void GetLogin(char[] value, int offset)
        {
            data_.GetUString(offset_ + 0, value, offset);
        }
        
        public void ReadLogin(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 0, stream, size);
        }
        
        public void WriteLogin(Stream stream)
        {
            data_.WriteUString(offset_ + 0, stream);
        }
        
        public string Server
        {
            set { data_.SetUString(offset_ + 8, value); }
            
            get { return data_.GetUString(offset_ + 8); }
        }
        
        public int GetServerLength()
        {
            return data_.GetUStringLength(offset_ + 8);
        }
        
        public void SetServer(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 8, value, offset, count);
        }
        
        public void GetServer(char[] value, int offset)
        {
            data_.GetUString(offset_ + 8, value, offset);
        }
        
        public void ReadServer(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 8, stream, size);
        }
        
        public void WriteServer(Stream stream)
        {
            data_.WriteUString(offset_ + 8, stream);
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountKeyNull
    {
        public AccountKeyNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 16);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public AccountKey Value
        {
            get { return new AccountKey(data_, GetDataOffset()); }
        }
        
        public string Login
        {
            set { data_.SetUString(GetDataOffset() + 0, value); }
            
            get { return data_.GetUString(GetDataOffset() + 0); }
        }
        
        public int GetLoginLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 0);
        }
        
        public void SetLogin(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 0, value, offset, count);
        }
        
        public void GetLogin(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 0, value, offset);
        }
        
        public void ReadLogin(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 0, stream, size);
        }
        
        public void WriteLogin(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 0, stream);
        }
        
        public string Server
        {
            set { data_.SetUString(GetDataOffset() + 8, value); }
            
            get { return data_.GetUString(GetDataOffset() + 8); }
        }
        
        public int GetServerLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 8);
        }
        
        public void SetServer(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 8, value, offset, count);
        }
        
        public void GetServer(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 8, value, offset);
        }
        
        public void ReadServer(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 8, stream, size);
        }
        
        public void WriteServer(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 8, stream);
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
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
    
    struct AccountKeyNullArray
    {
        public AccountKeyNullArray(MessageData data, int offset)
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
        
        public AccountKeyNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new AccountKeyNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginPermissions
    {
        public PluginPermissions(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public bool TradeAllowed
        {
            set { data_.SetBool(offset_ + 0, value); }
            
            get { return data_.GetBool(offset_ + 0); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginPermissionsNull
    {
        public PluginPermissionsNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 1);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public PluginPermissions Value
        {
            get { return new PluginPermissions(data_, GetDataOffset()); }
        }
        
        public bool TradeAllowed
        {
            set { data_.SetBool(GetDataOffset() + 0, value); }
            
            get { return data_.GetBool(GetDataOffset() + 0); }
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginPermissionsArray
    {
        public PluginPermissionsArray(MessageData data, int offset)
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
        
        public PluginPermissions this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                
                return new PluginPermissions(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginPermissionsNullArray
    {
        public PluginPermissionsNullArray(MessageData data, int offset)
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
        
        public PluginPermissionsNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new PluginPermissionsNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    enum BotState
    {
        Offline = 0,
        Starting = 1,
        Faulted = 2,
        Online = 3,
        Stopping = 4,
        Broken = 5,
        Reconnecting = 6,
    }
    
    struct BotStateArray
    {
        public BotStateArray(MessageData data, int offset)
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
        
        public BotState this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (BotState) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BotStateNullArray
    {
        public BotStateNullArray(MessageData data, int offset)
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
        
        public BotState? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (BotState) value.Value;
                
                return null;
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
            set { data_.SetUString(offset_ + 0, value); }
            
            get { return data_.GetUString(offset_ + 0); }
        }
        
        public int GetInstanceIdLength()
        {
            return data_.GetUStringLength(offset_ + 0);
        }
        
        public void SetInstanceId(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 0, value, offset, count);
        }
        
        public void GetInstanceId(char[] value, int offset)
        {
            data_.GetUString(offset_ + 0, value, offset);
        }
        
        public void ReadInstanceId(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 0, stream, size);
        }
        
        public void WriteInstanceId(Stream stream)
        {
            data_.WriteUString(offset_ + 0, stream);
        }
        
        public bool Isolated
        {
            set { data_.SetBool(offset_ + 8, value); }
            
            get { return data_.GetBool(offset_ + 8); }
        }
        
        public BotState State
        {
            set { data_.SetUInt(offset_ + 9, (uint) value); }
            
            get { return (BotState) data_.GetUInt(offset_ + 9); }
        }
        
        public PluginPermissions Permissions
        {
            get { return new PluginPermissions(data_, offset_ + 13); }
        }
        
        public AccountKey Account
        {
            get { return new AccountKey(data_, offset_ + 14); }
        }
        
        public PluginKey Plugin
        {
            get { return new PluginKey(data_, offset_ + 30); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BotModelNull
    {
        public BotModelNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 46);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public BotModel Value
        {
            get { return new BotModel(data_, GetDataOffset()); }
        }
        
        public string InstanceId
        {
            set { data_.SetUString(GetDataOffset() + 0, value); }
            
            get { return data_.GetUString(GetDataOffset() + 0); }
        }
        
        public int GetInstanceIdLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 0);
        }
        
        public void SetInstanceId(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 0, value, offset, count);
        }
        
        public void GetInstanceId(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 0, value, offset);
        }
        
        public void ReadInstanceId(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 0, stream, size);
        }
        
        public void WriteInstanceId(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 0, stream);
        }
        
        public bool Isolated
        {
            set { data_.SetBool(GetDataOffset() + 8, value); }
            
            get { return data_.GetBool(GetDataOffset() + 8); }
        }
        
        public BotState State
        {
            set { data_.SetUInt(GetDataOffset() + 9, (uint) value); }
            
            get { return (BotState) data_.GetUInt(GetDataOffset() + 9); }
        }
        
        public PluginPermissions Permissions
        {
            get { return new PluginPermissions(data_, GetDataOffset() + 13); }
        }
        
        public AccountKey Account
        {
            get { return new AccountKey(data_, GetDataOffset() + 14); }
        }
        
        public PluginKey Plugin
        {
            get { return new PluginKey(data_, GetDataOffset() + 30); }
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
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
            data_.ResizeArray(offset_, length, 46);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public BotModel this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 46);
                
                return new BotModel(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BotModelNullArray
    {
        public BotModelNullArray(MessageData data, int offset)
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
        
        public BotModelNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new BotModelNull(data_, itemOffset);
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
    
    struct AccountModelNull
    {
        public AccountModelNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 24);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public AccountModel Value
        {
            get { return new AccountModel(data_, GetDataOffset()); }
        }
        
        public AccountKey Key
        {
            get { return new AccountKey(data_, GetDataOffset() + 0); }
        }
        
        public BotModelArray Bots
        {
            get { return new BotModelArray(data_, GetDataOffset() + 16); }
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
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
    
    struct AccountModelNullArray
    {
        public AccountModelNullArray(MessageData data, int offset)
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
        
        public AccountModelNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new AccountModelNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
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
            
            data_.SetInt(4, 8);
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
            
            data_.SetInt(4, 9);
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
    
    struct PackageListRequest
    {
        public static implicit operator Request(PackageListRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static implicit operator Message(PackageListRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public PackageListRequest(int i)
        {
            info_ = BotAgent.Info.PackageListRequest;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 10);
        }
        
        public PackageListRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.PackageListRequest))
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
        
        public PackageListRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new PackageListRequest(info_, data);
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
    
    struct PackageListReport
    {
        public static implicit operator Report(PackageListReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static implicit operator Message(PackageListReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public PackageListReport(int i)
        {
            info_ = BotAgent.Info.PackageListReport;
            data_ = new MessageData(24);
            
            data_.SetInt(4, 11);
        }
        
        public PackageListReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.PackageListReport))
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
        
        public PackageModelArray Packages
        {
            get { return new PackageModelArray(data_, 16); }
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
        
        public PackageListReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new PackageListReport(info_, data);
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
    
    struct SubscribeRequest
    {
        public static implicit operator Request(SubscribeRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static implicit operator Message(SubscribeRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public SubscribeRequest(int i)
        {
            info_ = BotAgent.Info.SubscribeRequest;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 12);
        }
        
        public SubscribeRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.SubscribeRequest))
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
        
        public SubscribeRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new SubscribeRequest(info_, data);
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
    
    struct SubscribeReport
    {
        public static implicit operator Report(SubscribeReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static implicit operator Message(SubscribeReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public SubscribeReport(int i)
        {
            info_ = BotAgent.Info.SubscribeReport;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 13);
        }
        
        public SubscribeReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.SubscribeReport))
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
        
        public SubscribeReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new SubscribeReport(info_, data);
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
    
    struct AccountModelUpdate
    {
        public static implicit operator Update(AccountModelUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static implicit operator Message(AccountModelUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public AccountModelUpdate(int i)
        {
            info_ = BotAgent.Info.AccountModelUpdate;
            data_ = new MessageData(68);
            
            data_.SetInt(4, 14);
        }
        
        public AccountModelUpdate(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.AccountModelUpdate))
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
        
        public UpdateType Type
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (UpdateType) data_.GetUInt(16); }
        }
        
        public AccountModelNull OldItem
        {
            get { return new AccountModelNull(data_, 20); }
        }
        
        public AccountModelNull NewItem
        {
            get { return new AccountModelNull(data_, 44); }
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
        
        public AccountModelUpdate Clone()
        {
            MessageData data = data_.Clone();
            
            return new AccountModelUpdate(info_, data);
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
    
    struct BotModelUpdate
    {
        public static implicit operator Update(BotModelUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static implicit operator Message(BotModelUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public BotModelUpdate(int i)
        {
            info_ = BotAgent.Info.BotModelUpdate;
            data_ = new MessageData(112);
            
            data_.SetInt(4, 15);
        }
        
        public BotModelUpdate(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.BotModelUpdate))
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
        
        public UpdateType Type
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (UpdateType) data_.GetUInt(16); }
        }
        
        public BotModelNull OldItem
        {
            get { return new BotModelNull(data_, 20); }
        }
        
        public BotModelNull NewItem
        {
            get { return new BotModelNull(data_, 66); }
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
        
        public BotModelUpdate Clone()
        {
            MessageData data = data_.Clone();
            
            return new BotModelUpdate(info_, data);
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
    
    struct PackageModelUpdate
    {
        public static implicit operator Update(PackageModelUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static implicit operator Message(PackageModelUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public PackageModelUpdate(int i)
        {
            info_ = BotAgent.Info.PackageModelUpdate;
            data_ = new MessageData(68);
            
            data_.SetInt(4, 16);
        }
        
        public PackageModelUpdate(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.PackageModelUpdate))
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
        
        public UpdateType Type
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (UpdateType) data_.GetUInt(16); }
        }
        
        public PackageModelNull OldItem
        {
            get { return new PackageModelNull(data_, 20); }
        }
        
        public PackageModelNull NewItem
        {
            get { return new PackageModelNull(data_, 44); }
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
        
        public PackageModelUpdate Clone()
        {
            MessageData data = data_.Clone();
            
            return new PackageModelUpdate(info_, data);
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
        public static bool LoginRequest(Message message)
        {
            return message.Info.Is(Info.LoginRequest);
        }
        
        public static bool LoginReport(Message message)
        {
            return message.Info.Is(Info.LoginReport);
        }
        
        public static bool LoginReject(Message message)
        {
            return message.Info.Is(Info.LoginReject);
        }
        
        public static bool LogoutRequest(Message message)
        {
            return message.Info.Is(Info.LogoutRequest);
        }
        
        public static bool LogoutReport(Message message)
        {
            return message.Info.Is(Info.LogoutReport);
        }
        
        public static bool Request(Message message)
        {
            return message.Info.Is(Info.Request);
        }
        
        public static bool Report(Message message)
        {
            return message.Info.Is(Info.Report);
        }
        
        public static bool Update(Message message)
        {
            return message.Info.Is(Info.Update);
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
        
        public static bool PackageListRequest(Request message)
        {
            return message.Info.Is(Info.PackageListRequest);
        }
        
        public static bool PackageListRequest(Message message)
        {
            return message.Info.Is(Info.PackageListRequest);
        }
        
        public static bool PackageListReport(Report message)
        {
            return message.Info.Is(Info.PackageListReport);
        }
        
        public static bool PackageListReport(Message message)
        {
            return message.Info.Is(Info.PackageListReport);
        }
        
        public static bool SubscribeRequest(Request message)
        {
            return message.Info.Is(Info.SubscribeRequest);
        }
        
        public static bool SubscribeRequest(Message message)
        {
            return message.Info.Is(Info.SubscribeRequest);
        }
        
        public static bool SubscribeReport(Report message)
        {
            return message.Info.Is(Info.SubscribeReport);
        }
        
        public static bool SubscribeReport(Message message)
        {
            return message.Info.Is(Info.SubscribeReport);
        }
        
        public static bool AccountModelUpdate(Update message)
        {
            return message.Info.Is(Info.AccountModelUpdate);
        }
        
        public static bool AccountModelUpdate(Message message)
        {
            return message.Info.Is(Info.AccountModelUpdate);
        }
        
        public static bool BotModelUpdate(Update message)
        {
            return message.Info.Is(Info.BotModelUpdate);
        }
        
        public static bool BotModelUpdate(Message message)
        {
            return message.Info.Is(Info.BotModelUpdate);
        }
        
        public static bool PackageModelUpdate(Update message)
        {
            return message.Info.Is(Info.PackageModelUpdate);
        }
        
        public static bool PackageModelUpdate(Message message)
        {
            return message.Info.Is(Info.PackageModelUpdate);
        }
    }
    
    class Cast
    {
        public static Message Message(LoginRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static LoginRequest LoginRequest(Message message)
        {
            return new LoginRequest(message.Info, message.Data);
        }
        
        public static Message Message(LoginReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static LoginReport LoginReport(Message message)
        {
            return new LoginReport(message.Info, message.Data);
        }
        
        public static Message Message(LoginReject message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static LoginReject LoginReject(Message message)
        {
            return new LoginReject(message.Info, message.Data);
        }
        
        public static Message Message(LogoutRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static LogoutRequest LogoutRequest(Message message)
        {
            return new LogoutRequest(message.Info, message.Data);
        }
        
        public static Message Message(LogoutReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static LogoutReport LogoutReport(Message message)
        {
            return new LogoutReport(message.Info, message.Data);
        }
        
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
        
        public static Message Message(Update message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static Update Update(Message message)
        {
            return new Update(message.Info, message.Data);
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
        
        public static Request Request(PackageListRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static PackageListRequest PackageListRequest(Request message)
        {
            return new PackageListRequest(message.Info, message.Data);
        }
        
        public static Message Message(PackageListRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static PackageListRequest PackageListRequest(Message message)
        {
            return new PackageListRequest(message.Info, message.Data);
        }
        
        public static Report Report(PackageListReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static PackageListReport PackageListReport(Report message)
        {
            return new PackageListReport(message.Info, message.Data);
        }
        
        public static Message Message(PackageListReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static PackageListReport PackageListReport(Message message)
        {
            return new PackageListReport(message.Info, message.Data);
        }
        
        public static Request Request(SubscribeRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static SubscribeRequest SubscribeRequest(Request message)
        {
            return new SubscribeRequest(message.Info, message.Data);
        }
        
        public static Message Message(SubscribeRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static SubscribeRequest SubscribeRequest(Message message)
        {
            return new SubscribeRequest(message.Info, message.Data);
        }
        
        public static Report Report(SubscribeReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static SubscribeReport SubscribeReport(Report message)
        {
            return new SubscribeReport(message.Info, message.Data);
        }
        
        public static Message Message(SubscribeReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static SubscribeReport SubscribeReport(Message message)
        {
            return new SubscribeReport(message.Info, message.Data);
        }
        
        public static Update Update(AccountModelUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static AccountModelUpdate AccountModelUpdate(Update message)
        {
            return new AccountModelUpdate(message.Info, message.Data);
        }
        
        public static Message Message(AccountModelUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static AccountModelUpdate AccountModelUpdate(Message message)
        {
            return new AccountModelUpdate(message.Info, message.Data);
        }
        
        public static Update Update(BotModelUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static BotModelUpdate BotModelUpdate(Update message)
        {
            return new BotModelUpdate(message.Info, message.Data);
        }
        
        public static Message Message(BotModelUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static BotModelUpdate BotModelUpdate(Message message)
        {
            return new BotModelUpdate(message.Info, message.Data);
        }
        
        public static Update Update(PackageModelUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static PackageModelUpdate PackageModelUpdate(Update message)
        {
            return new PackageModelUpdate(message.Info, message.Data);
        }
        
        public static Message Message(PackageModelUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static PackageModelUpdate PackageModelUpdate(Message message)
        {
            return new PackageModelUpdate(message.Info, message.Data);
        }
    }
    
    class Info
    {
        static Info()
        {
            ConstructLoginRequest();
            ConstructLoginReport();
            ConstructLoginRejectReason();
            ConstructLoginReject();
            ConstructLogoutRequest();
            ConstructLogoutReason();
            ConstructLogoutReport();
            ConstructRequest();
            ConstructReport();
            ConstructUpdateType();
            ConstructUpdate();
            ConstructPluginKey();
            ConstructPluginType();
            ConstructPluginDescriptor();
            ConstructPluginInfo();
            ConstructPackageModel();
            ConstructAccountKey();
            ConstructPluginPermissions();
            ConstructBotState();
            ConstructBotModel();
            ConstructAccountModel();
            ConstructAccountListRequest();
            ConstructAccountListReport();
            ConstructPackageListRequest();
            ConstructPackageListReport();
            ConstructSubscribeRequest();
            ConstructSubscribeReport();
            ConstructAccountModelUpdate();
            ConstructBotModelUpdate();
            ConstructPackageModelUpdate();
            ConstructBotAgent();
        }
        
        static void ConstructLoginRequest()
        {
            FieldInfo Username = new FieldInfo();
            Username.name = "Username";
            Username.offset = 8;
            Username.type = FieldType.UString;
            Username.optional = false;
            Username.repeatable = false;
            
            FieldInfo Password = new FieldInfo();
            Password.name = "Password";
            Password.offset = 16;
            Password.type = FieldType.UString;
            Password.optional = false;
            Password.repeatable = false;
            
            LoginRequest = new MessageInfo();
            LoginRequest.name = "LoginRequest";
            LoginRequest.id = 0;
            LoginRequest.minSize = 24;
            LoginRequest.fields = new FieldInfo[2];
            LoginRequest.fields[0] = Username;
            LoginRequest.fields[1] = Password;
        }
        
        static void ConstructLoginReport()
        {
            
            LoginReport = new MessageInfo();
            LoginReport.name = "LoginReport";
            LoginReport.id = 1;
            LoginReport.minSize = 8;
            LoginReport.fields = new FieldInfo[0];
        }
        
        static void ConstructLoginRejectReason()
        {
            EnumMemberInfo InvalidCredentials = new EnumMemberInfo();
            InvalidCredentials.name = "InvalidCredentials";
            InvalidCredentials.value = 0;
            
            EnumMemberInfo InternalServerError = new EnumMemberInfo();
            InternalServerError.name = "InternalServerError";
            InternalServerError.value = 1;
            
            LoginRejectReason = new EnumInfo();
            LoginRejectReason.name = "LoginRejectReason";
            LoginRejectReason.minSize = 4;
            LoginRejectReason.members = new EnumMemberInfo[2];
            LoginRejectReason.members[0] = InvalidCredentials;
            LoginRejectReason.members[1] = InternalServerError;
        }
        
        static void ConstructLoginReject()
        {
            FieldInfo Reason = new FieldInfo();
            Reason.name = "Reason";
            Reason.offset = 8;
            Reason.type = FieldType.Enum;
            Reason.enumInfo = Info.LoginRejectReason;
            Reason.optional = false;
            Reason.repeatable = false;
            
            FieldInfo Text = new FieldInfo();
            Text.name = "Text";
            Text.offset = 12;
            Text.type = FieldType.UString;
            Text.optional = true;
            Text.repeatable = false;
            
            LoginReject = new MessageInfo();
            LoginReject.name = "LoginReject";
            LoginReject.id = 2;
            LoginReject.minSize = 20;
            LoginReject.fields = new FieldInfo[2];
            LoginReject.fields[0] = Reason;
            LoginReject.fields[1] = Text;
        }
        
        static void ConstructLogoutRequest()
        {
            
            LogoutRequest = new MessageInfo();
            LogoutRequest.name = "LogoutRequest";
            LogoutRequest.id = 3;
            LogoutRequest.minSize = 8;
            LogoutRequest.fields = new FieldInfo[0];
        }
        
        static void ConstructLogoutReason()
        {
            EnumMemberInfo ClientRequest = new EnumMemberInfo();
            ClientRequest.name = "ClientRequest";
            ClientRequest.value = 0;
            
            EnumMemberInfo ServerLogout = new EnumMemberInfo();
            ServerLogout.name = "ServerLogout";
            ServerLogout.value = 1;
            
            EnumMemberInfo InternalServerError = new EnumMemberInfo();
            InternalServerError.name = "InternalServerError";
            InternalServerError.value = 2;
            
            LogoutReason = new EnumInfo();
            LogoutReason.name = "LogoutReason";
            LogoutReason.minSize = 4;
            LogoutReason.members = new EnumMemberInfo[3];
            LogoutReason.members[0] = ClientRequest;
            LogoutReason.members[1] = ServerLogout;
            LogoutReason.members[2] = InternalServerError;
        }
        
        static void ConstructLogoutReport()
        {
            FieldInfo Reason = new FieldInfo();
            Reason.name = "Reason";
            Reason.offset = 8;
            Reason.type = FieldType.Enum;
            Reason.enumInfo = Info.LogoutReason;
            Reason.optional = false;
            Reason.repeatable = false;
            
            FieldInfo Text = new FieldInfo();
            Text.name = "Text";
            Text.offset = 12;
            Text.type = FieldType.UString;
            Text.optional = true;
            Text.repeatable = false;
            
            LogoutReport = new MessageInfo();
            LogoutReport.name = "LogoutReport";
            LogoutReport.id = 4;
            LogoutReport.minSize = 20;
            LogoutReport.fields = new FieldInfo[2];
            LogoutReport.fields[0] = Reason;
            LogoutReport.fields[1] = Text;
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
            Request.id = 5;
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
            Report.id = 6;
            Report.minSize = 16;
            Report.fields = new FieldInfo[1];
            Report.fields[0] = RequestId;
        }
        
        static void ConstructUpdateType()
        {
            EnumMemberInfo Added = new EnumMemberInfo();
            Added.name = "Added";
            Added.value = 0;
            
            EnumMemberInfo Updated = new EnumMemberInfo();
            Updated.name = "Updated";
            Updated.value = 1;
            
            EnumMemberInfo Removed = new EnumMemberInfo();
            Removed.name = "Removed";
            Removed.value = 2;
            
            UpdateType = new EnumInfo();
            UpdateType.name = "UpdateType";
            UpdateType.minSize = 4;
            UpdateType.members = new EnumMemberInfo[3];
            UpdateType.members[0] = Added;
            UpdateType.members[1] = Updated;
            UpdateType.members[2] = Removed;
        }
        
        static void ConstructUpdate()
        {
            FieldInfo Id = new FieldInfo();
            Id.name = "Id";
            Id.offset = 8;
            Id.type = FieldType.String;
            Id.optional = false;
            Id.repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.name = "Type";
            Type.offset = 16;
            Type.type = FieldType.Enum;
            Type.enumInfo = Info.UpdateType;
            Type.optional = false;
            Type.repeatable = false;
            
            Update = new MessageInfo();
            Update.name = "Update";
            Update.id = 7;
            Update.minSize = 20;
            Update.fields = new FieldInfo[2];
            Update.fields[0] = Id;
            Update.fields[1] = Type;
        }
        
        static void ConstructPluginKey()
        {
            FieldInfo PackageName = new FieldInfo();
            PackageName.name = "PackageName";
            PackageName.offset = 0;
            PackageName.type = FieldType.UString;
            PackageName.optional = false;
            PackageName.repeatable = false;
            
            FieldInfo DescriptorId = new FieldInfo();
            DescriptorId.name = "DescriptorId";
            DescriptorId.offset = 8;
            DescriptorId.type = FieldType.UString;
            DescriptorId.optional = false;
            DescriptorId.repeatable = false;
            
            PluginKey = new GroupInfo();
            PluginKey.name = "PluginKey";
            PluginKey.minSize = 16;
            PluginKey.fields = new FieldInfo[2];
            PluginKey.fields[0] = PackageName;
            PluginKey.fields[1] = DescriptorId;
        }
        
        static void ConstructPluginType()
        {
            EnumMemberInfo Indicator = new EnumMemberInfo();
            Indicator.name = "Indicator";
            Indicator.value = 0;
            
            EnumMemberInfo Robot = new EnumMemberInfo();
            Robot.name = "Robot";
            Robot.value = 1;
            
            EnumMemberInfo Unknown = new EnumMemberInfo();
            Unknown.name = "Unknown";
            Unknown.value = 2;
            
            PluginType = new EnumInfo();
            PluginType.name = "PluginType";
            PluginType.minSize = 4;
            PluginType.members = new EnumMemberInfo[3];
            PluginType.members[0] = Indicator;
            PluginType.members[1] = Robot;
            PluginType.members[2] = Unknown;
        }
        
        static void ConstructPluginDescriptor()
        {
            FieldInfo ApiVersion = new FieldInfo();
            ApiVersion.name = "ApiVersion";
            ApiVersion.offset = 0;
            ApiVersion.type = FieldType.UString;
            ApiVersion.optional = false;
            ApiVersion.repeatable = false;
            
            FieldInfo Id = new FieldInfo();
            Id.name = "Id";
            Id.offset = 8;
            Id.type = FieldType.UString;
            Id.optional = false;
            Id.repeatable = false;
            
            FieldInfo DisplayName = new FieldInfo();
            DisplayName.name = "DisplayName";
            DisplayName.offset = 16;
            DisplayName.type = FieldType.UString;
            DisplayName.optional = false;
            DisplayName.repeatable = false;
            
            FieldInfo UserDisplayName = new FieldInfo();
            UserDisplayName.name = "UserDisplayName";
            UserDisplayName.offset = 24;
            UserDisplayName.type = FieldType.UString;
            UserDisplayName.optional = false;
            UserDisplayName.repeatable = false;
            
            FieldInfo Category = new FieldInfo();
            Category.name = "Category";
            Category.offset = 32;
            Category.type = FieldType.UString;
            Category.optional = false;
            Category.repeatable = false;
            
            FieldInfo Version = new FieldInfo();
            Version.name = "Version";
            Version.offset = 40;
            Version.type = FieldType.UString;
            Version.optional = false;
            Version.repeatable = false;
            
            FieldInfo Description = new FieldInfo();
            Description.name = "Description";
            Description.offset = 48;
            Description.type = FieldType.UString;
            Description.optional = false;
            Description.repeatable = false;
            
            FieldInfo Copyright = new FieldInfo();
            Copyright.name = "Copyright";
            Copyright.offset = 56;
            Copyright.type = FieldType.UString;
            Copyright.optional = false;
            Copyright.repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.name = "Type";
            Type.offset = 64;
            Type.type = FieldType.Enum;
            Type.enumInfo = Info.PluginType;
            Type.optional = false;
            Type.repeatable = false;
            
            PluginDescriptor = new GroupInfo();
            PluginDescriptor.name = "PluginDescriptor";
            PluginDescriptor.minSize = 68;
            PluginDescriptor.fields = new FieldInfo[9];
            PluginDescriptor.fields[0] = ApiVersion;
            PluginDescriptor.fields[1] = Id;
            PluginDescriptor.fields[2] = DisplayName;
            PluginDescriptor.fields[3] = UserDisplayName;
            PluginDescriptor.fields[4] = Category;
            PluginDescriptor.fields[5] = Version;
            PluginDescriptor.fields[6] = Description;
            PluginDescriptor.fields[7] = Copyright;
            PluginDescriptor.fields[8] = Type;
        }
        
        static void ConstructPluginInfo()
        {
            FieldInfo Key = new FieldInfo();
            Key.name = "Key";
            Key.offset = 0;
            Key.type = FieldType.Group;
            Key.groupInfo = Info.PluginKey;
            Key.optional = false;
            Key.repeatable = false;
            
            FieldInfo Descriptor = new FieldInfo();
            Descriptor.name = "Descriptor";
            Descriptor.offset = 16;
            Descriptor.type = FieldType.Group;
            Descriptor.groupInfo = Info.PluginDescriptor;
            Descriptor.optional = false;
            Descriptor.repeatable = false;
            
            PluginInfo = new GroupInfo();
            PluginInfo.name = "PluginInfo";
            PluginInfo.minSize = 84;
            PluginInfo.fields = new FieldInfo[2];
            PluginInfo.fields[0] = Key;
            PluginInfo.fields[1] = Descriptor;
        }
        
        static void ConstructPackageModel()
        {
            FieldInfo Name = new FieldInfo();
            Name.name = "Name";
            Name.offset = 0;
            Name.type = FieldType.UString;
            Name.optional = false;
            Name.repeatable = false;
            
            FieldInfo Created = new FieldInfo();
            Created.name = "Created";
            Created.offset = 8;
            Created.type = FieldType.DateTime;
            Created.optional = false;
            Created.repeatable = false;
            
            FieldInfo Plugins = new FieldInfo();
            Plugins.name = "Plugins";
            Plugins.offset = 16;
            Plugins.type = FieldType.Group;
            Plugins.groupInfo = Info.PluginInfo;
            Plugins.optional = false;
            Plugins.repeatable = true;
            
            PackageModel = new GroupInfo();
            PackageModel.name = "PackageModel";
            PackageModel.minSize = 24;
            PackageModel.fields = new FieldInfo[3];
            PackageModel.fields[0] = Name;
            PackageModel.fields[1] = Created;
            PackageModel.fields[2] = Plugins;
        }
        
        static void ConstructAccountKey()
        {
            FieldInfo Login = new FieldInfo();
            Login.name = "Login";
            Login.offset = 0;
            Login.type = FieldType.UString;
            Login.optional = false;
            Login.repeatable = false;
            
            FieldInfo Server = new FieldInfo();
            Server.name = "Server";
            Server.offset = 8;
            Server.type = FieldType.UString;
            Server.optional = false;
            Server.repeatable = false;
            
            AccountKey = new GroupInfo();
            AccountKey.name = "AccountKey";
            AccountKey.minSize = 16;
            AccountKey.fields = new FieldInfo[2];
            AccountKey.fields[0] = Login;
            AccountKey.fields[1] = Server;
        }
        
        static void ConstructPluginPermissions()
        {
            FieldInfo TradeAllowed = new FieldInfo();
            TradeAllowed.name = "TradeAllowed";
            TradeAllowed.offset = 0;
            TradeAllowed.type = FieldType.Bool;
            TradeAllowed.optional = false;
            TradeAllowed.repeatable = false;
            
            PluginPermissions = new GroupInfo();
            PluginPermissions.name = "PluginPermissions";
            PluginPermissions.minSize = 1;
            PluginPermissions.fields = new FieldInfo[1];
            PluginPermissions.fields[0] = TradeAllowed;
        }
        
        static void ConstructBotState()
        {
            EnumMemberInfo Offline = new EnumMemberInfo();
            Offline.name = "Offline";
            Offline.value = 0;
            
            EnumMemberInfo Starting = new EnumMemberInfo();
            Starting.name = "Starting";
            Starting.value = 1;
            
            EnumMemberInfo Faulted = new EnumMemberInfo();
            Faulted.name = "Faulted";
            Faulted.value = 2;
            
            EnumMemberInfo Online = new EnumMemberInfo();
            Online.name = "Online";
            Online.value = 3;
            
            EnumMemberInfo Stopping = new EnumMemberInfo();
            Stopping.name = "Stopping";
            Stopping.value = 4;
            
            EnumMemberInfo Broken = new EnumMemberInfo();
            Broken.name = "Broken";
            Broken.value = 5;
            
            EnumMemberInfo Reconnecting = new EnumMemberInfo();
            Reconnecting.name = "Reconnecting";
            Reconnecting.value = 6;
            
            BotState = new EnumInfo();
            BotState.name = "BotState";
            BotState.minSize = 4;
            BotState.members = new EnumMemberInfo[7];
            BotState.members[0] = Offline;
            BotState.members[1] = Starting;
            BotState.members[2] = Faulted;
            BotState.members[3] = Online;
            BotState.members[4] = Stopping;
            BotState.members[5] = Broken;
            BotState.members[6] = Reconnecting;
        }
        
        static void ConstructBotModel()
        {
            FieldInfo InstanceId = new FieldInfo();
            InstanceId.name = "InstanceId";
            InstanceId.offset = 0;
            InstanceId.type = FieldType.UString;
            InstanceId.optional = false;
            InstanceId.repeatable = false;
            
            FieldInfo Isolated = new FieldInfo();
            Isolated.name = "Isolated";
            Isolated.offset = 8;
            Isolated.type = FieldType.Bool;
            Isolated.optional = false;
            Isolated.repeatable = false;
            
            FieldInfo State = new FieldInfo();
            State.name = "State";
            State.offset = 9;
            State.type = FieldType.Enum;
            State.enumInfo = Info.BotState;
            State.optional = false;
            State.repeatable = false;
            
            FieldInfo Permissions = new FieldInfo();
            Permissions.name = "Permissions";
            Permissions.offset = 13;
            Permissions.type = FieldType.Group;
            Permissions.groupInfo = Info.PluginPermissions;
            Permissions.optional = false;
            Permissions.repeatable = false;
            
            FieldInfo Account = new FieldInfo();
            Account.name = "Account";
            Account.offset = 14;
            Account.type = FieldType.Group;
            Account.groupInfo = Info.AccountKey;
            Account.optional = false;
            Account.repeatable = false;
            
            FieldInfo Plugin = new FieldInfo();
            Plugin.name = "Plugin";
            Plugin.offset = 30;
            Plugin.type = FieldType.Group;
            Plugin.groupInfo = Info.PluginKey;
            Plugin.optional = false;
            Plugin.repeatable = false;
            
            BotModel = new GroupInfo();
            BotModel.name = "BotModel";
            BotModel.minSize = 46;
            BotModel.fields = new FieldInfo[6];
            BotModel.fields[0] = InstanceId;
            BotModel.fields[1] = Isolated;
            BotModel.fields[2] = State;
            BotModel.fields[3] = Permissions;
            BotModel.fields[4] = Account;
            BotModel.fields[5] = Plugin;
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
            AccountListRequest.id = 8;
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
            AccountListReport.id = 9;
            AccountListReport.minSize = 24;
            AccountListReport.fields = new FieldInfo[2];
            AccountListReport.fields[0] = RequestId;
            AccountListReport.fields[1] = Accounts;
        }
        
        static void ConstructPackageListRequest()
        {
            FieldInfo Id = new FieldInfo();
            Id.name = "Id";
            Id.offset = 8;
            Id.type = FieldType.String;
            Id.optional = false;
            Id.repeatable = false;
            
            PackageListRequest = new MessageInfo();
            PackageListRequest.parentInfo = Request;
            PackageListRequest.name = "PackageListRequest";
            PackageListRequest.id = 10;
            PackageListRequest.minSize = 16;
            PackageListRequest.fields = new FieldInfo[1];
            PackageListRequest.fields[0] = Id;
        }
        
        static void ConstructPackageListReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.String;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            FieldInfo Packages = new FieldInfo();
            Packages.name = "Packages";
            Packages.offset = 16;
            Packages.type = FieldType.Group;
            Packages.groupInfo = Info.PackageModel;
            Packages.optional = false;
            Packages.repeatable = true;
            
            PackageListReport = new MessageInfo();
            PackageListReport.parentInfo = Report;
            PackageListReport.name = "PackageListReport";
            PackageListReport.id = 11;
            PackageListReport.minSize = 24;
            PackageListReport.fields = new FieldInfo[2];
            PackageListReport.fields[0] = RequestId;
            PackageListReport.fields[1] = Packages;
        }
        
        static void ConstructSubscribeRequest()
        {
            FieldInfo Id = new FieldInfo();
            Id.name = "Id";
            Id.offset = 8;
            Id.type = FieldType.String;
            Id.optional = false;
            Id.repeatable = false;
            
            SubscribeRequest = new MessageInfo();
            SubscribeRequest.parentInfo = Request;
            SubscribeRequest.name = "SubscribeRequest";
            SubscribeRequest.id = 12;
            SubscribeRequest.minSize = 16;
            SubscribeRequest.fields = new FieldInfo[1];
            SubscribeRequest.fields[0] = Id;
        }
        
        static void ConstructSubscribeReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.name = "RequestId";
            RequestId.offset = 8;
            RequestId.type = FieldType.String;
            RequestId.optional = false;
            RequestId.repeatable = false;
            
            SubscribeReport = new MessageInfo();
            SubscribeReport.parentInfo = Report;
            SubscribeReport.name = "SubscribeReport";
            SubscribeReport.id = 13;
            SubscribeReport.minSize = 16;
            SubscribeReport.fields = new FieldInfo[1];
            SubscribeReport.fields[0] = RequestId;
        }
        
        static void ConstructAccountModelUpdate()
        {
            FieldInfo Id = new FieldInfo();
            Id.name = "Id";
            Id.offset = 8;
            Id.type = FieldType.String;
            Id.optional = false;
            Id.repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.name = "Type";
            Type.offset = 16;
            Type.type = FieldType.Enum;
            Type.enumInfo = Info.UpdateType;
            Type.optional = false;
            Type.repeatable = false;
            
            FieldInfo OldItem = new FieldInfo();
            OldItem.name = "OldItem";
            OldItem.offset = 20;
            OldItem.type = FieldType.Group;
            OldItem.groupInfo = Info.AccountModel;
            OldItem.optional = true;
            OldItem.repeatable = false;
            
            FieldInfo NewItem = new FieldInfo();
            NewItem.name = "NewItem";
            NewItem.offset = 44;
            NewItem.type = FieldType.Group;
            NewItem.groupInfo = Info.AccountModel;
            NewItem.optional = true;
            NewItem.repeatable = false;
            
            AccountModelUpdate = new MessageInfo();
            AccountModelUpdate.parentInfo = Update;
            AccountModelUpdate.name = "AccountModelUpdate";
            AccountModelUpdate.id = 14;
            AccountModelUpdate.minSize = 68;
            AccountModelUpdate.fields = new FieldInfo[4];
            AccountModelUpdate.fields[0] = Id;
            AccountModelUpdate.fields[1] = Type;
            AccountModelUpdate.fields[2] = OldItem;
            AccountModelUpdate.fields[3] = NewItem;
        }
        
        static void ConstructBotModelUpdate()
        {
            FieldInfo Id = new FieldInfo();
            Id.name = "Id";
            Id.offset = 8;
            Id.type = FieldType.String;
            Id.optional = false;
            Id.repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.name = "Type";
            Type.offset = 16;
            Type.type = FieldType.Enum;
            Type.enumInfo = Info.UpdateType;
            Type.optional = false;
            Type.repeatable = false;
            
            FieldInfo OldItem = new FieldInfo();
            OldItem.name = "OldItem";
            OldItem.offset = 20;
            OldItem.type = FieldType.Group;
            OldItem.groupInfo = Info.BotModel;
            OldItem.optional = true;
            OldItem.repeatable = false;
            
            FieldInfo NewItem = new FieldInfo();
            NewItem.name = "NewItem";
            NewItem.offset = 66;
            NewItem.type = FieldType.Group;
            NewItem.groupInfo = Info.BotModel;
            NewItem.optional = true;
            NewItem.repeatable = false;
            
            BotModelUpdate = new MessageInfo();
            BotModelUpdate.parentInfo = Update;
            BotModelUpdate.name = "BotModelUpdate";
            BotModelUpdate.id = 15;
            BotModelUpdate.minSize = 112;
            BotModelUpdate.fields = new FieldInfo[4];
            BotModelUpdate.fields[0] = Id;
            BotModelUpdate.fields[1] = Type;
            BotModelUpdate.fields[2] = OldItem;
            BotModelUpdate.fields[3] = NewItem;
        }
        
        static void ConstructPackageModelUpdate()
        {
            FieldInfo Id = new FieldInfo();
            Id.name = "Id";
            Id.offset = 8;
            Id.type = FieldType.String;
            Id.optional = false;
            Id.repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.name = "Type";
            Type.offset = 16;
            Type.type = FieldType.Enum;
            Type.enumInfo = Info.UpdateType;
            Type.optional = false;
            Type.repeatable = false;
            
            FieldInfo OldItem = new FieldInfo();
            OldItem.name = "OldItem";
            OldItem.offset = 20;
            OldItem.type = FieldType.Group;
            OldItem.groupInfo = Info.PackageModel;
            OldItem.optional = true;
            OldItem.repeatable = false;
            
            FieldInfo NewItem = new FieldInfo();
            NewItem.name = "NewItem";
            NewItem.offset = 44;
            NewItem.type = FieldType.Group;
            NewItem.groupInfo = Info.PackageModel;
            NewItem.optional = true;
            NewItem.repeatable = false;
            
            PackageModelUpdate = new MessageInfo();
            PackageModelUpdate.parentInfo = Update;
            PackageModelUpdate.name = "PackageModelUpdate";
            PackageModelUpdate.id = 16;
            PackageModelUpdate.minSize = 68;
            PackageModelUpdate.fields = new FieldInfo[4];
            PackageModelUpdate.fields[0] = Id;
            PackageModelUpdate.fields[1] = Type;
            PackageModelUpdate.fields[2] = OldItem;
            PackageModelUpdate.fields[3] = NewItem;
        }
        
        
        
        
        
        
        
        static void ConstructBotAgent()
        {
            BotAgent = new ProtocolInfo();
            BotAgent.name = "BotAgent";
            BotAgent.majorVersion = 1;
            BotAgent.minorVersion = 0;
            BotAgent.AddMessageInfo(LoginRequest);
            BotAgent.AddMessageInfo(LoginReport);
            BotAgent.AddMessageInfo(LoginReject);
            BotAgent.AddMessageInfo(LogoutRequest);
            BotAgent.AddMessageInfo(LogoutReport);
            BotAgent.AddMessageInfo(Request);
            BotAgent.AddMessageInfo(Report);
            BotAgent.AddMessageInfo(Update);
            BotAgent.AddMessageInfo(AccountListRequest);
            BotAgent.AddMessageInfo(AccountListReport);
            BotAgent.AddMessageInfo(PackageListRequest);
            BotAgent.AddMessageInfo(PackageListReport);
            BotAgent.AddMessageInfo(SubscribeRequest);
            BotAgent.AddMessageInfo(SubscribeReport);
            BotAgent.AddMessageInfo(AccountModelUpdate);
            BotAgent.AddMessageInfo(BotModelUpdate);
            BotAgent.AddMessageInfo(PackageModelUpdate);
        }
        
        public static MessageInfo LoginRequest;
        public static MessageInfo LoginReport;
        public static EnumInfo LoginRejectReason;
        public static MessageInfo LoginReject;
        public static MessageInfo LogoutRequest;
        public static EnumInfo LogoutReason;
        public static MessageInfo LogoutReport;
        public static MessageInfo Request;
        public static MessageInfo Report;
        public static EnumInfo UpdateType;
        public static MessageInfo Update;
        public static GroupInfo PluginKey;
        public static EnumInfo PluginType;
        public static GroupInfo PluginDescriptor;
        public static GroupInfo PluginInfo;
        public static GroupInfo PackageModel;
        public static GroupInfo AccountKey;
        public static GroupInfo PluginPermissions;
        public static EnumInfo BotState;
        public static GroupInfo BotModel;
        public static GroupInfo AccountModel;
        public static MessageInfo AccountListRequest;
        public static MessageInfo AccountListReport;
        public static MessageInfo PackageListRequest;
        public static MessageInfo PackageListReport;
        public static MessageInfo SubscribeRequest;
        public static MessageInfo SubscribeReport;
        public static MessageInfo AccountModelUpdate;
        public static MessageInfo BotModelUpdate;
        public static MessageInfo PackageModelUpdate;
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
    
    class LoginRequestClientContext : ClientContext
    {
        public LoginRequestClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class LogoutRequestClientContext : ClientContext
    {
        public LogoutRequestClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class AccountListRequestClientContext : ClientContext
    {
        public AccountListRequestClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class PackageListRequestClientContext : ClientContext
    {
        public PackageListRequestClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class SubscribeRequestClientContext : ClientContext
    {
        public SubscribeRequestClientContext(bool waitable) : base(waitable)
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
            coreSessionListener_ = new CoreClientSessionListener(this);
            coreSession_.Listener = coreSessionListener_;
            
            stateMutex_ = new object();
            connected_ = false;
            
            Event_LoginReport_178_13_LoginReport_ = new Event_LoginReport_178_13_LoginReport(this);
            Event_LoginReject_178_13_LoginReject_ = new Event_LoginReject_178_13_LoginReject(this);
            Event_LogoutReport_187_9_LogoutReport_ = new Event_LogoutReport_187_9_LogoutReport(this);
            Event_LogoutReport_205_13_LogoutReport_ = new Event_LogoutReport_205_13_LogoutReport(this);
            Event_AccountModelUpdate_271_9_AccountModelUpdate_ = new Event_AccountModelUpdate_271_9_AccountModelUpdate(this);
            Event_BotModelUpdate_271_9_BotModelUpdate_ = new Event_BotModelUpdate_271_9_BotModelUpdate(this);
            Event_PackageModelUpdate_271_9_PackageModelUpdate_ = new Event_PackageModelUpdate_271_9_PackageModelUpdate(this);
            Event_AccountListReport_306_13_AccountListReport_ = new Event_AccountListReport_306_13_AccountListReport(this);
            Event_PackageListReport_312_13_PackageListReport_ = new Event_PackageListReport_312_13_PackageListReport(this);
            Event_SubscribeReport_318_13_SubscribeReport_ = new Event_SubscribeReport_318_13_SubscribeReport(this);
            
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
        
        public void SendLoginRequest(LoginRequestClientContext context, LoginRequest message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("SendLoginRequest({0})", message.ToString());
                
                ClientProcessor_.PreprocessSendLoginRequest(context, message);
                
                if (context != null)
                    context.Reset();
                
                coreSession_.Send(message);
                
                Client:
                
                ClientProcessor_.PostprocessSendLoginRequest(context, message);
                
                if (ClientProcessor_.Completed)
                    coreSession_.Disconnect("Client disconnect");
            }
        }
        
        public void SendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("SendLogoutRequest({0})", message.ToString());
                
                ClientProcessor_.PreprocessSendLogoutRequest(context, message);
                
                if (context != null)
                    context.Reset();
                
                coreSession_.Send(message);
                
                Client:
                
                ClientProcessor_.PostprocessSendLogoutRequest(context, message);
                
                if (ClientProcessor_.Completed)
                    coreSession_.Disconnect("Client disconnect");
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
                
                ClientProcessor_.PreprocessSend(message);
                
                string key = message.Id;
                ClientRequestProcessor ClientRequestProcessor;
                
                if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                {
                    ClientRequestProcessor = new ClientRequestProcessor(this, key);
                    ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                }
                
                ClientRequestProcessor.PreprocessSendAccountListRequest(context, message);
                
                if (context != null)
                    context.Reset();
                
                coreSession_.Send(message);
                
                ClientRequest:
                
                ClientRequestProcessor.PostprocessSendAccountListRequest(context, message);
                
                if (ClientRequestProcessor.Completed)
                    ClientRequestProcessorDictionary_.Remove(key);
                
                Client:
                
                ClientProcessor_.PostprocessSend(message);
                
                if (ClientProcessor_.Completed)
                    coreSession_.Disconnect("Client disconnect");
            }
        }
        
        public void SendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("SendPackageListRequest({0})", message.ToString());
                
                ClientProcessor_.PreprocessSend(message);
                
                string key = message.Id;
                ClientRequestProcessor ClientRequestProcessor;
                
                if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                {
                    ClientRequestProcessor = new ClientRequestProcessor(this, key);
                    ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                }
                
                ClientRequestProcessor.PreprocessSendPackageListRequest(context, message);
                
                if (context != null)
                    context.Reset();
                
                coreSession_.Send(message);
                
                ClientRequest:
                
                ClientRequestProcessor.PostprocessSendPackageListRequest(context, message);
                
                if (ClientRequestProcessor.Completed)
                    ClientRequestProcessorDictionary_.Remove(key);
                
                Client:
                
                ClientProcessor_.PostprocessSend(message);
                
                if (ClientProcessor_.Completed)
                    coreSession_.Disconnect("Client disconnect");
            }
        }
        
        public void SendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("SendSubscribeRequest({0})", message.ToString());
                
                ClientProcessor_.PreprocessSend(message);
                
                string key = message.Id;
                ClientRequestProcessor ClientRequestProcessor;
                
                if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                {
                    ClientRequestProcessor = new ClientRequestProcessor(this, key);
                    ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                }
                
                ClientRequestProcessor.PreprocessSendSubscribeRequest(context, message);
                
                if (context != null)
                    context.Reset();
                
                coreSession_.Send(message);
                
                ClientRequest:
                
                ClientRequestProcessor.PostprocessSendSubscribeRequest(context, message);
                
                if (ClientRequestProcessor.Completed)
                    ClientRequestProcessorDictionary_.Remove(key);
                
                Client:
                
                ClientProcessor_.PostprocessSend(message);
                
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
                
                if (Is.Update(message))
                {
                    Update Update = Cast.Update(message);
                    
                    string key = Update.Id;
                    ClientUpdateProcessor ClientUpdateProcessor;
                    
                    if (! ClientUpdateProcessorDictionary_.TryGetValue(key, out ClientUpdateProcessor))
                    {
                        ClientUpdateProcessor = new ClientUpdateProcessor(this, key);
                        ClientUpdateProcessorDictionary_.Add(key, ClientUpdateProcessor);
                    }
                    
                    ClientUpdateProcessor.PreprocessSend(message);
                    
                    coreSession_.Send(message);
                    
                    ClientUpdate:
                    
                    ClientUpdateProcessor.PostprocessSend(message);
                    
                    if (ClientUpdateProcessor.Completed)
                        ClientUpdateProcessorDictionary_.Remove(key);
                    
                    goto Client;
                }
                
                if (Is.Request(message))
                {
                    Request Request = Cast.Request(message);
                    
                    string key = Request.Id;
                    ClientRequestProcessor ClientRequestProcessor;
                    
                    if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                    {
                        ClientRequestProcessor = new ClientRequestProcessor(this, key);
                        ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                    }
                    
                    ClientRequestProcessor.PreprocessSend(message);
                    
                    coreSession_.Send(message);
                    
                    ClientRequest:
                    
                    ClientRequestProcessor.PostprocessSend(message);
                    
                    if (ClientRequestProcessor.Completed)
                        ClientRequestProcessorDictionary_.Remove(key);
                    
                    goto Client;
                }
                
                if (Is.Report(message))
                {
                    Report Report = Cast.Report(message);
                    
                    string key = Report.RequestId;
                    ClientRequestProcessor ClientRequestProcessor;
                    
                    if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                    {
                        ClientRequestProcessor = new ClientRequestProcessor(this, key);
                        ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                    }
                    
                    ClientRequestProcessor.PreprocessSend(message);
                    
                    coreSession_.Send(message);
                    
                    ClientRequest:
                    
                    ClientRequestProcessor.PostprocessSend(message);
                    
                    if (ClientRequestProcessor.Completed)
                        ClientRequestProcessorDictionary_.Remove(key);
                    
                    goto Client;
                }
                
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
                
                State_176_9_ = new State_176_9(this);
                State_178_13_ = new State_178_13(this);
                State_187_9_ = new State_187_9(this);
                State_205_13_ = new State_205_13(this);
                State_0_ = new State_0(this);
                
                state_ = State_176_9_;
                
                if (session_.coreSession_.LogStates)
                    session_.coreSession_.LogState("Client : 176_9");
            }
            
            public bool Completed
            {
                get { return state_ == State_0_; }
            }
            
            public void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
            {
                state_.PreprocessSendLoginRequest(context, message);
            }
            
            public void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
            {
                state_.PostprocessSendLoginRequest(context, message);
            }
            
            public void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
            {
                state_.PreprocessSendLogoutRequest(context, message);
            }
            
            public void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
            {
                state_.PostprocessSendLogoutRequest(context, message);
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
                
                public abstract void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message);
                
                public abstract void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message);
                
                public abstract void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message);
                
                public abstract void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message);
                
                public abstract void PreprocessSend(Message message);
                
                public abstract void PostprocessSend(Message message);
                
                public abstract void ProcessReceive(Message message);
                
                public abstract void ProcessDisconnect(List<ClientContext> contextList);
                
                protected ClientProcessor processor_;
            }
            
            class State_176_9 : State
            {
                public State_176_9(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    processor_.State_178_13_.LoginRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_178_13_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("Client : 178_13");
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 176_9 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    if (Is.LoginRequest(message))
                        return;
                    
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 176_9 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                    if (Is.LoginRequest(message))
                    {
                        processor_.state_ = processor_.State_178_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 178_13");
                        
                        return;
                    }
                }
                
                public override void ProcessReceive(Message message)
                {
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 176_9 : {0}", message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            class State_178_13 : State
            {
                public State_178_13(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 178_13 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 178_13 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 178_13 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.LoginReport(message))
                    {
                        LoginReport LoginReport = Cast.LoginReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_LoginReport_178_13_LoginReport_.LoginRequestClientContext_ = LoginRequestClientContext_;
                            processor_.session_.Event_LoginReport_178_13_LoginReport_.LoginReport_ = LoginReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_LoginReport_178_13_LoginReport_;
                        }
                        
                        LoginRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_187_9_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 187_9");
                        
                        return;
                    }
                    
                    if (Is.LoginReject(message))
                    {
                        LoginReject LoginReject = Cast.LoginReject(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_LoginReject_178_13_LoginReject_.LoginRequestClientContext_ = LoginRequestClientContext_;
                            processor_.session_.Event_LoginReject_178_13_LoginReject_.LoginReject_ = LoginReject;
                            
                            processor_.session_.event_ = processor_.session_.Event_LoginReject_178_13_LoginReject_;
                        }
                        
                        LoginRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 0");
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 178_13 : {0}", message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (LoginRequestClientContext_ != null)
                        contextList.Add(LoginRequestClientContext_);
                }
                
                public LoginRequestClientContext LoginRequestClientContext_;
            }
            
            class State_187_9 : State
            {
                public State_187_9(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 187_9 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    processor_.State_205_13_.LogoutRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_205_13_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("Client : 205_13");
                }
                
                public override void PreprocessSend(Message message)
                {
                    if (Is.Request(message))
                        return;
                    
                    if (Is.LogoutRequest(message))
                        return;
                    
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 187_9 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                    if (Is.Request(message))
                    {
                        processor_.state_ = processor_.State_187_9_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 187_9");
                        
                        return;
                    }
                    
                    if (Is.LogoutRequest(message))
                    {
                        processor_.state_ = processor_.State_205_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 205_13");
                        
                        return;
                    }
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.Report(message))
                    {
                        Report Report = Cast.Report(message);
                        
                        processor_.state_ = processor_.State_187_9_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 187_9");
                        
                        return;
                    }
                    
                    if (Is.Update(message))
                    {
                        Update Update = Cast.Update(message);
                        
                        processor_.state_ = processor_.State_187_9_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 187_9");
                        
                        return;
                    }
                    
                    if (Is.LogoutReport(message))
                    {
                        LogoutReport LogoutReport = Cast.LogoutReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_LogoutReport_187_9_LogoutReport_.LogoutReport_ = LogoutReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_LogoutReport_187_9_LogoutReport_;
                        }
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 0");
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 187_9 : {0}", message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            class State_205_13 : State
            {
                public State_205_13(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 205_13 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 205_13 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 205_13 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.Report(message))
                    {
                        Report Report = Cast.Report(message);
                        
                        processor_.state_ = processor_.State_205_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 205_13");
                        
                        return;
                    }
                    
                    if (Is.Update(message))
                    {
                        Update Update = Cast.Update(message);
                        
                        processor_.state_ = processor_.State_205_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 205_13");
                        
                        return;
                    }
                    
                    if (Is.LogoutReport(message))
                    {
                        LogoutReport LogoutReport = Cast.LogoutReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_LogoutReport_205_13_LogoutReport_.LogoutRequestClientContext_ = LogoutRequestClientContext_;
                            processor_.session_.Event_LogoutReport_205_13_LogoutReport_.LogoutReport_ = LogoutReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_LogoutReport_205_13_LogoutReport_;
                        }
                        
                        LogoutRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 0");
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 205_13 : {0}", message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (LogoutRequestClientContext_ != null)
                        contextList.Add(LogoutRequestClientContext_);
                }
                
                public LogoutRequestClientContext LogoutRequestClientContext_;
            }
            
            class State_0 : State
            {
                public State_0(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
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
            
            State_176_9 State_176_9_;
            State_178_13 State_178_13_;
            State_187_9 State_187_9_;
            State_205_13 State_205_13_;
            State_0 State_0_;
            
            State state_;
        }
        
        class ClientUpdateProcessor
        {
            public ClientUpdateProcessor(ClientSession session, string id)
            {
                session_ = session;
                id_ = id;
                
                State_271_9_ = new State_271_9(this);
                State_0_ = new State_0(this);
                
                state_ = State_271_9_;
                
                if (session_.coreSession_.LogStates)
                    session_.coreSession_.LogState("ClientUpdate({0}) : 271_9", id_);
            }
            
            public bool Completed
            {
                get { return state_ == State_0_; }
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
                public State(ClientUpdateProcessor processor)
                {
                    processor_ = processor;
                }
                
                public abstract void PreprocessSend(Message message);
                
                public abstract void PostprocessSend(Message message);
                
                public abstract void ProcessReceive(Message message);
                
                public abstract void ProcessDisconnect(List<ClientContext> contextList);
                
                protected ClientUpdateProcessor processor_;
            }
            
            class State_271_9 : State
            {
                public State_271_9(ClientUpdateProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientUpdate({2}) : 271_9 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.AccountModelUpdate(message))
                    {
                        AccountModelUpdate AccountModelUpdate = Cast.AccountModelUpdate(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_AccountModelUpdate_271_9_AccountModelUpdate_.AccountModelUpdate_ = AccountModelUpdate;
                            
                            processor_.session_.event_ = processor_.session_.Event_AccountModelUpdate_271_9_AccountModelUpdate_;
                        }
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientUpdate({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.BotModelUpdate(message))
                    {
                        BotModelUpdate BotModelUpdate = Cast.BotModelUpdate(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_BotModelUpdate_271_9_BotModelUpdate_.BotModelUpdate_ = BotModelUpdate;
                            
                            processor_.session_.event_ = processor_.session_.Event_BotModelUpdate_271_9_BotModelUpdate_;
                        }
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientUpdate({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.PackageModelUpdate(message))
                    {
                        PackageModelUpdate PackageModelUpdate = Cast.PackageModelUpdate(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_PackageModelUpdate_271_9_PackageModelUpdate_.PackageModelUpdate_ = PackageModelUpdate;
                            
                            processor_.session_.event_ = processor_.session_.Event_PackageModelUpdate_271_9_PackageModelUpdate_;
                        }
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientUpdate({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientUpdate({0}) : 271_9 : {1}", processor_.id_, message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            class State_0 : State
            {
                public State_0(ClientUpdateProcessor processor) : base(processor)
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
            string id_;
            
            State_271_9 State_271_9_;
            State_0 State_0_;
            
            State state_;
        }
        
        class ClientRequestProcessor
        {
            public ClientRequestProcessor(ClientSession session, string id)
            {
                session_ = session;
                id_ = id;
                
                State_304_9_ = new State_304_9(this);
                State_306_13_ = new State_306_13(this);
                State_312_13_ = new State_312_13(this);
                State_318_13_ = new State_318_13(this);
                State_0_ = new State_0(this);
                
                state_ = State_304_9_;
                
                if (session_.coreSession_.LogStates)
                    session_.coreSession_.LogState("ClientRequest({0}) : 304_9", id_);
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
            
            public void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
            {
                state_.PreprocessSendPackageListRequest(context, message);
            }
            
            public void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
            {
                state_.PostprocessSendPackageListRequest(context, message);
            }
            
            public void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
            {
                state_.PreprocessSendSubscribeRequest(context, message);
            }
            
            public void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
            {
                state_.PostprocessSendSubscribeRequest(context, message);
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
                public State(ClientRequestProcessor processor)
                {
                    processor_ = processor;
                }
                
                public abstract void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message);
                
                public abstract void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message);
                
                public abstract void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message);
                
                public abstract void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message);
                
                public abstract void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message);
                
                public abstract void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message);
                
                public abstract void PreprocessSend(Message message);
                
                public abstract void PostprocessSend(Message message);
                
                public abstract void ProcessReceive(Message message);
                
                public abstract void ProcessDisconnect(List<ClientContext> contextList);
                
                protected ClientRequestProcessor processor_;
            }
            
            class State_304_9 : State
            {
                public State_304_9(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    processor_.State_306_13_.AccountListRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_306_13_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("ClientRequest({0}) : 306_13", processor_.id_);
                }
                
                public override void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                }
                
                public override void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                    processor_.State_312_13_.PackageListRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_312_13_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("ClientRequest({0}) : 312_13", processor_.id_);
                }
                
                public override void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                }
                
                public override void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                    processor_.State_318_13_.SubscribeRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_318_13_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("ClientRequest({0}) : 318_13", processor_.id_);
                }
                
                public override void PreprocessSend(Message message)
                {
                    if (Is.AccountListRequest(message))
                        return;
                    
                    if (Is.PackageListRequest(message))
                        return;
                    
                    if (Is.SubscribeRequest(message))
                        return;
                    
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 304_9 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                    if (Is.AccountListRequest(message))
                    {
                        processor_.state_ = processor_.State_306_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 306_13", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.PackageListRequest(message))
                    {
                        processor_.state_ = processor_.State_312_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 312_13", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.SubscribeRequest(message))
                    {
                        processor_.state_ = processor_.State_318_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 318_13", processor_.id_);
                        
                        return;
                    }
                }
                
                public override void ProcessReceive(Message message)
                {
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientRequest({0}) : 304_9 : {1}", processor_.id_, message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            class State_306_13 : State
            {
                public State_306_13(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 306_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 306_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                }
                
                public override void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 306_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 306_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
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
                            processor_.session_.Event_AccountListReport_306_13_AccountListReport_.AccountListRequestClientContext_ = AccountListRequestClientContext_;
                            processor_.session_.Event_AccountListReport_306_13_AccountListReport_.AccountListReport_ = AccountListReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_AccountListReport_306_13_AccountListReport_;
                        }
                        
                        AccountListRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientRequest({0}) : 306_13 : {1}", processor_.id_, message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (AccountListRequestClientContext_ != null)
                        contextList.Add(AccountListRequestClientContext_);
                }
                
                public AccountListRequestClientContext AccountListRequestClientContext_;
            }
            
            class State_312_13 : State
            {
                public State_312_13(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 312_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 312_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                }
                
                public override void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 312_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 312_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.PackageListReport(message))
                    {
                        PackageListReport PackageListReport = Cast.PackageListReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_PackageListReport_312_13_PackageListReport_.PackageListRequestClientContext_ = PackageListRequestClientContext_;
                            processor_.session_.Event_PackageListReport_312_13_PackageListReport_.PackageListReport_ = PackageListReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_PackageListReport_312_13_PackageListReport_;
                        }
                        
                        PackageListRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientRequest({0}) : 312_13 : {1}", processor_.id_, message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (PackageListRequestClientContext_ != null)
                        contextList.Add(PackageListRequestClientContext_);
                }
                
                public PackageListRequestClientContext PackageListRequestClientContext_;
            }
            
            class State_318_13 : State
            {
                public State_318_13(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 318_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 318_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                }
                
                public override void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 318_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 318_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.SubscribeReport(message))
                    {
                        SubscribeReport SubscribeReport = Cast.SubscribeReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_SubscribeReport_318_13_SubscribeReport_.SubscribeRequestClientContext_ = SubscribeRequestClientContext_;
                            processor_.session_.Event_SubscribeReport_318_13_SubscribeReport_.SubscribeReport_ = SubscribeReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_SubscribeReport_318_13_SubscribeReport_;
                        }
                        
                        SubscribeRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientRequest({0}) : 318_13 : {1}", processor_.id_, message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (SubscribeRequestClientContext_ != null)
                        contextList.Add(SubscribeRequestClientContext_);
                }
                
                public SubscribeRequestClientContext SubscribeRequestClientContext_;
            }
            
            class State_0 : State
            {
                public State_0(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                }
                
                public override void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                }
                
                public override void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                }
                
                public override void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
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
            string id_;
            
            State_304_9 State_304_9_;
            State_306_13 State_306_13_;
            State_312_13 State_312_13_;
            State_318_13 State_318_13_;
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
        
        class Event_LoginReport_178_13_LoginReport : Event
        {
            public Event_LoginReport_178_13_LoginReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnLoginReport({0})", LoginReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnLoginReport(session_, LoginRequestClientContext_, LoginReport_);
                    }
                    catch
                    {
                    }
                }
                
                if (LoginRequestClientContext_ != null)
                    LoginRequestClientContext_.SetCompleted();
                
                LoginReport_ = new LoginReport();
                
                LoginRequestClientContext_ = null;
            }
            
            public LoginRequestClientContext LoginRequestClientContext_;
            
            public LoginReport LoginReport_;
        }
        
        class Event_LoginReject_178_13_LoginReject : Event
        {
            public Event_LoginReject_178_13_LoginReject(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnLoginReject({0})", LoginReject_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnLoginReject(session_, LoginRequestClientContext_, LoginReject_);
                    }
                    catch
                    {
                    }
                }
                
                if (LoginRequestClientContext_ != null)
                    LoginRequestClientContext_.SetCompleted();
                
                LoginReject_ = new LoginReject();
                
                LoginRequestClientContext_ = null;
            }
            
            public LoginRequestClientContext LoginRequestClientContext_;
            
            public LoginReject LoginReject_;
        }
        
        class Event_LogoutReport_187_9_LogoutReport : Event
        {
            public Event_LogoutReport_187_9_LogoutReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnLogoutReport({0})", LogoutReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnLogoutReport(session_, LogoutReport_);
                    }
                    catch
                    {
                    }
                }
                
                LogoutReport_ = new LogoutReport();
            }
            
            public LogoutReport LogoutReport_;
        }
        
        class Event_LogoutReport_205_13_LogoutReport : Event
        {
            public Event_LogoutReport_205_13_LogoutReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnLogoutReport({0})", LogoutReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnLogoutReport(session_, LogoutRequestClientContext_, LogoutReport_);
                    }
                    catch
                    {
                    }
                }
                
                if (LogoutRequestClientContext_ != null)
                    LogoutRequestClientContext_.SetCompleted();
                
                LogoutReport_ = new LogoutReport();
                
                LogoutRequestClientContext_ = null;
            }
            
            public LogoutRequestClientContext LogoutRequestClientContext_;
            
            public LogoutReport LogoutReport_;
        }
        
        class Event_AccountModelUpdate_271_9_AccountModelUpdate : Event
        {
            public Event_AccountModelUpdate_271_9_AccountModelUpdate(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnAccountModelUpdate({0})", AccountModelUpdate_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnAccountModelUpdate(session_, AccountModelUpdate_);
                    }
                    catch
                    {
                    }
                }
                
                AccountModelUpdate_ = new AccountModelUpdate();
            }
            
            public AccountModelUpdate AccountModelUpdate_;
        }
        
        class Event_BotModelUpdate_271_9_BotModelUpdate : Event
        {
            public Event_BotModelUpdate_271_9_BotModelUpdate(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnBotModelUpdate({0})", BotModelUpdate_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnBotModelUpdate(session_, BotModelUpdate_);
                    }
                    catch
                    {
                    }
                }
                
                BotModelUpdate_ = new BotModelUpdate();
            }
            
            public BotModelUpdate BotModelUpdate_;
        }
        
        class Event_PackageModelUpdate_271_9_PackageModelUpdate : Event
        {
            public Event_PackageModelUpdate_271_9_PackageModelUpdate(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnPackageModelUpdate({0})", PackageModelUpdate_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnPackageModelUpdate(session_, PackageModelUpdate_);
                    }
                    catch
                    {
                    }
                }
                
                PackageModelUpdate_ = new PackageModelUpdate();
            }
            
            public PackageModelUpdate PackageModelUpdate_;
        }
        
        class Event_AccountListReport_306_13_AccountListReport : Event
        {
            public Event_AccountListReport_306_13_AccountListReport(ClientSession clientSession) : base(clientSession)
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
        
        class Event_PackageListReport_312_13_PackageListReport : Event
        {
            public Event_PackageListReport_312_13_PackageListReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnPackageListReport({0})", PackageListReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnPackageListReport(session_, PackageListRequestClientContext_, PackageListReport_);
                    }
                    catch
                    {
                    }
                }
                
                if (PackageListRequestClientContext_ != null)
                    PackageListRequestClientContext_.SetCompleted();
                
                PackageListReport_ = new PackageListReport();
                
                PackageListRequestClientContext_ = null;
            }
            
            public PackageListRequestClientContext PackageListRequestClientContext_;
            
            public PackageListReport PackageListReport_;
        }
        
        class Event_SubscribeReport_318_13_SubscribeReport : Event
        {
            public Event_SubscribeReport_318_13_SubscribeReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnSubscribeReport({0})", SubscribeReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnSubscribeReport(session_, SubscribeRequestClientContext_, SubscribeReport_);
                    }
                    catch
                    {
                    }
                }
                
                if (SubscribeRequestClientContext_ != null)
                    SubscribeRequestClientContext_.SetCompleted();
                
                SubscribeReport_ = new SubscribeReport();
                
                SubscribeRequestClientContext_ = null;
            }
            
            public SubscribeRequestClientContext SubscribeRequestClientContext_;
            
            public SubscribeReport SubscribeReport_;
        }
        
        class CoreClientSessionListener : Core.ClientSessionListener
        {
            public CoreClientSessionListener(ClientSession session)
            {
                session_ = session;
            }
            
            public override void OnConnect(Core.ClientSession clientSession)
            {
                session_.OnCoreConnect();
            }
            
            public override void OnConnectError(Core.ClientSession clientSession, string text)
            {
                session_.OnCoreConnectError(text);
            }
            
            public override void OnDisconnect(Core.ClientSession clientSession, string text)
            {
                session_.OnCoreDisconnect(text);
            }
            
            public override void OnReceive(Core.ClientSession clientSession, Message message)
            {
                session_.OnCoreReceive(message);
            }
            
            public override void OnSend(Core.ClientSession clientSession)
            {
                session_.OnCoreSend();
            }
            
            ClientSession session_;
        }
        
        void OnCoreConnect()
        {
            ConnectClientContext connectContext;
            
            lock (stateMutex_)
            {
                ClientProcessor_ = new ClientProcessor(this);
                
                ClientUpdateProcessorDictionary_ = new SortedDictionary<string, ClientUpdateProcessor>();
                
                ClientRequestProcessorDictionary_ = new SortedDictionary<string, ClientRequestProcessor>();
                
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
        
        void OnCoreConnectError(string text)
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
                    listener_.OnConnectError(this, connectContext, text);
                }
                catch
                {
                }
            }
            
            if (connectContext != null)
                connectContext.SetCompleted();
        }
        
        void OnCoreDisconnect(string text)
        {
            DisconnectClientContext disconnectContext;
            List<ClientContext> contexList = new List<ClientContext>();
            
            lock (stateMutex_)
            {
                disconnectContext = disconnectContext_;
                disconnectContext_ = null;
                
                foreach(var processor in ClientUpdateProcessorDictionary_)
                    processor.Value.ProcessDisconnect(contexList);
                
                ClientUpdateProcessorDictionary_ = null;
                
                foreach(var processor in ClientRequestProcessorDictionary_)
                    processor.Value.ProcessDisconnect(contexList);
                
                ClientRequestProcessorDictionary_ = null;
                
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
        
        void OnCoreReceive(Message message)
        {
            lock (stateMutex_)
            {
                ClientProcessor_.ProcessReceive(message);
                
                if (Is.Update(message))
                {
                    Update Update = Cast.Update(message);
                    
                    string key = Update.Id;
                    ClientUpdateProcessor ClientUpdateProcessor;
                    
                    if (! ClientUpdateProcessorDictionary_.TryGetValue(key, out ClientUpdateProcessor))
                    {
                        ClientUpdateProcessor = new ClientUpdateProcessor(this, key);
                        ClientUpdateProcessorDictionary_.Add(key, ClientUpdateProcessor);
                    }
                    
                    ClientUpdateProcessor.ProcessReceive(message);
                    
                    ClientUpdate:
                    
                    if (ClientUpdateProcessor.Completed)
                        ClientUpdateProcessorDictionary_.Remove(key);
                    
                    goto Client;
                }
                
                if (Is.Request(message))
                {
                    Request Request = Cast.Request(message);
                    
                    string key = Request.Id;
                    ClientRequestProcessor ClientRequestProcessor;
                    
                    if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                    {
                        ClientRequestProcessor = new ClientRequestProcessor(this, key);
                        ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                    }
                    
                    ClientRequestProcessor.ProcessReceive(message);
                    
                    ClientRequest:
                    
                    if (ClientRequestProcessor.Completed)
                        ClientRequestProcessorDictionary_.Remove(key);
                    
                    goto Client;
                }
                
                if (Is.Report(message))
                {
                    Report Report = Cast.Report(message);
                    
                    string key = Report.RequestId;
                    ClientRequestProcessor ClientRequestProcessor;
                    
                    if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                    {
                        ClientRequestProcessor = new ClientRequestProcessor(this, key);
                        ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                    }
                    
                    ClientRequestProcessor.ProcessReceive(message);
                    
                    ClientRequest:
                    
                    if (ClientRequestProcessor.Completed)
                        ClientRequestProcessorDictionary_.Remove(key);
                    
                    goto Client;
                }
                
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
        
        void OnCoreSend()
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
        CoreClientSessionListener coreSessionListener_;
        
        ClientSessionListener listener_;
        
        object stateMutex_;
        ConnectClientContext connectContext_;
        DisconnectClientContext disconnectContext_;
        bool connected_;
        
        ClientProcessor ClientProcessor_;
        SortedDictionary<string, ClientUpdateProcessor> ClientUpdateProcessorDictionary_;
        SortedDictionary<string, ClientRequestProcessor> ClientRequestProcessorDictionary_;
        
        Event_LoginReport_178_13_LoginReport Event_LoginReport_178_13_LoginReport_;
        Event_LoginReject_178_13_LoginReject Event_LoginReject_178_13_LoginReject_;
        Event_LogoutReport_187_9_LogoutReport Event_LogoutReport_187_9_LogoutReport_;
        Event_LogoutReport_205_13_LogoutReport Event_LogoutReport_205_13_LogoutReport_;
        Event_AccountModelUpdate_271_9_AccountModelUpdate Event_AccountModelUpdate_271_9_AccountModelUpdate_;
        Event_BotModelUpdate_271_9_BotModelUpdate Event_BotModelUpdate_271_9_BotModelUpdate_;
        Event_PackageModelUpdate_271_9_PackageModelUpdate Event_PackageModelUpdate_271_9_PackageModelUpdate_;
        Event_AccountListReport_306_13_AccountListReport Event_AccountListReport_306_13_AccountListReport_;
        Event_PackageListReport_312_13_PackageListReport Event_PackageListReport_312_13_PackageListReport_;
        Event_SubscribeReport_318_13_SubscribeReport Event_SubscribeReport_318_13_SubscribeReport_;
        
        Event event_;
    }
    
    class ClientSessionListener
    {
        public virtual void OnConnect(ClientSession clientSession, ConnectClientContext connectContext)
        {
        }
        
        public virtual void OnConnectError(ClientSession clientSession, ConnectClientContext connectContext, string text)
        {
        }
        
        public virtual void OnDisconnect(ClientSession clientSession, DisconnectClientContext disconnectContext, ClientContext[] contexts, string text)
        {
        }
        
        public virtual void OnLoginReport(ClientSession session, LoginRequestClientContext LoginRequestClientContext, LoginReport message)
        {
        }
        
        public virtual void OnLoginReject(ClientSession session, LoginRequestClientContext LoginRequestClientContext, LoginReject message)
        {
        }
        
        public virtual void OnLogoutReport(ClientSession session, LogoutReport message)
        {
        }
        
        public virtual void OnLogoutReport(ClientSession session, LogoutRequestClientContext LogoutRequestClientContext, LogoutReport message)
        {
        }
        
        public virtual void OnAccountModelUpdate(ClientSession session, AccountModelUpdate message)
        {
        }
        
        public virtual void OnBotModelUpdate(ClientSession session, BotModelUpdate message)
        {
        }
        
        public virtual void OnPackageModelUpdate(ClientSession session, PackageModelUpdate message)
        {
        }
        
        public virtual void OnAccountListReport(ClientSession session, AccountListRequestClientContext AccountListRequestClientContext, AccountListReport message)
        {
        }
        
        public virtual void OnPackageListReport(ClientSession session, PackageListRequestClientContext PackageListRequestClientContext, PackageListReport message)
        {
        }
        
        public virtual void OnSubscribeReport(ClientSession session, SubscribeRequestClientContext SubscribeRequestClientContext, SubscribeReport message)
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
            coreServerListener_ = new CoreServerListener(this);
            coreServer_.Listener = coreServerListener_;
            
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
                
                Event_LoginRequest_222_9_LoginRequest_ = new Event_LoginRequest_222_9_LoginRequest(this);
                Event_LogoutRequest_233_9_LogoutRequest_ = new Event_LogoutRequest_233_9_LogoutRequest(this);
                Event_AccountListRequest_330_9_AccountListRequest_ = new Event_AccountListRequest_330_9_AccountListRequest(this);
                Event_PackageListRequest_330_9_PackageListRequest_ = new Event_PackageListRequest_330_9_PackageListRequest(this);
                Event_SubscribeRequest_330_9_SubscribeRequest_ = new Event_SubscribeRequest_330_9_SubscribeRequest(this);
                
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
            
            public void Send(Message message)
            {
                lock (stateMutex_)
                {
                    if (ServerProcessor_ == null)
                        throw new Exception(string.Format("Session is not active : {0}({1})", server_.coreServer_.Name, coreSession_.Guid));
                    
                    if (coreSession_.LogEvents)
                        coreSession_.LogEvent("Send({0})", message.ToString());
                    
                    ServerProcessor_.PreprocessSend(message);
                    
                    if (Is.Update(message))
                    {
                        Update Update = Cast.Update(message);
                        
                        string key = Update.Id;
                        ServerUpdateProcessor ServerUpdateProcessor;
                        
                        if (! ServerUpdateProcessorDictionary_.TryGetValue(key, out ServerUpdateProcessor))
                        {
                            ServerUpdateProcessor = new ServerUpdateProcessor(this, key);
                            ServerUpdateProcessorDictionary_.Add(key, ServerUpdateProcessor);
                        }
                        
                        ServerUpdateProcessor.PreprocessSend(message);
                        
                        coreSession_.Send(message);
                        
                        ServerUpdate:
                        
                        ServerUpdateProcessor.PostprocessSend(message);
                        
                        if (ServerUpdateProcessor.Completed)
                            ServerUpdateProcessorDictionary_.Remove(key);
                        
                        goto Server;
                    }
                    
                    if (Is.Request(message))
                    {
                        Request Request = Cast.Request(message);
                        
                        string key = Request.Id;
                        ServerRequestProcessor ServerRequestProcessor;
                        
                        if (! ServerRequestProcessorDictionary_.TryGetValue(key, out ServerRequestProcessor))
                        {
                            ServerRequestProcessor = new ServerRequestProcessor(this, key);
                            ServerRequestProcessorDictionary_.Add(key, ServerRequestProcessor);
                        }
                        
                        ServerRequestProcessor.PreprocessSend(message);
                        
                        coreSession_.Send(message);
                        
                        ServerRequest:
                        
                        ServerRequestProcessor.PostprocessSend(message);
                        
                        if (ServerRequestProcessor.Completed)
                            ServerRequestProcessorDictionary_.Remove(key);
                        
                        goto Server;
                    }
                    
                    if (Is.Report(message))
                    {
                        Report Report = Cast.Report(message);
                        
                        string key = Report.RequestId;
                        ServerRequestProcessor ServerRequestProcessor;
                        
                        if (! ServerRequestProcessorDictionary_.TryGetValue(key, out ServerRequestProcessor))
                        {
                            ServerRequestProcessor = new ServerRequestProcessor(this, key);
                            ServerRequestProcessorDictionary_.Add(key, ServerRequestProcessor);
                        }
                        
                        ServerRequestProcessor.PreprocessSend(message);
                        
                        coreSession_.Send(message);
                        
                        ServerRequest:
                        
                        ServerRequestProcessor.PostprocessSend(message);
                        
                        if (ServerRequestProcessor.Completed)
                            ServerRequestProcessorDictionary_.Remove(key);
                        
                        goto Server;
                    }
                    
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
                    
                    State_222_9_ = new State_222_9(this);
                    State_224_13_ = new State_224_13(this);
                    State_233_9_ = new State_233_9(this);
                    State_251_13_ = new State_251_13(this);
                    State_0_ = new State_0(this);
                    
                    state_ = State_222_9_;
                    
                    if (session_.coreSession_.LogStates)
                        session_.coreSession_.LogState("Server : 222_9");
                }
                
                public bool Completed
                {
                    get { return state_ == State_0_; }
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
                    
                    public abstract void PreprocessSend(Message message);
                    
                    public abstract void PostprocessSend(Message message);
                    
                    public abstract void ProcessReceive(Message message);
                    
                    public abstract void ProcessDisconnect(List<ServerContext> contextList);
                    
                    protected ServerProcessor processor_;
                }
                
                class State_222_9 : State
                {
                    public State_222_9(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 222_9 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        if (Is.LoginRequest(message))
                        {
                            LoginRequest LoginRequest = Cast.LoginRequest(message);
                            
                            if (processor_.session_.event_ == null)
                            {
                                processor_.session_.Event_LoginRequest_222_9_LoginRequest_.LoginRequest_ = LoginRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_LoginRequest_222_9_LoginRequest_;
                            }
                            
                            processor_.state_ = processor_.State_224_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 224_13");
                            
                            return;
                        }
                        
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 222_9 : {0}", message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_224_13 : State
                {
                    public State_224_13(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.LoginReport(message))
                            return;
                        
                        if (Is.LoginReject(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 224_13 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.LoginReport(message))
                        {
                            processor_.state_ = processor_.State_233_9_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 233_9");
                            
                            return;
                        }
                        
                        if (Is.LoginReject(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 0");
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 224_13 : {0}", message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_233_9 : State
                {
                    public State_233_9(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.Report(message))
                            return;
                        
                        if (Is.Update(message))
                            return;
                        
                        if (Is.LogoutReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 233_9 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.Report(message))
                        {
                            processor_.state_ = processor_.State_233_9_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 233_9");
                            
                            return;
                        }
                        
                        if (Is.Update(message))
                        {
                            processor_.state_ = processor_.State_233_9_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 233_9");
                            
                            return;
                        }
                        
                        if (Is.LogoutReport(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 0");
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        if (Is.Request(message))
                        {
                            Request Request = Cast.Request(message);
                            
                            processor_.state_ = processor_.State_233_9_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 233_9");
                            
                            return;
                        }
                        
                        if (Is.LogoutRequest(message))
                        {
                            LogoutRequest LogoutRequest = Cast.LogoutRequest(message);
                            
                            if (processor_.session_.event_ == null)
                            {
                                processor_.session_.Event_LogoutRequest_233_9_LogoutRequest_.LogoutRequest_ = LogoutRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_LogoutRequest_233_9_LogoutRequest_;
                            }
                            
                            processor_.state_ = processor_.State_251_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 251_13");
                            
                            return;
                        }
                        
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 233_9 : {0}", message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_251_13 : State
                {
                    public State_251_13(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.Report(message))
                            return;
                        
                        if (Is.Update(message))
                            return;
                        
                        if (Is.LogoutReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 251_13 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.Report(message))
                        {
                            processor_.state_ = processor_.State_251_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 251_13");
                            
                            return;
                        }
                        
                        if (Is.Update(message))
                        {
                            processor_.state_ = processor_.State_251_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 251_13");
                            
                            return;
                        }
                        
                        if (Is.LogoutReport(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 0");
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 251_13 : {0}", message.Info.name));
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
                
                State_222_9 State_222_9_;
                State_224_13 State_224_13_;
                State_233_9 State_233_9_;
                State_251_13 State_251_13_;
                State_0 State_0_;
                
                State state_;
            }
            
            class ServerUpdateProcessor
            {
                public ServerUpdateProcessor(Session session, string id)
                {
                    session_ = session;
                    id_ = id;
                    
                    State_287_9_ = new State_287_9(this);
                    State_0_ = new State_0(this);
                    
                    state_ = State_287_9_;
                    
                    if (session_.coreSession_.LogStates)
                        session_.coreSession_.LogState("ServerUpdate({0}) : 287_9", id_);
                }
                
                public bool Completed
                {
                    get { return state_ == State_0_; }
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
                    public State(ServerUpdateProcessor processor)
                    {
                        processor_ = processor;
                    }
                    
                    public abstract void PreprocessSend(Message message);
                    
                    public abstract void PostprocessSend(Message message);
                    
                    public abstract void ProcessReceive(Message message);
                    
                    public abstract void ProcessDisconnect(List<ServerContext> contextList);
                    
                    protected ServerUpdateProcessor processor_;
                }
                
                class State_287_9 : State
                {
                    public State_287_9(ServerUpdateProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.AccountModelUpdate(message))
                            return;
                        
                        if (Is.BotModelUpdate(message))
                            return;
                        
                        if (Is.PackageModelUpdate(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerUpdate({2}) : 287_9 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.AccountModelUpdate(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerUpdate({0}) : 0", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.BotModelUpdate(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerUpdate({0}) : 0", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.PackageModelUpdate(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerUpdate({0}) : 0", processor_.id_);
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerUpdate({0}) : 287_9 : {1}", processor_.id_, message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_0 : State
                {
                    public State_0(ServerUpdateProcessor processor) : base(processor)
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
                string id_;
                
                State_287_9 State_287_9_;
                State_0 State_0_;
                
                State state_;
            }
            
            class ServerRequestProcessor
            {
                public ServerRequestProcessor(Session session, string id)
                {
                    session_ = session;
                    id_ = id;
                    
                    State_330_9_ = new State_330_9(this);
                    State_332_13_ = new State_332_13(this);
                    State_338_13_ = new State_338_13(this);
                    State_344_13_ = new State_344_13(this);
                    State_0_ = new State_0(this);
                    
                    state_ = State_330_9_;
                    
                    if (session_.coreSession_.LogStates)
                        session_.coreSession_.LogState("ServerRequest({0}) : 330_9", id_);
                }
                
                public bool Completed
                {
                    get { return state_ == State_0_; }
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
                    public State(ServerRequestProcessor processor)
                    {
                        processor_ = processor;
                    }
                    
                    public abstract void PreprocessSend(Message message);
                    
                    public abstract void PostprocessSend(Message message);
                    
                    public abstract void ProcessReceive(Message message);
                    
                    public abstract void ProcessDisconnect(List<ServerContext> contextList);
                    
                    protected ServerRequestProcessor processor_;
                }
                
                class State_330_9 : State
                {
                    public State_330_9(ServerRequestProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerRequest({2}) : 330_9 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
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
                                processor_.session_.Event_AccountListRequest_330_9_AccountListRequest_.AccountListRequest_ = AccountListRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_AccountListRequest_330_9_AccountListRequest_;
                            }
                            
                            processor_.state_ = processor_.State_332_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 332_13", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.PackageListRequest(message))
                        {
                            PackageListRequest PackageListRequest = Cast.PackageListRequest(message);
                            
                            if (processor_.session_.event_ == null)
                            {
                                processor_.session_.Event_PackageListRequest_330_9_PackageListRequest_.PackageListRequest_ = PackageListRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_PackageListRequest_330_9_PackageListRequest_;
                            }
                            
                            processor_.state_ = processor_.State_338_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 338_13", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.SubscribeRequest(message))
                        {
                            SubscribeRequest SubscribeRequest = Cast.SubscribeRequest(message);
                            
                            if (processor_.session_.event_ == null)
                            {
                                processor_.session_.Event_SubscribeRequest_330_9_SubscribeRequest_.SubscribeRequest_ = SubscribeRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_SubscribeRequest_330_9_SubscribeRequest_;
                            }
                            
                            processor_.state_ = processor_.State_344_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 344_13", processor_.id_);
                            
                            return;
                        }
                        
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerRequest({0}) : 330_9 : {1}", processor_.id_, message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_332_13 : State
                {
                    public State_332_13(ServerRequestProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.AccountListReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerRequest({2}) : 332_13 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.AccountListReport(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 0", processor_.id_);
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerRequest({0}) : 332_13 : {1}", processor_.id_, message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_338_13 : State
                {
                    public State_338_13(ServerRequestProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.PackageListReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerRequest({2}) : 338_13 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.PackageListReport(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 0", processor_.id_);
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerRequest({0}) : 338_13 : {1}", processor_.id_, message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_344_13 : State
                {
                    public State_344_13(ServerRequestProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.SubscribeReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerRequest({2}) : 344_13 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.SubscribeReport(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 0", processor_.id_);
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerRequest({0}) : 344_13 : {1}", processor_.id_, message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_0 : State
                {
                    public State_0(ServerRequestProcessor processor) : base(processor)
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
                string id_;
                
                State_330_9 State_330_9_;
                State_332_13 State_332_13_;
                State_338_13 State_338_13_;
                State_344_13 State_344_13_;
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
            
            class Event_LoginRequest_222_9_LoginRequest : Event
            {
                public Event_LoginRequest_222_9_LoginRequest(Session session) : base(session)
                {
                }
                
                public override void Dispatch()
                {
                    if (session_.coreSession_.LogEvents)
                        session_.coreSession_.LogEvent("OnLoginRequest({0})", LoginRequest_.ToString());
                    
                    if (session_.server_.listener_ != null)
                    {
                        try
                        {
                            session_.server_.listener_.OnLoginRequest(session_.server_, session_, LoginRequest_);
                        }
                        catch
                        {
                        }
                    }
                    
                    LoginRequest_ = new LoginRequest();
                }
                
                public LoginRequest LoginRequest_;
            }
            
            class Event_LogoutRequest_233_9_LogoutRequest : Event
            {
                public Event_LogoutRequest_233_9_LogoutRequest(Session session) : base(session)
                {
                }
                
                public override void Dispatch()
                {
                    if (session_.coreSession_.LogEvents)
                        session_.coreSession_.LogEvent("OnLogoutRequest({0})", LogoutRequest_.ToString());
                    
                    if (session_.server_.listener_ != null)
                    {
                        try
                        {
                            session_.server_.listener_.OnLogoutRequest(session_.server_, session_, LogoutRequest_);
                        }
                        catch
                        {
                        }
                    }
                    
                    LogoutRequest_ = new LogoutRequest();
                }
                
                public LogoutRequest LogoutRequest_;
            }
            
            class Event_AccountListRequest_330_9_AccountListRequest : Event
            {
                public Event_AccountListRequest_330_9_AccountListRequest(Session session) : base(session)
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
            
            class Event_PackageListRequest_330_9_PackageListRequest : Event
            {
                public Event_PackageListRequest_330_9_PackageListRequest(Session session) : base(session)
                {
                }
                
                public override void Dispatch()
                {
                    if (session_.coreSession_.LogEvents)
                        session_.coreSession_.LogEvent("OnPackageListRequest({0})", PackageListRequest_.ToString());
                    
                    if (session_.server_.listener_ != null)
                    {
                        try
                        {
                            session_.server_.listener_.OnPackageListRequest(session_.server_, session_, PackageListRequest_);
                        }
                        catch
                        {
                        }
                    }
                    
                    PackageListRequest_ = new PackageListRequest();
                }
                
                public PackageListRequest PackageListRequest_;
            }
            
            class Event_SubscribeRequest_330_9_SubscribeRequest : Event
            {
                public Event_SubscribeRequest_330_9_SubscribeRequest(Session session) : base(session)
                {
                }
                
                public override void Dispatch()
                {
                    if (session_.coreSession_.LogEvents)
                        session_.coreSession_.LogEvent("OnSubscribeRequest({0})", SubscribeRequest_.ToString());
                    
                    if (session_.server_.listener_ != null)
                    {
                        try
                        {
                            session_.server_.listener_.OnSubscribeRequest(session_.server_, session_, SubscribeRequest_);
                        }
                        catch
                        {
                        }
                    }
                    
                    SubscribeRequest_ = new SubscribeRequest();
                }
                
                public SubscribeRequest SubscribeRequest_;
            }
            
            internal void OnCoreConnect()
            {
                lock (stateMutex_)
                {
                    ServerProcessor_ = new ServerProcessor(this);
                    
                    ServerUpdateProcessorDictionary_ = new SortedDictionary<string, ServerUpdateProcessor>();
                    
                    ServerRequestProcessorDictionary_ = new SortedDictionary<string, ServerRequestProcessor>();
                    
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
                    foreach(var processor in ServerUpdateProcessorDictionary_)
                        processor.Value.ProcessDisconnect(contexList);
                    
                    ServerUpdateProcessorDictionary_ = null;
                    
                    foreach(var processor in ServerRequestProcessorDictionary_)
                        processor.Value.ProcessDisconnect(contexList);
                    
                    ServerRequestProcessorDictionary_ = null;
                    
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
                    
                    if (Is.Update(message))
                    {
                        Update Update = Cast.Update(message);
                        
                        string key = Update.Id;
                        ServerUpdateProcessor ServerUpdateProcessor;
                        
                        if (! ServerUpdateProcessorDictionary_.TryGetValue(key, out ServerUpdateProcessor))
                        {
                            ServerUpdateProcessor = new ServerUpdateProcessor(this, key);
                            ServerUpdateProcessorDictionary_.Add(key, ServerUpdateProcessor);
                        }
                        
                        ServerUpdateProcessor.ProcessReceive(message);
                        
                        ServerUpdate:
                        
                        if (ServerUpdateProcessor.Completed)
                            ServerUpdateProcessorDictionary_.Remove(key);
                        
                        goto Server;
                    }
                    
                    if (Is.Request(message))
                    {
                        Request Request = Cast.Request(message);
                        
                        string key = Request.Id;
                        ServerRequestProcessor ServerRequestProcessor;
                        
                        if (! ServerRequestProcessorDictionary_.TryGetValue(key, out ServerRequestProcessor))
                        {
                            ServerRequestProcessor = new ServerRequestProcessor(this, key);
                            ServerRequestProcessorDictionary_.Add(key, ServerRequestProcessor);
                        }
                        
                        ServerRequestProcessor.ProcessReceive(message);
                        
                        ServerRequest:
                        
                        if (ServerRequestProcessor.Completed)
                            ServerRequestProcessorDictionary_.Remove(key);
                        
                        goto Server;
                    }
                    
                    if (Is.Report(message))
                    {
                        Report Report = Cast.Report(message);
                        
                        string key = Report.RequestId;
                        ServerRequestProcessor ServerRequestProcessor;
                        
                        if (! ServerRequestProcessorDictionary_.TryGetValue(key, out ServerRequestProcessor))
                        {
                            ServerRequestProcessor = new ServerRequestProcessor(this, key);
                            ServerRequestProcessorDictionary_.Add(key, ServerRequestProcessor);
                        }
                        
                        ServerRequestProcessor.ProcessReceive(message);
                        
                        ServerRequest:
                        
                        if (ServerRequestProcessor.Completed)
                            ServerRequestProcessorDictionary_.Remove(key);
                        
                        goto Server;
                    }
                    
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
            SortedDictionary<string, ServerUpdateProcessor> ServerUpdateProcessorDictionary_;
            SortedDictionary<string, ServerRequestProcessor> ServerRequestProcessorDictionary_;
            
            Event_LoginRequest_222_9_LoginRequest Event_LoginRequest_222_9_LoginRequest_;
            Event_LogoutRequest_233_9_LogoutRequest Event_LogoutRequest_233_9_LogoutRequest_;
            Event_AccountListRequest_330_9_AccountListRequest Event_AccountListRequest_330_9_AccountListRequest_;
            Event_PackageListRequest_330_9_PackageListRequest Event_PackageListRequest_330_9_PackageListRequest_;
            Event_SubscribeRequest_330_9_SubscribeRequest Event_SubscribeRequest_330_9_SubscribeRequest_;
            
            Event event_;
        }
        
        class CoreServerListener : Core.ServerListener
        {
            public CoreServerListener(Server server)
            {
                server_ = server;
            }
            
            public override void OnConnect(Core.Server coreServer, Core.Server.Session coreSession)
            {
                Session session = new Session(server_, coreSession);
                
                lock (server_.stateMutex_)
                {
                    server_.sessionDictionary_.Add(session.Id, session);
                }
                
                coreSession.Data = session;
                
                session.OnCoreConnect();
            }
            
            public override void OnDisconnect(Core.Server coreServer, Core.Server.Session coreSession, string text)
            {
                Session session = ((Session) coreSession.Data);
                
                session.OnCoreDisconnect(text);
                
                coreSession.Data = null;
                
                lock (server_.stateMutex_)
                {
                    server_.sessionDictionary_.Remove(session.Id);
                }
            }
            
            public override void OnReceive(Core.Server coreServer, Core.Server.Session coreSession, Message message)
            {
                ((Session) coreSession.Data).OnCoreReceive(message);
            }
            
            public override void OnSend(Core.Server coreServer, Core.Server.Session coreSession)
            {
                ((Session) coreSession.Data).OnCoreSend();
            }
            
            Server server_;
        }
        
        Core.Server coreServer_;
        CoreServerListener coreServerListener_;
        
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
        
        public virtual void OnLoginRequest(Server server, Server.Session session, LoginRequest message)
        {
        }
        
        public virtual void OnLogoutRequest(Server server, Server.Session session, LogoutRequest message)
        {
        }
        
        public virtual void OnAccountListRequest(Server server, Server.Session session, AccountListRequest message)
        {
        }
        
        public virtual void OnPackageListRequest(Server server, Server.Session session, PackageListRequest message)
        {
        }
        
        public virtual void OnSubscribeRequest(Server server, Server.Session session, SubscribeRequest message)
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
