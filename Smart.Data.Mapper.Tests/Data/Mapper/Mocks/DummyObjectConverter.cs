namespace Smart.Data.Mapper.Mocks
{
    using System;

    using Smart.Converter;

    public class DummyObjectConverter : IObjectConverter
    {
        public bool CanConvert<T>(object value) => false;

        public bool CanConvert(object value, Type targetType) => false;

        public bool CanConvert(Type sourceType, Type targetType) => false;

        public T Convert<T>(object value) => default;

        public object Convert(object value, Type targetType) => null;

        public Func<object, object> CreateConverter(Type sourceType, Type targetType) => null;
    }
}
