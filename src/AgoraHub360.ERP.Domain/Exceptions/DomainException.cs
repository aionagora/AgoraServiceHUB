namespace AgoraHub360.ERP.Domain.Exceptions;

using System;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
