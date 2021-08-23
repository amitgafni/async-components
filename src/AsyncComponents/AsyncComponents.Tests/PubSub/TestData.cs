using AsyncComponents.PubSub;
using System;
using System.Diagnostics.CodeAnalysis;

namespace AsyncComponents.Tests.PubSub
{
    internal class TestData : IEquatable<TestData>
    {
        public TestData()
        {
            Data = Guid.NewGuid();
        }
        public Guid Data { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as TestData);
        }

        public bool Equals([AllowNull] TestData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if(this.GetType() != other.GetType()) return false;
            return Data.Equals(other.Data);
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }
    }

    internal class OtherTestData : IEquatable<OtherTestData>
    {
        public OtherTestData()
        {
            Data = Guid.NewGuid();
        }
        public Guid Data { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as OtherTestData);
        }

        public bool Equals([AllowNull] OtherTestData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (this.GetType() != other.GetType()) return false;
            return Data.Equals(other.Data);
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }
    }

    internal class DerivedTestData : TestData
    {
        public DerivedTestData() : base()
        {

        }
    }
}