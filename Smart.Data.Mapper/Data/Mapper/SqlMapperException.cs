namespace Smart.Data.Mapper
{
    using System;

    public class SqlMapperException : Exception
    {
        public SqlMapperException()
        {
        }

        public SqlMapperException(string message)
            : base(message)
        {
        }

        public SqlMapperException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
