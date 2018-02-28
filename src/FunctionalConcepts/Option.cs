using System;

namespace FunctionalConcepts
{
    public struct Option<T> : IEquatable<Option<T>>
           where T : class
    {
        private readonly T _value;
        public T Value
        {
            get
            {
                if (HasNoValue)
                    throw new InvalidOperationException();

                return _value;
            }
        }

        public bool HasValue => _value != null;
        public bool HasNoValue => !HasValue;

        private Option(T value)
        {
            _value = value;
        }

        public static implicit operator Option<T>(T value)
        {
            return new Option<T>(value);
        }

        public static bool operator ==(Option<T> option, T value)
        {
            if (option.HasNoValue)
                return false;

            return option.Value.Equals(value);
        }

        public static bool operator !=(Option<T> option, T value)
        {
            return !(option == value);
        }

        public static bool operator ==(Option<T> first, Option<T> second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(Option<T> first, Option<T> second)
        {
            return !(first == second);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Option<T>))
                return false;

            var other = (Option<T>)obj;
            return Equals(other);
        }

        public bool Equals(Option<T> other)
        {
            if (HasNoValue && other.HasNoValue)
                return true;

            if (HasNoValue || other.HasNoValue)
                return false;

            return _value.Equals(other._value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            if (HasNoValue)
                return "No value";

            return Value.ToString();
        }

        public T Unwrap(T defaultValue = default(T))
        {
            if (HasValue)
                return Value;

            return defaultValue;
        }
    }
}
