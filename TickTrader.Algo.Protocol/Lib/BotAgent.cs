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
            
            data_.SetInt(4, 7);
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
            
            data_.SetInt(4, 8);
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
            ConstructLoginRequest();
            ConstructLoginReport();
            ConstructLoginRejectReason();
            ConstructLoginReject();
            ConstructLogoutRequest();
            ConstructLogoutReason();
            ConstructLogoutReport();
            ConstructRequest();
            ConstructReport();
            ConstructPluginKey();
            ConstructAccountKey();
            ConstructPluginPermissions();
            ConstructBotState();
            ConstructBotModel();
            ConstructAccountModel();
            ConstructAccountListRequest();
            ConstructAccountListReport();
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
            InstanceId.type = FieldType.String;
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
            AccountListRequest.id = 7;
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
            AccountListReport.id = 8;
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
            BotAgent.AddMessageInfo(LoginRequest);
            BotAgent.AddMessageInfo(LoginReport);
            BotAgent.AddMessageInfo(LoginReject);
            BotAgent.AddMessageInfo(LogoutRequest);
            BotAgent.AddMessageInfo(LogoutReport);
            BotAgent.AddMessageInfo(Request);
            BotAgent.AddMessageInfo(Report);
            BotAgent.AddMessageInfo(AccountListRequest);
            BotAgent.AddMessageInfo(AccountListReport);
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
        public static GroupInfo PluginKey;
        public static GroupInfo AccountKey;
        public static GroupInfo PluginPermissions;
        public static EnumInfo BotState;
        public static GroupInfo BotModel;
        public static GroupInfo AccountModel;
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
            
            Event_LoginReport_103_13_LoginReport_ = new Event_LoginReport_103_13_LoginReport(this);
            Event_LoginReject_103_13_LoginReject_ = new Event_LoginReject_103_13_LoginReject(this);
            Event_LogoutReport_112_9_LogoutReport_ = new Event_LogoutReport_112_9_LogoutReport(this);
            Event_LogoutReport_126_13_LogoutReport_ = new Event_LogoutReport_126_13_LogoutReport(this);
            Event_AccountListReport_183_13_AccountListReport_ = new Event_AccountListReport_183_13_AccountListReport(this);
            
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
        
        public void Send(Message message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("Send({0})", message.ToString());
                
                ClientProcessor_.PreprocessSend(message);
                
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
                
                State_101_9_ = new State_101_9(this);
                State_103_13_ = new State_103_13(this);
                State_112_9_ = new State_112_9(this);
                State_126_13_ = new State_126_13(this);
                State_0_ = new State_0(this);
                
                state_ = State_101_9_;
                
                if (session_.coreSession_.LogStates)
                    session_.coreSession_.LogState("Client : 101_9");
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
            
            class State_101_9 : State
            {
                public State_101_9(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    processor_.State_103_13_.LoginRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_103_13_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("Client : 103_13");
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 101_9 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    if (Is.LoginRequest(message))
                        return;
                    
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 101_9 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                    if (Is.LoginRequest(message))
                    {
                        processor_.state_ = processor_.State_103_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 103_13");
                        
                        return;
                    }
                }
                
                public override void ProcessReceive(Message message)
                {
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 101_9 : {0}", message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            class State_103_13 : State
            {
                public State_103_13(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 103_13 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 103_13 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 103_13 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
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
                            processor_.session_.Event_LoginReport_103_13_LoginReport_.LoginRequestClientContext_ = LoginRequestClientContext_;
                            processor_.session_.Event_LoginReport_103_13_LoginReport_.LoginReport_ = LoginReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_LoginReport_103_13_LoginReport_;
                        }
                        
                        LoginRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_112_9_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 112_9");
                        
                        return;
                    }
                    
                    if (Is.LoginReject(message))
                    {
                        LoginReject LoginReject = Cast.LoginReject(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_LoginReject_103_13_LoginReject_.LoginRequestClientContext_ = LoginRequestClientContext_;
                            processor_.session_.Event_LoginReject_103_13_LoginReject_.LoginReject_ = LoginReject;
                            
                            processor_.session_.event_ = processor_.session_.Event_LoginReject_103_13_LoginReject_;
                        }
                        
                        LoginRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 0");
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 103_13 : {0}", message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (LoginRequestClientContext_ != null)
                        contextList.Add(LoginRequestClientContext_);
                }
                
                public LoginRequestClientContext LoginRequestClientContext_;
            }
            
            class State_112_9 : State
            {
                public State_112_9(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 112_9 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    processor_.State_126_13_.LogoutRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_126_13_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("Client : 126_13");
                }
                
                public override void PreprocessSend(Message message)
                {
                    if (Is.Request(message))
                        return;
                    
                    if (Is.LogoutRequest(message))
                        return;
                    
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 112_9 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                    if (Is.Request(message))
                    {
                        processor_.state_ = processor_.State_112_9_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 112_9");
                        
                        return;
                    }
                    
                    if (Is.LogoutRequest(message))
                    {
                        processor_.state_ = processor_.State_126_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 126_13");
                        
                        return;
                    }
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.Report(message))
                    {
                        Report Report = Cast.Report(message);
                        
                        processor_.state_ = processor_.State_112_9_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 112_9");
                        
                        return;
                    }
                    
                    if (Is.LogoutReport(message))
                    {
                        LogoutReport LogoutReport = Cast.LogoutReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_LogoutReport_112_9_LogoutReport_.LogoutReport_ = LogoutReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_LogoutReport_112_9_LogoutReport_;
                        }
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 0");
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 112_9 : {0}", message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            class State_126_13 : State
            {
                public State_126_13(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 126_13 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 126_13 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 126_13 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.Report(message))
                    {
                        Report Report = Cast.Report(message);
                        
                        processor_.state_ = processor_.State_126_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 126_13");
                        
                        return;
                    }
                    
                    if (Is.LogoutReport(message))
                    {
                        LogoutReport LogoutReport = Cast.LogoutReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_LogoutReport_126_13_LogoutReport_.LogoutRequestClientContext_ = LogoutRequestClientContext_;
                            processor_.session_.Event_LogoutReport_126_13_LogoutReport_.LogoutReport_ = LogoutReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_LogoutReport_126_13_LogoutReport_;
                        }
                        
                        LogoutRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 0");
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 126_13 : {0}", message.Info.name));
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
            
            State_101_9 State_101_9_;
            State_103_13 State_103_13_;
            State_112_9 State_112_9_;
            State_126_13 State_126_13_;
            State_0 State_0_;
            
            State state_;
        }
        
        class ClientRequestProcessor
        {
            public ClientRequestProcessor(ClientSession session, string id)
            {
                session_ = session;
                id_ = id;
                
                State_181_9_ = new State_181_9(this);
                State_183_13_ = new State_183_13(this);
                State_0_ = new State_0(this);
                
                state_ = State_181_9_;
                
                if (session_.coreSession_.LogStates)
                    session_.coreSession_.LogState("ClientRequest({0}) : 181_9", id_);
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
                public State(ClientRequestProcessor processor)
                {
                    processor_ = processor;
                }
                
                public abstract void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message);
                
                public abstract void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message);
                
                public abstract void PreprocessSend(Message message);
                
                public abstract void PostprocessSend(Message message);
                
                public abstract void ProcessReceive(Message message);
                
                public abstract void ProcessDisconnect(List<ClientContext> contextList);
                
                protected ClientRequestProcessor processor_;
            }
            
            class State_181_9 : State
            {
                public State_181_9(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    processor_.State_183_13_.AccountListRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_183_13_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("ClientRequest({0}) : 183_13", processor_.id_);
                }
                
                public override void PreprocessSend(Message message)
                {
                    if (Is.AccountListRequest(message))
                        return;
                    
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 181_9 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSend(Message message)
                {
                    if (Is.AccountListRequest(message))
                    {
                        processor_.state_ = processor_.State_183_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 183_13", processor_.id_);
                        
                        return;
                    }
                }
                
                public override void ProcessReceive(Message message)
                {
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientRequest({0}) : 181_9 : {1}", processor_.id_, message.Info.name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            class State_183_13 : State
            {
                public State_183_13(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 183_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 183_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
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
                            processor_.session_.Event_AccountListReport_183_13_AccountListReport_.AccountListRequestClientContext_ = AccountListRequestClientContext_;
                            processor_.session_.Event_AccountListReport_183_13_AccountListReport_.AccountListReport_ = AccountListReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_AccountListReport_183_13_AccountListReport_;
                        }
                        
                        AccountListRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientRequest({0}) : 183_13 : {1}", processor_.id_, message.Info.name));
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
                public State_0(ClientRequestProcessor processor) : base(processor)
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
            string id_;
            
            State_181_9 State_181_9_;
            State_183_13 State_183_13_;
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
        
        class Event_LoginReport_103_13_LoginReport : Event
        {
            public Event_LoginReport_103_13_LoginReport(ClientSession clientSession) : base(clientSession)
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
        
        class Event_LoginReject_103_13_LoginReject : Event
        {
            public Event_LoginReject_103_13_LoginReject(ClientSession clientSession) : base(clientSession)
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
        
        class Event_LogoutReport_112_9_LogoutReport : Event
        {
            public Event_LogoutReport_112_9_LogoutReport(ClientSession clientSession) : base(clientSession)
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
        
        class Event_LogoutReport_126_13_LogoutReport : Event
        {
            public Event_LogoutReport_126_13_LogoutReport(ClientSession clientSession) : base(clientSession)
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
        
        class Event_AccountListReport_183_13_AccountListReport : Event
        {
            public Event_AccountListReport_183_13_AccountListReport(ClientSession clientSession) : base(clientSession)
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
        
        void OnCoreReceive(Core.ClientSession clientSession, Message message)
        {
            lock (stateMutex_)
            {
                ClientProcessor_.ProcessReceive(message);
                
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
        SortedDictionary<string, ClientRequestProcessor> ClientRequestProcessorDictionary_;
        
        Event_LoginReport_103_13_LoginReport Event_LoginReport_103_13_LoginReport_;
        Event_LoginReject_103_13_LoginReject Event_LoginReject_103_13_LoginReject_;
        Event_LogoutReport_112_9_LogoutReport Event_LogoutReport_112_9_LogoutReport_;
        Event_LogoutReport_126_13_LogoutReport Event_LogoutReport_126_13_LogoutReport_;
        Event_AccountListReport_183_13_AccountListReport Event_AccountListReport_183_13_AccountListReport_;
        
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
                
                Event_LoginRequest_139_9_LoginRequest_ = new Event_LoginRequest_139_9_LoginRequest(this);
                Event_LogoutRequest_150_9_LogoutRequest_ = new Event_LogoutRequest_150_9_LogoutRequest(this);
                Event_AccountListRequest_195_9_AccountListRequest_ = new Event_AccountListRequest_195_9_AccountListRequest(this);
                
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
                    
                    State_139_9_ = new State_139_9(this);
                    State_141_13_ = new State_141_13(this);
                    State_150_9_ = new State_150_9(this);
                    State_164_13_ = new State_164_13(this);
                    State_0_ = new State_0(this);
                    
                    state_ = State_139_9_;
                    
                    if (session_.coreSession_.LogStates)
                        session_.coreSession_.LogState("Server : 139_9");
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
                
                class State_139_9 : State
                {
                    public State_139_9(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 139_9 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
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
                                processor_.session_.Event_LoginRequest_139_9_LoginRequest_.LoginRequest_ = LoginRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_LoginRequest_139_9_LoginRequest_;
                            }
                            
                            processor_.state_ = processor_.State_141_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 141_13");
                            
                            return;
                        }
                        
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 139_9 : {0}", message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_141_13 : State
                {
                    public State_141_13(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.LoginReport(message))
                            return;
                        
                        if (Is.LoginReject(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 141_13 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.LoginReport(message))
                        {
                            processor_.state_ = processor_.State_150_9_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 150_9");
                            
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
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 141_13 : {0}", message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_150_9 : State
                {
                    public State_150_9(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.Report(message))
                            return;
                        
                        if (Is.LogoutReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 150_9 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.Report(message))
                        {
                            processor_.state_ = processor_.State_150_9_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 150_9");
                            
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
                            
                            processor_.state_ = processor_.State_150_9_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 150_9");
                            
                            return;
                        }
                        
                        if (Is.LogoutRequest(message))
                        {
                            LogoutRequest LogoutRequest = Cast.LogoutRequest(message);
                            
                            if (processor_.session_.event_ == null)
                            {
                                processor_.session_.Event_LogoutRequest_150_9_LogoutRequest_.LogoutRequest_ = LogoutRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_LogoutRequest_150_9_LogoutRequest_;
                            }
                            
                            processor_.state_ = processor_.State_164_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 164_13");
                            
                            return;
                        }
                        
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 150_9 : {0}", message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_164_13 : State
                {
                    public State_164_13(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.Report(message))
                            return;
                        
                        if (Is.LogoutReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 164_13 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.Report(message))
                        {
                            processor_.state_ = processor_.State_164_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 164_13");
                            
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
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 164_13 : {0}", message.Info.name));
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
                
                State_139_9 State_139_9_;
                State_141_13 State_141_13_;
                State_150_9 State_150_9_;
                State_164_13 State_164_13_;
                State_0 State_0_;
                
                State state_;
            }
            
            class ServerRequestProcessor
            {
                public ServerRequestProcessor(Session session, string id)
                {
                    session_ = session;
                    id_ = id;
                    
                    State_195_9_ = new State_195_9(this);
                    State_197_13_ = new State_197_13(this);
                    State_0_ = new State_0(this);
                    
                    state_ = State_195_9_;
                    
                    if (session_.coreSession_.LogStates)
                        session_.coreSession_.LogState("ServerRequest({0}) : 195_9", id_);
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
                
                class State_195_9 : State
                {
                    public State_195_9(ServerRequestProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerRequest({2}) : 195_9 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
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
                                processor_.session_.Event_AccountListRequest_195_9_AccountListRequest_.AccountListRequest_ = AccountListRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_AccountListRequest_195_9_AccountListRequest_;
                            }
                            
                            processor_.state_ = processor_.State_197_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 197_13", processor_.id_);
                            
                            return;
                        }
                        
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerRequest({0}) : 195_9 : {1}", processor_.id_, message.Info.name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_197_13 : State
                {
                    public State_197_13(ServerRequestProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.AccountListReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerRequest({2}) : 197_13 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.name));
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
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerRequest({0}) : 197_13 : {1}", processor_.id_, message.Info.name));
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
                
                State_195_9 State_195_9_;
                State_197_13 State_197_13_;
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
            
            class Event_LoginRequest_139_9_LoginRequest : Event
            {
                public Event_LoginRequest_139_9_LoginRequest(Session session) : base(session)
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
            
            class Event_LogoutRequest_150_9_LogoutRequest : Event
            {
                public Event_LogoutRequest_150_9_LogoutRequest(Session session) : base(session)
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
            
            class Event_AccountListRequest_195_9_AccountListRequest : Event
            {
                public Event_AccountListRequest_195_9_AccountListRequest(Session session) : base(session)
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
            SortedDictionary<string, ServerRequestProcessor> ServerRequestProcessorDictionary_;
            
            Event_LoginRequest_139_9_LoginRequest Event_LoginRequest_139_9_LoginRequest_;
            Event_LogoutRequest_150_9_LogoutRequest Event_LogoutRequest_150_9_LogoutRequest_;
            Event_AccountListRequest_195_9_AccountListRequest Event_AccountListRequest_195_9_AccountListRequest_;
            
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
        
        public virtual void OnLoginRequest(Server server, Server.Session session, LoginRequest message)
        {
        }
        
        public virtual void OnLogoutRequest(Server server, Server.Session session, LogoutRequest message)
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
