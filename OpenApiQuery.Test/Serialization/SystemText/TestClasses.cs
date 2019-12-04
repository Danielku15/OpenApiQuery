namespace OpenApiQuery.Test.Serialization.SystemText
{
    public class SimpleClass
    {
        public int IntProp { get; set; }
        public string StringProp { get; set; }
        public double DoubleProp { get; set; }
    }

    public class ArrayWrapper<T>
    {
        public T[] Items { get; set; }
    }

    public class Base
    {
        public int BaseProp { get; set; }
    }

    public class Sub1 : Base
    {
        public byte SubProp { get; set; }
        public double Sub1Prop { get; set; }
    }

    public class Sub2 : Base
    {
        public sbyte SubProp { get; set; }
        public string Sub2Prop { get; set; }
    }

    public class SimpleNavigation
    {
        public SimpleClass Nav1 { get; set; }
        public SimpleClass Nav2 { get; set; }
        public SimpleClass Nav3 { get; set; }
    }

    public class CollectionNavigation
    {
        public SimpleClass[] Nav1 { get; set; }
        public SimpleClass[] Nav2 { get; set; }
        public SimpleClass[] Nav3 { get; set; }
    }
}
